using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public class NotificationDelivery
{
    private NotificationDelivery() { } // For EF Core

    public Guid Id { get; private set; }
    public Guid NotificationId { get; private set; }

    public DeliveryChannel Channel { get; private set; }
    public DeliveryStatus Status { get; private set; }

    public int AttemptCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public Guid? LockToken { get; private set; }

    public DateTime? SentAt { get; private set; }
    public string? LastError { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public NotificationDelivery(
        Guid notificationId,
        DeliveryChannel channel)
    {
        if (notificationId == Guid.Empty)
            throw new ArgumentException(
                "NotificationId is required.");

        if (!Enum.IsDefined(channel))
            throw new ArgumentOutOfRangeException(
                nameof(channel));

        Id = Guid.NewGuid();
        NotificationId = notificationId;
        Channel = channel;
        Status = DeliveryStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void MarkAsProcessing(
        Guid lockToken,
        DateTime lockedUntil)
    {
        if (Status != DeliveryStatus.Pending)
            throw new InvalidOperationException(
                "Only pending deliveries can be claimed.");

        if (lockToken == Guid.Empty)
            throw new ArgumentException(
                "Lock token is required.");

        if (lockedUntil <= DateTime.UtcNow)
            throw new ArgumentException(
                "Lock expiration must be in the future.");

        Status = DeliveryStatus.Processing;
        LockToken = lockToken;
        LockedUntil = lockedUntil;
        NextRetryAt = null;

        AttemptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSent(Guid lockToken)
    {
        EnsureLeaseOwner(lockToken);

        Status = DeliveryStatus.Sent;
        SentAt = DateTime.UtcNow;
        LockedUntil = null;
        LockToken = null;
        LastError = null;
        UpdatedAt = SentAt.Value;
    }

    public void ScheduleRetry(
        Guid lockToken,
        DateTime nextRetryAt,
        string error)
    {
        EnsureLeaseOwner(lockToken);

        if (nextRetryAt <= DateTime.UtcNow)
            throw new ArgumentException(
                "Retry time must be in the future.");

        Status = DeliveryStatus.Pending;
        NextRetryAt = nextRetryAt;
        LastError = error;
        LockedUntil = null;
        LockToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(
        Guid lockToken,
        string error)
    {
        EnsureLeaseOwner(lockToken);

        Status = DeliveryStatus.Failed;
        LastError = error;
        NextRetryAt = null;
        LockedUntil = null;
        LockToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureLeaseOwner(Guid lockToken)
    {
        if (Status != DeliveryStatus.Processing ||
            LockToken != lockToken ||
            LockedUntil is null ||
            LockedUntil <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Delivery is not owned by an active lease.");
        }
    }
}