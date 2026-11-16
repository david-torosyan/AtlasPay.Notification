using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public class NotificationPreference
{
    private NotificationPreference() { } // For EF Core

    public Guid UserId { get; private set; }

    public NotificationType NotificationType { get; private set; }

    public DeliveryChannel Channel { get; private set; }

    public bool Enabled { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public NotificationPreference(
        Guid userId,
        NotificationType notificationType,
        DeliveryChannel channel,
        bool enabled)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "UserId cannot be empty.", nameof(userId));

        if (!Enum.IsDefined(notificationType))
            throw new ArgumentOutOfRangeException(
                nameof(notificationType));

        if (!Enum.IsDefined(channel))
            throw new ArgumentOutOfRangeException(
                nameof(channel));

        UserId = userId;
        NotificationType = notificationType;
        Channel = channel;
        Enabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        if (Enabled == enabled)
            return;

        Enabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }
}