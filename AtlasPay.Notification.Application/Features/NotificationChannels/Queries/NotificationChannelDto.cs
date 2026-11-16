using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Features.NotificationChannels.Queries;

public sealed record NotificationChannelDto(
    DeliveryChannel Channel,
    bool IsActive);