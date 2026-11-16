using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationPreferenceReadRepository
{
    Task<List<NotificationPreferenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<bool?> GetEnabledAsync(
        Guid userId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default);
}
