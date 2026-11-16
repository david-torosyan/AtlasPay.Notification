using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationPreferenceReadRepository
{
    Task<List<NotificationPreferenceDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);
}
