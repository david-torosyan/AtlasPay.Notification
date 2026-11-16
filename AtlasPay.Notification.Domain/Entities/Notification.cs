using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public class Notification
{
    private Notification() { } // For EF Core

    private readonly List<NotificationDelivery> _deliveries = [];

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? AccountId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Payload { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<NotificationDelivery> Deliveries =>
                _deliveries.AsReadOnly();

    public Notification(
        Guid eventId,
        Guid userId,
        Guid? accountId,
        NotificationType type,
        string payload)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.");

        Id = Guid.NewGuid();
        EventId = eventId;
        UserId = userId;
        AccountId = accountId;
        Type = type;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddDelivery(DeliveryChannel channel)
    {
        if (!Enum.IsDefined(channel))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (_deliveries.Any(x => x.Channel == channel))
            throw new InvalidOperationException(
                $"Delivery channel {channel} already exists.");

        var delivery = new NotificationDelivery(Id, channel);

        _deliveries.Add(delivery);
    }
}
