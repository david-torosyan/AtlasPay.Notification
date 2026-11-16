using AtlasPay.Notification.Domain.Entities;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface IProcessedEventRepository
{
    Task<bool> ExistsAsync(
        Guid eventId,
        CancellationToken ct = default);

    void Add(
        ProcessedEvent processedEvent);
}
