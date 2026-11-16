using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;

public sealed record AccountNotificationPreferenceResponse(
    Guid AccountId,
    NotificationType NotificationType,
    DeliveryChannel Channel,
    bool Enabled);