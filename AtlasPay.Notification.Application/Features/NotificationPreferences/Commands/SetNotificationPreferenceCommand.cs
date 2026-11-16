
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.NotificationPreferences.Commands;

public sealed record SetNotificationPreferenceCommand(
    Guid UserId,
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);
