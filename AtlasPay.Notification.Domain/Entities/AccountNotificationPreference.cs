using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public sealed class AccountNotificationPreference
{
    private AccountNotificationPreference() { } // For EF Core

    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public DeliveryChannel Channel { get; private set; }
    public bool Enabled { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public AccountNotificationPreference(
        Guid userId,
        Guid accountId,
        NotificationType notificationType,
        DeliveryChannel channel,
        bool enabled)
    {
        UserId = userId;
        AccountId = accountId;
        NotificationType = notificationType;
        Channel = channel;
        Enabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }
}