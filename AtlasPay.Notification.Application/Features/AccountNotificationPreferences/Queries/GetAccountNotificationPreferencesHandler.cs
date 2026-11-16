using AtlasPay.Notification.Application.Abstractions.Persistence;

namespace AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;

public sealed class GetAccountNotificationPreferencesHandler(
    IAccountNotificationPreferenceReadRepository repository)
{
    public Task<IReadOnlyList<AccountNotificationPreferenceResponse>> HandleAsync(
        GetAccountNotificationPreferencesQuery query,
        CancellationToken ct = default)
        => repository.GetByUserIdAsync(query.UserId, ct);
}