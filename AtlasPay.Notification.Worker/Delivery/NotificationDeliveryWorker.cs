namespace AtlasPay.Notification.Worker.Delivery;

public sealed class NotificationDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDeliveryWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Notification delivery worker started");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        while (await timer.WaitForNextTickAsync(stoppingToken)
                  && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope =
                    scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider
                    .GetRequiredService<NotificationDeliveryProcessor>();

                await processor.ProcessNextAsync(stoppingToken);
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
}