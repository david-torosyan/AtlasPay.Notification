namespace AtlasPay.Notification.Application.Abstractions.Delivery;

public interface IEmailProvider
{
    Task SendAsync(
        EmailMessage message,
        CancellationToken ct);
}