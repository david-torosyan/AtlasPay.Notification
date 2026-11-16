using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;

public sealed class GetNotificationPreferencesHandler(
    INotificationPreferenceReadRepository repository)
{
    public Task<List<NotificationPreferenceDto>> HandleAsync(
        GetNotificationPreferencesQuery query,
        CancellationToken ct = default)
    {
        return repository.GetByUserIdAsync(query.UserId, ct);
    }
}