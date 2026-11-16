using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.NotificationChannels.Queries;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public class NotificationChannelReadRepository(
    NotificationDbContext dbContext)
    : INotificationChannelReadRepository
{
    public Task<List<NotificationChannelDto>> GetAllAsync(CancellationToken ct = default)
    {
        return dbContext.NotificationChannels
            .AsNoTracking()
            .OrderBy(x => x.Channel)
            .Select(x => new NotificationChannelDto(
                x.Channel,
                x.IsActive))
            .ToListAsync(ct);
    }
}
