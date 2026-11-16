using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;
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
}
