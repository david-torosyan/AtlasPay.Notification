using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Domain.Entities;

public class Notification
{
    private Notification() { } // For EF Core

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid UserId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Payload { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public Notification(
        Guid eventId,
        Guid userId,
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
        Type = type;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }
}
