namespace AtlasPay.Notification.Application.Abstractions.Delivery;

public sealed record EmailMessage(
    string To,
    string Subject,
    string Body);