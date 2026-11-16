using AtlasPay.Notification.Api.Contracts.AccountNotificationPreferences;
using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Commands;
using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Notification.Api.Controllers;

[ApiController]
[Route("api/account-notification-preferences")]
public sealed class AccountNotificationPreferencesController : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> SetAsync(
        [FromBody] SetAccountNotificationPreferenceRequest request,
        [FromServices] SetAccountNotificationPreferenceHandler handler,
        CancellationToken ct)
    {
        var command = new SetAccountNotificationPreferenceCommand(
            request.UserId,
            request.AccountId,
            request.NotificationType,
            request.Channel,
            request.Enabled);

        await handler.HandleAsync(command, ct);

        return NoContent();
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetAsync(
        Guid userId,
        [FromServices] GetAccountNotificationPreferencesHandler handler,
        CancellationToken ct)
    {
        var query = new GetAccountNotificationPreferencesQuery(userId);

        var preferences = await handler.HandleAsync(query, ct);

        return Ok(preferences);
    }
}