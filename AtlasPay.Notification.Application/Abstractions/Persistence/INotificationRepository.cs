using NotificationEntity = AtlasPay.Notification.Domain.Entities.Notification;
namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    void Add(NotificationEntity notification);
}
