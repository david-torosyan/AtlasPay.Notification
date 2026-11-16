
using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Worker.Mappers;
using AtlasPay.Notification.Worker.Publishers;
using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class FinanceEventsConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    DeadLetterPublisher deadLetterPublisher,
    ILogger<FinanceEventsConsumer> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);

    private readonly int _maxRetries =
        Math.Max(1, configuration.GetValue("Kafka:MaxRetries", 3));

    private async Task ConsumeAsync(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .Build();

        consumer.Subscribe(configuration["Kafka:Topic"] ?? "finance.events");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = consumer.Consume(ct);

                await ProcessWithRetriesAsync(result, ct);

                consumer.Commit(result);

                logger.LogInformation(
                    "Committed Kafka offset {Offset}",
                    result.TopicPartitionOffset);
            }
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
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
            catch (Exception ex) when (
                ex is JsonException or NotSupportedException)
            {
                logger.LogError(
                    ex,
                    "Invalid Kafka event at {Offset}. Sending to DLT.",
                    result.TopicPartitionOffset);

                await deadLetterPublisher.PublishAsync(
                    result, ex, ct);

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

    private async Task ProcessAsync(
        ConsumeResult<string, string> result,
        CancellationToken ct)
    {
        var eventTypeHeader = result.Message.Headers?
            .FirstOrDefault(x => x.Key == "event-type");

        var eventType = eventTypeHeader is null
            ? null
            : Encoding.UTF8.GetString(eventTypeHeader.GetValueBytes());

        await using var scope = scopeFactory.CreateAsyncScope();

        var mapper = scope.ServiceProvider
            .GetRequiredService<FinanceEventMapper>();

        var handler = scope.ServiceProvider
            .GetRequiredService<ProcessFinancialEventHandler>();

        switch (eventType)
        {
            case FinanceEventTypes.DepositCompletedV2:
                {
                    var financialEvent =
                        JsonSerializer.Deserialize<DepositCompletedV2>(
                            result.Message.Value,
                            JsonOptions)
                        ?? throw new JsonException("Invalid deposit event.");

                    if (financialEvent.EventId == Guid.Empty ||
                        financialEvent.CustomerId == Guid.Empty ||
                        financialEvent.AccountId == Guid.Empty)
                    {
                        throw new JsonException(
                            "DepositCompletedV2 contains invalid identifiers.");
                    }

                    var command = await mapper.MapAsync(financialEvent, ct);

                    await handler.HandleAsync(command, ct);

                    break;
                }
            case FinanceEventTypes.WithdrawalCompletedV2:
                {
                    var financialEvent =
                        JsonSerializer.Deserialize<WithdrawalCompletedV2>(
                            result.Message.Value,
                            JsonOptions)
                        ?? throw new JsonException("Invalid withdrawal event.");

                    var command = await mapper.MapAsync(financialEvent, ct);

                    await handler.HandleAsync(command, ct);
                    break;
                }
            case FinanceEventTypes.TransferCompletedV2:
                {
                    var financialEvent =
                        JsonSerializer.Deserialize<TransferCompletedV2>(
                            result.Message.Value,
                            JsonOptions)
                        ?? throw new JsonException("Invalid transfer event.");

                    var command = await mapper.MapAsync(financialEvent, ct);

                    await handler.HandleAsync(command, ct);

                    break;
                }
            case FinanceEventTypes.TransferReversedV2:
                {
                    var financialEvent =
                        JsonSerializer.Deserialize<TransferReversedV2>(
                            result.Message.Value,
                            JsonOptions)
                        ?? throw new JsonException("Invalid reversal event.");

                    var command = await mapper.MapAsync(financialEvent, ct);

                    await handler.HandleAsync(command, ct);

                    break;
                }
            default:
                throw new NotSupportedException(
                    $"Unsupported Finance event type: {eventType}");
        }
    }
}
