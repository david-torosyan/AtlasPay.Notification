
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.NotificationPreferences.Queries;

public sealed record NotificationPreferenceDto(
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);
