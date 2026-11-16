
using System.Text;
using System.Text.Json;
using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Worker.Mappers;
using Confluent.Kafka;

namespace AtlasPay.Notification.Worker.Consumers;

public sealed class FinanceEventsConsumer(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<FinanceEventsConsumer> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);

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

                // Retry the same message until processing succeeds.
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        await ProcessAsync(result, ct);

                        consumer.Commit(result);

                        logger.LogInformation(
                            "Committed Kafka event at {Offset}",
                            result.TopicPartitionOffset);

                        break;
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
                            "Failed to process Kafka event at {Offset}",
                            result.TopicPartitionOffset);

                        await Task.Delay(TimeSpan.FromSeconds(5), ct);
                    }
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
