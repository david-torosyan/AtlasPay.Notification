using AtlasPay.Contracts.Events.Identity.V1;
using AtlasPay.Notification.Application.Features.CustomerUserMappings.Commands.RegisterCustomerUser;
using AtlasPay.Notification.Worker.Configuration;
using AtlasPay.Notification.Worker.Kafka;
using AtlasPay.Notification.Worker.Publishers;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class IdentityEventsConsumer(
    KafkaConsumerFactory consumerFactory,
    IServiceScopeFactory scopeFactory,
    DeadLetterPublisher deadLetterPublisher,
    IOptions<KafkaSettings> options,
    ILogger<IdentityEventsConsumer> logger)
    : BackgroundService
{
    private const string ConsumerName = "Identity";

    private readonly int _maxRetries = options.Value.MaxRetries;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);

    private async Task ConsumeAsync(CancellationToken ct)
    {
        using var consumer = consumerFactory.Create(ConsumerName);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = consumer.Consume(ct);

                await ProcessWithRetriesAsync(result, ct);

                consumer.Commit(result);

                logger.LogInformation(
                    "Committed identity Kafka offset {Offset}",
                    result.TopicPartitionOffset);
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

    private async Task ProcessWithRetriesAsync(
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        for (var attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await ProcessAsync(result, ct);
                return;
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Invalid identity event at {Offset}. Sending to DLT.",
                    result.TopicPartitionOffset);

                await deadLetterPublisher.PublishAsync(
                    ConsumerName, result, ex, ct);

                return;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(
                    ex,
                    "Invalid identity event data at {Offset}. Sending to DLT.",
                    result.TopicPartitionOffset);

                await deadLetterPublisher.PublishAsync(
                    ConsumerName, result, ex, ct);

                return;
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Identity event processing failed. Attempt {Attempt}/{MaxRetries}",
                    attempt,
                    _maxRetries);

                if (attempt == _maxRetries)
                    throw;

                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    ct);
            }
        }
    }

    private async Task ProcessAsync(
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        var @event = JsonSerializer.Deserialize<IdentityUserRegisteredV1>(
            result.Message.Value,
            JsonOptions)
            ?? throw new JsonException("Invalid identity event.");

        if (@event.EventId == Guid.Empty)
            throw new ArgumentException("EventId is required.");

        await using var scope = scopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<RegisterCustomerUserHandler>();

        await handler.HandleAsync(
            new RegisterCustomerUserCommand(
                @event.CustomerId,
                @event.UserId,
                @event.Email),
            ct);

        logger.LogInformation(
            "Processed identity registration event {EventId} for customer {CustomerId}",
            @event.EventId,
            @event.CustomerId);
    }
}
