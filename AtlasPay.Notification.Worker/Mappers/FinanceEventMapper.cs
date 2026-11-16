using AtlasPay.Contracts.Events.Finance;
using AtlasPay.Contracts.Events.Finance.V2;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Features.Notifications.Commands;
using AtlasPay.Notification.Domain.Enums;
using AtlasPay.Notification.Worker.Exceptions;
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
            throw new CustomerMappingNotFoundException(@event.CustomerId);

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

    public async Task<ProcessFinancialEventCommand> MapAsync(
        WithdrawalCompletedV2 @event,
        CancellationToken ct = default)
    {
        var userId = await customerUserRepository.GetUserIdAsync(
            @event.CustomerId, ct);

        if (userId is null)
            throw new CustomerMappingNotFoundException(@event.CustomerId);

        var recipient = new NotificationRecipient(
            UserId: userId.Value,
            AccountId: @event.AccountId,
            NotificationType: NotificationType.MoneyWithdrawn,
            Payload: JsonSerializer.Serialize(new
            {
                @event.WithdrawalId,
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        return new ProcessFinancialEventCommand(
            @event.EventId,
            FinanceEventTypes.WithdrawalCompletedV2,
            [recipient]);
    }

    public async Task<ProcessFinancialEventCommand> MapAsync(
    TransferCompletedV2 @event,
    CancellationToken ct = default)
    {
        var fromUserId = await customerUserRepository.GetUserIdAsync(
            @event.FromCustomerId, ct);

        var toUserId = await customerUserRepository.GetUserIdAsync(
            @event.ToCustomerId, ct);

        if (fromUserId is null || toUserId is null)
            throw new InvalidOperationException(
                "Unable to resolve transfer recipients.");

        var sender = new NotificationRecipient(
            UserId: fromUserId.Value,
            AccountId: @event.FromAccountId,
            NotificationType: NotificationType.MoneyTransferred,
            Payload: JsonSerializer.Serialize(new
            {
                @event.TransferId,
                Direction = "Outgoing",
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        var receiver = new NotificationRecipient(
            UserId: toUserId.Value,
            AccountId: @event.ToAccountId,
            NotificationType: NotificationType.MoneyTransferred,
            Payload: JsonSerializer.Serialize(new
            {
                @event.TransferId,
                Direction = "Incoming",
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        return new ProcessFinancialEventCommand(
            @event.EventId,
            FinanceEventTypes.TransferCompletedV2,
            [sender, receiver]);
    }

    public async Task<ProcessFinancialEventCommand> MapAsync(
        TransferReversedV2 @event,
        CancellationToken ct = default)
    {
        var fromUserId = await customerUserRepository.GetUserIdAsync(
            @event.FromCustomerId, ct);

        var toUserId = await customerUserRepository.GetUserIdAsync(
            @event.ToCustomerId, ct);

        if (fromUserId is null || toUserId is null)
            throw new InvalidOperationException(
                "Unable to resolve transfer reversal recipients.");

        var sender = new NotificationRecipient(
            UserId: fromUserId.Value,
            AccountId: @event.FromAccountId,
            NotificationType: NotificationType.TransferReversed,
            Payload: JsonSerializer.Serialize(new
            {
                @event.ReversalId,
                @event.OriginalTransferId,
                Direction = "OutgoingTransferReversed",
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        var receiver = new NotificationRecipient(
            UserId: toUserId.Value,
            AccountId: @event.ToAccountId,
            NotificationType: NotificationType.TransferReversed,
            Payload: JsonSerializer.Serialize(new
            {
                @event.ReversalId,
                @event.OriginalTransferId,
                Direction = "IncomingTransferReversed",
                @event.Amount,
                @event.Currency,
                @event.OccurredAt
            }));

        return new ProcessFinancialEventCommand(
            @event.EventId,
            FinanceEventTypes.TransferReversedV2,
            [sender, receiver]);
    }
}