using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationChannelRepository(
    NotificationDbContext dbContext)
    : INotificationChannelRepository
{
    public Task<NotificationChannel?> GetAsync(
        DeliveryChannel channel,
        CancellationToken ct = default)
    {
        return dbContext.NotificationChannels
            .FirstOrDefaultAsync(x => x.Channel == channel, ct);
    }
}