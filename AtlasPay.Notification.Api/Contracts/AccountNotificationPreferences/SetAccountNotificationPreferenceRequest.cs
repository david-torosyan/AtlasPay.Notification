using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Api.Contracts.AccountNotificationPreferences;

public sealed record SetAccountNotificationPreferenceRequest(
    Guid UserId,
    Guid AccountId,
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);