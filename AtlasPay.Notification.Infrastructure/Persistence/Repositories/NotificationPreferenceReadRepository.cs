using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public class NotificationPreferenceReadRepository(
    NotificationDbContext dbContext)
    : INotificationPreferenceReadRepository
{
    public Task<List<NotificationPreferenceDto>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
    {
        return dbContext.NotificationPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new NotificationPreferenceDto(
                x.NotificationType,
                x.Channel,
                x.Enabled))
            .ToListAsync(ct);
    }

    public Task<bool?> GetEnabledAsync(
        Guid userId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default)
            => dbContext.NotificationPreferences
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.NotificationType == notificationType &&
                    x.Channel == channel)
                .Select(x => (bool?)x.Enabled)
                .FirstOrDefaultAsync(ct);
}
