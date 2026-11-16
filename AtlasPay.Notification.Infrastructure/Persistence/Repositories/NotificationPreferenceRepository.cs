
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationPreferenceRepository(
    NotificationDbContext dbContext)
    : INotificationPreferenceRepository
{
    public Task AddAsync(
        NotificationPreference preference,
        CancellationToken ct = default)
    {
        dbContext.NotificationPreferences.Add(preference);
        return Task.CompletedTask;
    }

    public async Task<NotificationPreference?> GetAsync(
        Guid userId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default)
    {
        return await dbContext.NotificationPreferences
                        .FirstOrDefaultAsync(
                            x => x.UserId == userId &&
                                 x.NotificationType == notificationType &&
                                 x.Channel == channel,
                            ct);
    }
}