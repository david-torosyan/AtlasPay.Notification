
using AtlasPay.Notification.Api.Contracts.NotificationChannels;
using AtlasPay.Notification.Application.Features.NotificationChannels.Commands;
using AtlasPay.Notification.Application.Features.NotificationChannels.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AtlasPay.Notification.Api.Controllers;

[ApiController]
[Route("api/notification-channels")]
public sealed class NotificationChannelsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NotificationChannelDto>>> GetAll(
        [FromServices] GetNotificationChannelsHandler handler,
        CancellationToken ct)
    {
        var channels = await handler.HandleAsync(
            new GetNotificationChannelsQuery(), ct);

        return Ok(channels);
    }

    [HttpPut]
    public async Task<IActionResult> SetStatus(
        [FromBody] SetNotificationChannelStatusRequest request,
        [FromServices] SetNotificationChannelStatusHandler handler,
        CancellationToken ct)
    {
        var command = new SetNotificationChannelStatusCommand(
            request.Channel,
            request.IsActive);

        await handler.HandleAsync(command, ct);

        return NoContent();
    }
}
