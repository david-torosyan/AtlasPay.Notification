using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.NotificationChannels.Commands;

public sealed record SetNotificationChannelStatusCommand(
    DeliveryChannel Channel,
    bool IsActive);