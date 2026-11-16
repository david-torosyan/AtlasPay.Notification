using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Api.Contracts.NotificationChannels;

public sealed record SetNotificationChannelStatusRequest(
    DeliveryChannel Channel,
    bool IsActive);