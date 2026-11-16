using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;
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
}