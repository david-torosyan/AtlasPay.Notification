
using AtlasPay.Notification.Application.Abstractions.Persistence;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(
    NotificationDbContext dbContext)
    : INotificationRepository
{
    public void Add(NotificationEntity notification)
        => dbContext.Notifications.Add(notification);
}