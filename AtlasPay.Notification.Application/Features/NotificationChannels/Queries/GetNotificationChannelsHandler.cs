using AtlasPay.Notification.Application.Abstractions.Persistence;

namespace AtlasPay.Notification.Application.Features.NotificationChannels.Queries;

public sealed class GetNotificationChannelsHandler(
    INotificationChannelReadRepository repository)
{
    public Task<List<NotificationChannelDto>> HandleAsync(
        GetNotificationChannelsQuery query,
        CancellationToken ct = default)
    {
        return repository.GetAllAsync(ct);
    }
}