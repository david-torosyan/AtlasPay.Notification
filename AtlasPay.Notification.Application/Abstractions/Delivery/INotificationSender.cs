using AtlasPay.Notification.Domain.Enums;
using NotificationEntity = AtlasPay.Notification.Domain.Entities.Notification;

namespace AtlasPay.Notification.Application.Abstractions.Delivery;

public interface INotificationSender
{
    DeliveryChannel Channel { get; }

    Task SendAsync(
        NotificationEntity notification,
        CancellationToken ct);
}