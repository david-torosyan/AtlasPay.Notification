using AtlasPay.Contracts.Events.Identity;
using AtlasPay.Contracts.Events.Identity.V1;
using AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;
using AtlasPay.Notification.Worker.Kafka;
using Confluent.Kafka;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class IdentityEventsConsumer(
    KafkaConsumerFactory consumerFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<IdentityEventsConsumer> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);
    private readonly string _consumerName = "Identity";

    private async Task ConsumeAsync(CancellationToken ct)
    {
        using var consumer = consumerFactory.Create(_consumerName);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = consumer.Consume(ct);

                var @event = JsonSerializer.Deserialize<IdentityUserRegisteredV1>(
                    result.Message.Value,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException("Invalid identity event.");

                await using var scope = scopeFactory.CreateAsyncScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<RegisterCustomerUserHandler>();

                await handler.HandleAsync(
                    new RegisterCustomerUserCommand(
                        @event.CustomerId,
                        @event.UserId),
                    ct);

                consumer.Commit(result);

                logger.LogInformation(
                    "Processed identity event {EventId}",
                    @event.EventId);
            }
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        finally
        {
            consumer.Close();
        }
    }
}
