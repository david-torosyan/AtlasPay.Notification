using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.Notifications.Commands;

public sealed record NotificationRecipient(
    Guid UserId,
    Guid? AccountId,
    NotificationType NotificationType,
    string Payload);

public sealed record ProcessFinancialEventCommand(
    Guid EventId,
    string EventType,
    IReadOnlyList<NotificationRecipient> Recipients);