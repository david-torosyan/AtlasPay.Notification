using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetAsync(
        DeliveryChannel channel,
        CancellationToken ct = default);
}