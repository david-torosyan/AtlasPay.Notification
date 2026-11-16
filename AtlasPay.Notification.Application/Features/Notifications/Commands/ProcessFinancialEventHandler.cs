using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;

using NotificationEntity = AtlasPay.Notification.Domain.Entities.Notification;

namespace AtlasPay.Notification.Application.Features.Notifications.Commands;

public sealed class ProcessFinancialEventHandler(
    INotificationRepository notificationRepository,
    IProcessedEventRepository processedEventRepository,
    INotificationPreferenceReadRepository preferenceRepository,
    IAccountNotificationPreferenceReadRepository accountNotificationPreferenceRepository,
    INotificationChannelReadRepository channelRepository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        ProcessFinancialEventCommand command,
        CancellationToken ct = default)
    {
        if (await processedEventRepository.ExistsAsync(command.EventId, ct))
            return;

        var activeChannels = (await channelRepository.GetAllAsync(ct))
            .Where(x => x.IsActive)
            .Select(x => x.Channel)
            .ToHashSet();

        foreach (var recipient in command.Recipients)
        {
            var notification = new NotificationEntity(
                command.EventId,
                recipient.UserId,
                recipient.AccountId,
                recipient.NotificationType,
                recipient.Payload);

            foreach (var channel in activeChannels)
            {
                bool? enabled = null;

                if (recipient.AccountId is Guid accountId)
                {
                    enabled = await accountNotificationPreferenceRepository
                        .GetEnabledAsync(
                            recipient.UserId,
                            accountId,
                            recipient.NotificationType,
                            channel,
                            ct);
                }

                enabled ??= await preferenceRepository.GetEnabledAsync(
                    recipient.UserId,
                    recipient.NotificationType,
                    channel,
                    ct);

                if (enabled == true)
                    notification.AddDelivery(channel);
            }

            notificationRepository.Add(notification);
        }

        processedEventRepository.Add(
            new ProcessedEvent(command.EventId, command.EventType));

        await unitOfWork.SaveChangesAsync(ct);
    }
}