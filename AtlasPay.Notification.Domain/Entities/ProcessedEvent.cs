
namespace AtlasPay.Notification.Domain.Entities;

public class ProcessedEvent
{
    private ProcessedEvent() { } // For EF Core

    public Guid EventId { get; private set; }

    public string EventType { get; private set; } = null!;

    public DateTime ProcessedAt { get; private set; }

    public ProcessedEvent(
        Guid eventId,
        string eventType)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException(
                "EventId cannot be empty.", nameof(eventId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException(
                "EventType is required.", nameof(eventType));

        EventId = eventId;
        EventType = eventType;
        ProcessedAt = DateTime.UtcNow;
    }
}