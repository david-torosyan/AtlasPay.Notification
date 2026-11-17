using NotificationEntity =
    AtlasPay.Notification.Domain.Entities.Notification;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public sealed class ClaimedDelivery
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public Guid LockToken { get; set; }
    public int AttemptCount { get; set; }
}

public interface INotificationDeliveryRepository
{
    Task<ClaimedDelivery?> ClaimNextAsync(
        Guid lockToken,
        DateTime lockedUntil,
        CancellationToken ct);

    Task<NotificationEntity?> GetNotificationAsync(
        Guid notificationId,
        CancellationToken ct);

    Task<bool> MarkAsSentAsync(
        Guid deliveryId,
        Guid lockToken,
        CancellationToken ct);

    Task<bool> ScheduleRetryAsync(
        Guid deliveryId,
        Guid lockToken,
        DateTime nextRetryAt,
        string error,
        CancellationToken ct);

    Task<bool> MarkAsFailedAsync(
        Guid deliveryId,
        Guid lockToken,
        string error,
        CancellationToken ct);
}