using AtlasPay.Notification.Application.Abstractions.Delivery;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Worker.Delivery;

public sealed class NotificationDeliveryProcessor(
    INotificationDeliveryRepository repository,
    IEnumerable<INotificationSender> senders,
    ILogger<NotificationDeliveryProcessor> logger)
{
    private const int MaxAttempts = 5;

    private static readonly TimeSpan SendTimeout =
        TimeSpan.FromSeconds(30);

    public async Task ProcessAsync(
        ClaimedDelivery delivery,
        CancellationToken ct)
    {
        try
        {
            var notification = await repository.GetNotificationAsync(
                delivery.NotificationId, ct);

            if (notification is null)
                throw new InvalidOperationException(
                    $"Notification {delivery.NotificationId} not found.");

            var channel = Enum.Parse<DeliveryChannel>(
                    delivery.Channel);

            var sender = senders.Single(
                x => x.Channel == channel);

            using var timeoutCts =
                CancellationTokenSource.CreateLinkedTokenSource(ct);

            timeoutCts.CancelAfter(SendTimeout);

            await sender.SendAsync(
                  notification,
                  timeoutCts.Token);

            var updated = await repository.MarkAsSentAsync(
                 delivery.Id,
                 delivery.LockToken,
                 ct);

            if (!updated)
            {
                logger.LogWarning(
                    "Cannot mark delivery {DeliveryId} as sent: claim lost",
                    delivery.Id);
                return;
            }

            logger.LogInformation(
                "Notification delivery {DeliveryId} sent successfully",
                delivery.Id);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Notification delivery {DeliveryId} failed",
                delivery.Id);

            var error = ex.Message;

            if (error.Length > 1000)
                error = error[..1000];

            bool updated;

            if (delivery.AttemptCount >= MaxAttempts)
            {
                updated = await repository.MarkAsFailedAsync(
                    delivery.Id,
                    delivery.LockToken,
                    error,
                    ct);
            }
            else
            {
                var delay = TimeSpan.FromSeconds(
                    Math.Pow(2, delivery.AttemptCount) * 15);

                updated = await repository.ScheduleRetryAsync(
                    delivery.Id,
                    delivery.LockToken,
                    DateTime.UtcNow.Add(delay),
                    error,
                    ct);
            }

            if (!updated)
            {
                logger.LogWarning(
                    "Cannot finalize delivery {DeliveryId}: claim lost",
                    delivery.Id);
            }
        }
    }
}