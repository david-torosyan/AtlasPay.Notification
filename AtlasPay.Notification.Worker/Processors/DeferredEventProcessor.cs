
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Worker.Exceptions;

namespace AtlasPay.Notification.Worker.Processors;

public sealed class DeferredEventProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<DeferredEventProcessor> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(1);

    private readonly string _workerId = $"worker-{Guid.NewGuid()}";
    private readonly int _batchSize = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        try
        {
            do
            {
                await ProcessDueEventsAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
    }

    private async Task ProcessDueEventsAsync(CancellationToken ct)
    {
        IReadOnlyList<(Guid EventId, Guid ClaimId)> claims;

        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IDeferredEventRepository>();

            var events = await repository.ClaimDueEventsAsync(
                _workerId,
                _batchSize,
                LeaseDuration,
                ct);

            claims = events
                .Select(x => (x.EventId, x.ClaimId!.Value))
                .ToArray();
        }

        foreach (var (eventId, claimId) in claims)
        {
            ct.ThrowIfCancellationRequested();

            await ProcessEventAsync(eventId, claimId, ct);
        }
    }

    private async Task ProcessEventAsync(
        Guid eventId,
        Guid claimId,
        CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IDeferredEventRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var processor = scope.ServiceProvider
            .GetRequiredService<FinanceEventProcessor>();

        var deferredEvent = await repository.GetByIdAsync(eventId, ct);

        if (deferredEvent is null ||
            deferredEvent.ClaimId != claimId ||
            deferredEvent.LockedUntil <= DateTime.UtcNow)
        {
            logger.LogWarning(
                "Deferred event {EventId} is no longer owned by this claim",
                eventId);

            return;
        }

        try
        {
            await using var transaction =
                await unitOfWork.BeginTransactionAsync(ct);

            await processor.ProcessAsync(
                deferredEvent.EventType,
                deferredEvent.Payload,
                ct);

            var completed = await repository.CompleteAsync(
                eventId,
                claimId,
                ct);

            if (!completed)
                throw new DeferredEventClaimLostException(eventId);

            await unitOfWork.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (DeferredEventClaimLostException ex)
        {
            logger.LogWarning(
                "Claim lost for deferred event {EventId}",
                ex.EventId);
        }
        catch (Exception ex)
        {
            var scheduled = await repository.ScheduleRetryAsync(
                eventId,
                claimId,
                ex.Message,
                ct);

            if (!scheduled)
            {
                logger.LogWarning(
                    "Cannot schedule retry for event {EventId}: claim lost",
                    eventId);

                return;
            }

            if (ex is CustomerMappingNotFoundException)
            {
                logger.LogWarning(
                    "Customer mapping still missing for event {EventId}",
                    eventId);
            }
            else
            {
                logger.LogError(
                    ex,
                    "Failed to process deferred event {EventId}",
                    eventId);
            }
        }
    }
}
