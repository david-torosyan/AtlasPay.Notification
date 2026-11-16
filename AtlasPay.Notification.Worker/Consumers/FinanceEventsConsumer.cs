using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class FinanceEventsConsumer(
    IConfiguration configuration,
    ILogger<FinanceEventsConsumer> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken),
            stoppingToken);
    }

    private void Consume(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],

            GroupId = configuration["Kafka:GroupId"],

            AutoOffsetReset = AutoOffsetReset.Earliest,

            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumer =
          new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(
            configuration["Kafka:Topic"] ?? "finance.events");

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(ct);

                    var eventType = result.Message.Headers
                        .FirstOrDefault(x => x.Key == "event-type");

                    var type = eventType is null
                        ? null
                        : Encoding.UTF8.GetString(
                            eventType.GetValueBytes());

                    switch (type)
                    {
                        case FinanceEventTypes.DepositCompletedV2:
                            {
                                var @event =
                                    JsonSerializer.Deserialize<DepositCompletedV2>(
                                        result.Message.Value);

                                logger.LogInformation(
                                    "Received deposit event {EventId}",
                                    @event?.EventId);

                                break;
                            }

                        case FinanceEventTypes.WithdrawalCompletedV2:
                            {
                                var @event =
                                    JsonSerializer.Deserialize<WithdrawalCompletedV2>(
                                        result.Message.Value);

                                logger.LogInformation(
                                    "Received withdrawal event {EventId}",
                                    @event?.EventId);

                                break;
                            }

                        case FinanceEventTypes.TransferCompletedV2:
                            {
                                var @event =
                                    JsonSerializer.Deserialize<TransferCompletedV2>(
                                        result.Message.Value);

                                logger.LogInformation(
                                    "Received transfer event {EventId}",
                                    @event?.EventId);

                                break;
                            }

                        case FinanceEventTypes.TransferReversedV2:
                            {
                                var @event =
                                    JsonSerializer.Deserialize<TransferReversedV2>(
                                        result.Message.Value);

                                logger.LogInformation(
                                    "Received reversal event {EventId}",
                                    @event?.EventId);

                                break;
                            }

                        default:
                            logger.LogWarning(
                                "Unsupported Finance event type {EventType}",
                                type);

                            // Do not acknowledge unhandled events yet.
                            throw new InvalidOperationException(
                                $"Unsupported Finance event type: {type}");
                    }

                    // Do not commit offsets yet.
                    // We have only deserialized the event,
                    // not persisted its notification.
                    throw new NotImplementedException(
                        "Notification processing is not connected yet.");
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex,
                        "Kafka consumption failed.");

                    throw;
                }
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