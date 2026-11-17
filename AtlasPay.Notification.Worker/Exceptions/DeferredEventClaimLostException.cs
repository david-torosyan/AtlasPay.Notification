namespace AtlasPay.Notification.Worker.Exceptions;

public sealed class DeferredEventClaimLostException(Guid eventId)
    : Exception($"Claim lost for deferred event {eventId}.")
{
    public Guid EventId { get; } = eventId;
}