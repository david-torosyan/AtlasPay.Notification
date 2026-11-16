using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface IAccountNotificationPreferenceReadRepository
{
    Task<IReadOnlyList<AccountNotificationPreferenceResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);
}