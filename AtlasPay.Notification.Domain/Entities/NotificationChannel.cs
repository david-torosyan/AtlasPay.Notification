using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public class NotificationChannel
{
    private NotificationChannel() { } // For EF Core
    public DeliveryChannel Channel { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public NotificationChannel(
        DeliveryChannel channel,
        bool isActive)
    {
        if (!Enum.IsDefined(channel))
            throw new ArgumentOutOfRangeException(nameof(channel));

        Channel = channel;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
