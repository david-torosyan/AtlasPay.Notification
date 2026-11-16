using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class AccountNotificationPreferenceReadRepository(
    NotificationDbContext dbContext)
    : IAccountNotificationPreferenceReadRepository
{
    public async Task<IReadOnlyList<AccountNotificationPreferenceResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
        => await dbContext.AccountNotificationPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new AccountNotificationPreferenceResponse(
                x.AccountId,
                x.NotificationType,
                x.Channel,
                x.Enabled))
            .ToListAsync(ct);

    public Task<bool?> GetEnabledAsync(
        Guid userId,
        Guid accountId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default)
            => dbContext.AccountNotificationPreferences
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.AccountId == accountId &&
                    x.NotificationType == notificationType &&
                    x.Channel == channel)
                .Select(x => (bool?)x.Enabled)
                .FirstOrDefaultAsync(ct);
}