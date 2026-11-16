
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Api.Contracts.NotificationPreferences;

public sealed record SetNotificationPreferenceRequest(
    Guid UserId,
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);
