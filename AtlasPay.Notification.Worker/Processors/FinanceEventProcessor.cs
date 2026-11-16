
using System.Text.Json;
using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Worker.Mappers;

namespace AtlasPay.Notification.Worker.Processors;

public sealed class FinanceEventProcessor(
    FinanceEventMapper mapper,
    ProcessFinancialEventHandler handler)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task ProcessAsync(
        string eventType,
        string payload,
        CancellationToken ct = default)
    {
        ProcessFinancialEventCommand command = eventType switch
        {
            FinanceEventTypes.DepositCompletedV2 =>
                await mapper.MapAsync(
                    Deserialize<DepositCompletedV2>(payload), ct),

            FinanceEventTypes.WithdrawalCompletedV2 =>
                await mapper.MapAsync(
                    Deserialize<WithdrawalCompletedV2>(payload), ct),

            FinanceEventTypes.TransferCompletedV2 =>
                await mapper.MapAsync(
                    Deserialize<TransferCompletedV2>(payload), ct),

            FinanceEventTypes.TransferReversedV2 =>
                await mapper.MapAsync(
                    Deserialize<TransferReversedV2>(payload), ct),

            _ => throw new NotSupportedException(
                $"Unsupported event type: {eventType}")
        };

        await handler.HandleAsync(command, ct);
    }

    private static T Deserialize<T>(string payload)
        where T : class
        => JsonSerializer.Deserialize<T>(payload, JsonOptions)
            ?? throw new JsonException(
                $"Invalid {typeof(T).Name} payload.");
}
