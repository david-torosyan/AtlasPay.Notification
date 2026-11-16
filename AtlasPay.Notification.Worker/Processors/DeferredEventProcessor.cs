
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Worker.Exceptions;

namespace AtlasPay.Notification.Worker.Processors;

public sealed class DeferredEventProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<DeferredEventProcessor> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval =
        TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
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
        IReadOnlyList<Guid> eventIds;

        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IDeferredEventRepository>();

            var events = await repository.GetDueAsync(
                DateTime.UtcNow, 20, ct);

            eventIds = events.Select(x => x.EventId).ToArray();
        }

        foreach (var eventId in eventIds)
        {
            ct.ThrowIfCancellationRequested();

            await ProcessEventAsync(eventId, ct);
        }
    }

    private async Task ProcessEventAsync(
        Guid eventId,
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

        if (deferredEvent is null)
            return;

        try
        {
            await processor.ProcessAsync(
                deferredEvent.EventType,
                deferredEvent.Payload,
                ct);

            repository.Remove(deferredEvent);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation(
                "Successfully processed deferred event {EventId}",
                eventId);
        }
        catch (CustomerMappingNotFoundException ex)
        {
            deferredEvent.ScheduleRetry(
                ex.Message,
                TimeSpan.FromMinutes(1));

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogWarning(
                "Customer mapping still missing for event {EventId}",
                eventId);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process deferred event {EventId}",
                eventId);

            // Do not immediately retry the same failing event.
            // A later poll can retry after this delay.
            deferredEvent.ScheduleRetry(
                ex.Message,
                TimeSpan.FromMinutes(5));

            await unitOfWork.SaveChangesAsync(ct);
        }
    }
}
