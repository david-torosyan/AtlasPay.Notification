using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;

namespace AtlasPay.Notification.Application.Features.NotificationPreferences.Commands;

public sealed class SetNotificationPreferenceHandler(
    INotificationPreferenceRepository repository,
    INotificationChannelRepository channelRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
       SetNotificationPreferenceCommand command,
       CancellationToken ct = default)
    {
        if (command.UserId == Guid.Empty)
            throw new ArgumentException(
                "UserId is required.", nameof(command.UserId));

        var channel = await channelRepository.GetAsync(
                command.Channel, ct);
        if (channel is null || !channel.IsActive)
            throw new KeyNotFoundException(
                $"Channel {command.Channel} is not supported.");

        var preference = await repository.GetAsync(
            command.UserId,
            command.NotificationType,
            command.Channel,
            ct);

        if (preference is null)
        {
            preference = new NotificationPreference(
                command.UserId,
                command.NotificationType,
                command.Channel,
                command.Enabled);

            await repository.AddAsync(preference, ct);
        }
        else
        {
            preference.SetEnabled(command.Enabled);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
