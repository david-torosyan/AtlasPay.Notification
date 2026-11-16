using AtlasPay.Notification.Application.Features.NotificationChannels.Queries;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface INotificationChannelReadRepository
{
    Task<List<NotificationChannelDto>> GetAllAsync(
        CancellationToken ct = default);
}