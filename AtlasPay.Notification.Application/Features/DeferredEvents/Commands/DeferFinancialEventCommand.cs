namespace AtlasPay.Notification.Application.Features.DeferredEvents.Commands;

public sealed record DeferFinancialEventCommand(
    Guid EventId,
    string EventType,
    string Payload);