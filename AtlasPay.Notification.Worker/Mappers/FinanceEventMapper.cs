using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Domain.Enums;
using System.Text.Json;

namespace AtlasPay.Notification.Worker.Mappers;

public sealed class FinanceEventMapper(
    ICustomerUserMappingReadRepository customerUserRepository)
{
    public async Task<ProcessFinancialEventCommand> MapAsync(
        DepositCompletedV2 @event,
        CancellationToken ct = default)
    {
        var userId = await customerUserRepository.GetUserIdAsync(
            @event.CustomerId, ct);

        if (userId is null)
            throw new InvalidOperationException(
                $"No user mapping found for customer {@event.CustomerId}.");

        var recipient = new NotificationRecipient(
            UserId: userId.Value,
            AccountId: @event.AccountId,
            NotificationType: NotificationType.MoneyDeposited,
            Payload: JsonSerializer.Serialize(new
            {
                @event.DepositId,
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        return new ProcessFinancialEventCommand(
            @event.EventId,
            FinanceEventTypes.DepositCompletedV2,
            [recipient]);
    }
}