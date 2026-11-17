namespace AtlasPay.Notification.Domain.Entities;

public sealed class DeferredEvent
{
    private DeferredEvent() { }

    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;

    public int AttemptCount { get; private set; }
    public DateTime NextRetryAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? LastError { get; private set; }
    public string? LockedBy { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    public Guid? ClaimId { get; private set; } // For deleteing atomicly

    public DeferredEvent(
        Guid eventId,
        string eventType,
        string payload)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId cannot be empty.");

        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        EventId = eventId;
        EventType = eventType;
        Payload = payload;
        AttemptCount = 0;
        CreatedAt = DateTime.UtcNow;
        NextRetryAt = CreatedAt;
    }

    public void ScheduleRetry(string error, TimeSpan delay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay));

        AttemptCount++;
        LastError = error;
        NextRetryAt = DateTime.UtcNow.Add(delay);
    }

    public void Claim(
        string workerId,
        Guid claimId,
        DateTime lockedUntil)
    {
        LockedBy = workerId;
        ClaimId = claimId;
        LockedUntil = lockedUntil;
    }
}