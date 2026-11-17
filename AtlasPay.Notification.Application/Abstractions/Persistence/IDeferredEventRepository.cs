using AtlasPay.Notification.Domain.Entities;

public interface IDeferredEventRepository
{
    Task<bool> ExistsAsync(
        Guid eventId,
        CancellationToken ct = default);

    Task<IReadOnlyList<DeferredEvent>> GetDueAsync(
        DateTime now,
        int batchSize,
        CancellationToken ct = default);

    Task<DeferredEvent?> GetByIdAsync(
        Guid eventId,
        CancellationToken ct = default);

    void Add(DeferredEvent deferredEvent);

    Task<bool> CompleteAsync(
        Guid eventId,
        Guid claimId,
        CancellationToken ct = default);

    Task<IReadOnlyList<DeferredEvent>> ClaimDueEventsAsync(
        string workerId,
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken ct = default);

    Task<bool> ScheduleRetryAsync(
        Guid eventId,
        Guid claimId,
        string error,
        CancellationToken ct = default);
}