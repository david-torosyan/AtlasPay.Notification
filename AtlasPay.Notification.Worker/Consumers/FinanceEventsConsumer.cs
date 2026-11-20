
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Exceptions;
using AtlasPay.Notification.Application.Features.DeferredEvents.Commands;
using AtlasPay.Notification.Worker.Configuration;
using AtlasPay.Notification.Worker.Exceptions;
using AtlasPay.Notification.Worker.Kafka;
using AtlasPay.Notification.Worker.Processors;
using AtlasPay.Notification.Worker.Publishers;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class FinanceEventsConsumer(
    KafkaConsumerFactory consumerFactory,
    IServiceScopeFactory scopeFactory,
    DeadLetterPublisher deadLetterPublisher,
    IOptions<KafkaSettings> options,
    ILogger<FinanceEventsConsumer> logger)
    : BackgroundService
{
    private readonly int _maxRetries = options.Value.MaxRetries;
    private readonly string _consumerName = "Finance";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);

    private async Task ConsumeAsync(CancellationToken ct)
    {
        using var consumer = consumerFactory.Create(_consumerName);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = consumer.Consume(ct);

                try
                {
                    await ProcessWithRetriesAsync(result, ct);
                }
                catch (NotificationAlreadyExistsException)
                {
                    logger.LogInformation(
                        "Notification already exists at Kafka offset {Offset}",
                        result.TopicPartitionOffset);
                }

                consumer.Commit(result);

                logger.LogInformation(
                    "Committed Kafka offset {Offset}",
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
            catch (NotificationAlreadyExistsException)
            {
                // Duplicate is not a transient failure.
                throw;
            }
            catch (CustomerMappingNotFoundException ex)
            {
                logger.LogWarning(
                    ex,
                    "Customer {CustomerId} mapping missing. Deferring event.",
                    ex.CustomerId);

                await DeferEventAsync(result, ct);
                return;
            }
            catch (Exception ex) when (
                ex is JsonException or NotSupportedException)
            {
                logger.LogError(
                    ex,
                    "Invalid Kafka event at {Offset}. Sending to DLT.",
                    result.TopicPartitionOffset);

                await deadLetterPublisher.PublishAsync(_consumerName, result, ex, ct);
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
                    "Processing failed. Attempt {Attempt}/{MaxRetries}",
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

    private async Task DeferEventAsync(
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        using var document =
            JsonDocument.Parse(result.Message.Value);

        var eventId = document.RootElement
            .GetProperty("eventId")
            .GetGuid();

        var eventTypeHeader = result.Message.Headers?
            .FirstOrDefault(x => x.Key == "event-type");

        var eventType = eventTypeHeader is null
            ? throw new JsonException("Missing event-type header.")
            : Encoding.UTF8.GetString(
                eventTypeHeader.GetValueBytes());

        await using var scope = scopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<DeferFinancialEventHandler>();

        await handler.HandleAsync(
            new DeferFinancialEventCommand(
                EventId: eventId,
                EventType: eventType,
                Payload: result.Message.Value),
            ct);

        logger.LogInformation(
            "Deferred financial event {EventId}",
            eventId);
    }

    private async Task ProcessAsync(
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        var eventTypeHeader = result.Message.Headers?
            .FirstOrDefault(x => x.Key == "event-type");

        var eventType = eventTypeHeader is null
            ? throw new JsonException("Missing event-type header.")
            : Encoding.UTF8.GetString(
                eventTypeHeader.GetValueBytes());

        await using var scope = scopeFactory.CreateAsyncScope();

        var processor = scope.ServiceProvider
            .GetRequiredService<FinanceEventProcessor>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        await processor.ProcessAsync(
            eventType,
            result.Message.Value,
            ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
