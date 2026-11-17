using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class DeferredEventRepository(
    NotificationDbContext dbContext)
    : IDeferredEventRepository
{
    public Task<bool> ExistsAsync(
        Guid eventId,
        CancellationToken ct = default)
        => dbContext.DeferredEvents
            .AnyAsync(x => x.EventId == eventId, ct);

    public void Add(DeferredEvent deferredEvent)
        => dbContext.DeferredEvents.Add(deferredEvent);

    public async Task<IReadOnlyList<DeferredEvent>> GetDueAsync(
        DateTime now,
        int batchSize,
        CancellationToken ct = default)
            => await dbContext.DeferredEvents
                .Where(x => x.NextRetryAt <= now)
                .OrderBy(x => x.NextRetryAt)
                .Take(batchSize)
                .ToListAsync(ct);

    public async Task<bool> CompleteAsync(
        Guid eventId,
        Guid claimId,
        CancellationToken ct = default)
    {
        var effectedRows = await dbContext.DeferredEvents
            .Where(x =>
                x.EventId == eventId &&
                x.ClaimId == claimId &&
                x.LockedUntil > DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);

        return effectedRows == 1;
    }

    public async Task<bool> ScheduleRetryAsync(
        Guid eventId,
        Guid claimId,
        string error,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var affectedRows = await dbContext.Database
            .ExecuteSqlInterpolatedAsync($"""
            UPDATE public.deferred_events
            SET
                "AttemptCount" = "AttemptCount" + 1,
                "NextRetryAt" = {now} + make_interval(
                    secs => LEAST(
                        1800,
                        (power(2::numeric, LEAST("AttemptCount", 5)) * 60)::integer
                    )
                ),
                "LastError" = {error},
                "LockedBy" = NULL,
                "LockedUntil" = NULL,
                "ClaimId" = NULL
            WHERE "EventId" = {eventId}
              AND "ClaimId" = {claimId}
              AND "LockedUntil" > {now}
            """, ct);

        return affectedRows == 1;
    }


    public Task<DeferredEvent?> GetByIdAsync
        (Guid eventId, CancellationToken ct = default)
        => dbContext.DeferredEvents
            .FirstOrDefaultAsync(x => x.EventId == eventId, ct);

    public async Task<IReadOnlyList<DeferredEvent>> ClaimDueEventsAsync(
        string workerId,
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var lockedUntil = now.Add(leaseDuration);

        await using var transaction
            = await dbContext.Database.BeginTransactionAsync(ct);

        var events = await dbContext.DeferredEvents
            .FromSqlInterpolated($"""
                SELECT * 
                FROM public.deferred_events 
                WHERE "NextRetryAt" <= {now}
                    AND (
                        "LockedUntil" IS NULL
                        OR "LockedUntil" <= {now}
                    )
                ORDER BY "NextRetryAt", "CreatedAt"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
            """)
            .AsTracking()
            .ToListAsync(ct);

        foreach (var deferredEvent in events)
        {
            deferredEvent.Claim(workerId, Guid.NewGuid(), lockedUntil);
        }

        await dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return events;
    }
}