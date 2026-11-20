
using AtlasPay.Notification.Api.Contracts.NotificationPreferences;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Commands;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Notification.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notification-preferences")]
public sealed class NotificationPreferencesController : ControllerBase
{
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> SetPreference(
        [FromBody] SetNotificationPreferenceRequest request,
        [FromServices] SetNotificationPreferenceHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new SetNotificationPreferenceCommand(
            request.UserId,
            request.NotificationType,
            request.Channel,
            request.Enabled);

        await handler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<List<NotificationPreferenceDto>>> GetByUserId(
        Guid userId,
        [FromServices] GetNotificationPreferencesHandler handler,
        CancellationToken ct)
    {
        var query = new GetNotificationPreferencesQuery(userId);

        var preferences = await handler.HandleAsync(query, ct);

        return Ok(preferences);
    }

}
