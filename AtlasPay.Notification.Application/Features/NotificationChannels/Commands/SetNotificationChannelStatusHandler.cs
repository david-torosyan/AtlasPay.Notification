using AtlasPay.Notification.Application.Abstractions.Persistence;

namespace AtlasPay.Notification.Application.Features.NotificationChannels.Commands;

public sealed class SetNotificationChannelStatusHandler(
    INotificationChannelRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        SetNotificationChannelStatusCommand command,
        CancellationToken ct = default)
    {
        var channel = await repository.GetAsync(command.Channel, ct)
            ?? throw new KeyNotFoundException(
                $"Channel {command.Channel} not found.");

        if (command.IsActive)
            channel.Activate();
        else
            channel.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}