
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;

using NotificationEntity = AtlasPay.Notification.Domain.Entities.Notification;

namespace AtlasPay.Notification.Application.Features.Notifications.Commands;

public sealed class ProcessFinancialEventHandler(
    INotificationRepository notificationRepository,
    IProcessedEventRepository processedEventRepository,
    INotificationPreferenceReadRepository preferenceRepository,
    INotificationChannelReadRepository channelRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        ProcessFinancialEventCommand command,
        CancellationToken ct = default)
    {
        if (await processedEventRepository.ExistsAsync(
                command.EventId, ct))
            return;

        var activeChannels = (await channelRepository.GetAllAsync(ct))
            .Where(x => x.IsActive)
            .Select(x => x.Channel)
            .ToHashSet();

        foreach (var recipient in command.Recipients)
        {
            var preferences = await preferenceRepository.GetByUserIdAsync(
                    recipient.UserId, ct);

            var channels = preferences
                .Where(x =>
                    x.NotificationType == recipient.Type &&
                    x.Enabled &&
                    activeChannels.Contains(x.Channel))
                .Select(x => x.Channel)
                .Distinct()
                .ToList();

            if (channels.Count == 0)
                continue;

            var notification = new NotificationEntity(
                    command.EventId,
                    recipient.UserId,
                    recipient.Type,
                    recipient.Payload);

            foreach (var channel in channels)
                notification.AddDelivery(channel);

            notificationRepository.Add(notification);
        }

        processedEventRepository.Add(
            new ProcessedEvent(
                command.EventId,
                command.EventType));

        await unitOfWork.SaveChangesAsync(ct);
    }
}
