using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class AccountNotificationPreferenceRepository(
    NotificationDbContext dbContext)
    : IAccountNotificationPreferenceRepository
{
    public Task<AccountNotificationPreference?> GetAsync(
        Guid userId,
        Guid accountId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default)
            => dbContext.AccountNotificationPreferences
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.AccountId == accountId &&
                    x.NotificationType == notificationType &&
                    x.Channel == channel, ct);

    public void Add(AccountNotificationPreference preference)
            => dbContext.AccountNotificationPreferences.Add(preference);
}