using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationDeliveryRepository(
    NotificationDbContext dbContext)
    : INotificationDeliveryRepository
{
    public async Task<IReadOnlyList<ClaimedDelivery>> ClaimBatchAsync(
    int batchSize,
    Guid lockToken,
    DateTime lockedUntil,
    CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var now = DateTime.UtcNow;

        var query = dbContext.Database
            .SqlQuery<ClaimedDelivery>(
                $"""
            WITH candidate AS (
                SELECT "Id"
                FROM notification_deliveries
                WHERE
                    (
                        "Status" = 'Pending'
                        AND "AttemptCount" < 5
                        AND (
                            "NextRetryAt" IS NULL
                            OR "NextRetryAt" <= {now}
                        )
                    )
                    OR (
                        "Status" = 'Processing'
                        AND "AttemptCount" < 5
                        AND "LockedUntil" <= {now}
                    )
                ORDER BY "CreatedAt", "Id"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
            )
            UPDATE notification_deliveries AS d
            SET
                "Status" = 'Processing',
                "LockToken" = {lockToken},
                "LockedUntil" = {lockedUntil},
                "AttemptCount" = d."AttemptCount" + 1,
                "UpdatedAt" = {now}
            FROM candidate
            WHERE d."Id" = candidate."Id"
            RETURNING
                d."Id" AS "Id",
                d."NotificationId" AS "NotificationId",
                d."Channel" AS "Channel",
                d."LockToken" AS "LockToken",
                d."AttemptCount" AS "AttemptCount"
            """);

        var deliveries = new List<ClaimedDelivery>();

        await foreach (var delivery in
            query.AsAsyncEnumerable().WithCancellation(ct))
        {
            deliveries.Add(delivery);
        }

        return deliveries;
    }

    public Task<NotificationEntity?> GetNotificationAsync(
        Guid notificationId,
        CancellationToken ct) =>
        dbContext.Notifications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == notificationId, ct);

    public async Task<bool> MarkAsSentAsync(
        Guid deliveryId,
        Guid lockToken,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var affected = await OwnedDelivery(deliveryId, lockToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, DeliveryStatus.Sent)
                .SetProperty(x => x.SentAt, now)
                .SetProperty(x => x.LockToken, (Guid?)null)
                .SetProperty(x => x.LockedUntil, (DateTime?)null)
                .SetProperty(x => x.NextRetryAt, (DateTime?)null)
                .SetProperty(x => x.LastError, (string?)null)
                .SetProperty(x => x.UpdatedAt, now),
                ct);

        return affected == 1;
    }

    public async Task<bool> ScheduleRetryAsync(
        Guid deliveryId,
        Guid lockToken,
        DateTime nextRetryAt,
        string error,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var affected = await OwnedDelivery(deliveryId, lockToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, DeliveryStatus.Pending)
                .SetProperty(x => x.NextRetryAt, nextRetryAt)
                .SetProperty(x => x.LastError, error)
                .SetProperty(x => x.LockToken, (Guid?)null)
                .SetProperty(x => x.LockedUntil, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, now),
                ct);

        return affected == 1;
    }

    public async Task<bool> MarkAsFailedAsync(
        Guid deliveryId,
        Guid lockToken,
        string error,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var affected = await OwnedDelivery(deliveryId, lockToken)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, DeliveryStatus.Failed)
                .SetProperty(x => x.LastError, error)
                .SetProperty(x => x.NextRetryAt, (DateTime?)null)
                .SetProperty(x => x.LockToken, (Guid?)null)
                .SetProperty(x => x.LockedUntil, (DateTime?)null)
                .SetProperty(x => x.UpdatedAt, now),
                ct);

        return affected == 1;
    }

    private IQueryable<Domain.Entities.NotificationDelivery>
        OwnedDelivery(Guid deliveryId, Guid lockToken)
            => dbContext.NotificationDeliveries.Where(
                x => x.Id == deliveryId
                     && x.Status == DeliveryStatus.Processing
                     && x.LockToken == lockToken
                     && x.LockedUntil > DateTime.UtcNow);

    public Task<int> FailExhaustedClaimsAsync(
        int maxAttempts,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        return dbContext.NotificationDeliveries
            .Where(x =>
                x.Status == DeliveryStatus.Processing &&
                x.AttemptCount >= maxAttempts &&
                x.LockedUntil <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    DeliveryStatus.Failed)
                .SetProperty(
                    x => x.LastError,
                    "Delivery claim expired after maximum attempts.")
                .SetProperty(
                    x => x.LockToken,
                    (Guid?)null)
                .SetProperty(
                    x => x.LockedUntil,
                    (DateTime?)null)
                .SetProperty(
                    x => x.NextRetryAt,
                    (DateTime?)null)
                .SetProperty(
                    x => x.UpdatedAt,
                    now),
                ct);
    }
}