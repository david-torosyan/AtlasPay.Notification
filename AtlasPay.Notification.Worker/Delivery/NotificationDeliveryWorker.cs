using AtlasPay.Notification.Application.Abstractions.Persistence;

namespace AtlasPay.Notification.Worker.Delivery;

public sealed class NotificationDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDeliveryWorker> logger)
    : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxConcurrency = 5;
    private const int MaxAttempts = 5;

    private static readonly TimeSpan LeaseDuration
        = TimeSpan.FromMinutes(2);

    private static readonly TimeSpan PollInterval
        = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Notification delivery worker started");

        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var deliveries = await ClaimBatchAsync(stoppingToken);

                if (deliveries == null || deliveries.Count == 0)
                    continue;

                logger.LogInformation(
                    "Claimed {Count} notification deliveries",
                    deliveries.Count);

                await Parallel.ForEachAsync(
                    deliveries,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = MaxConcurrency,
                        CancellationToken = stoppingToken
                    },
                    async (delivery, ct) =>
                    {
                        try
                        {
                            await using var scope =
                                scopeFactory.CreateAsyncScope();

                            var processor = scope.ServiceProvider
                                .GetRequiredService<
                                    NotificationDeliveryProcessor>();

                            await processor.ProcessAsync(
                                delivery, ct);
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
                                "Unexpected error processing delivery {DeliveryId}",
                                delivery.Id);
                        }
                    });
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected notification delivery worker error");
            }
        }

        logger.LogInformation(
            "Notification delivery worker stopped");
    }

    private async Task<IReadOnlyList<ClaimedDelivery>?> ClaimBatchAsync(
        CancellationToken ct)
    {
        await using var scope =
            scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<INotificationDeliveryRepository>();

        await repository.FailExhaustedClaimsAsync(
            MaxAttempts, ct);

        return await repository.ClaimBatchAsync(
            BatchSize,
            Guid.NewGuid(),
            DateTime.UtcNow.Add(LeaseDuration),
            ct);
    }
}