using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Commands;

public sealed record SetAccountNotificationPreferenceCommand(
    Guid UserId,
    Guid AccountId,
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);