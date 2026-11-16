
using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetAsync(
        Guid userId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        NotificationPreference preference,
        CancellationToken cancellationToken = default);
}
