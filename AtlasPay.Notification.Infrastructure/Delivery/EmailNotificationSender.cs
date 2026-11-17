using AtlasPay.Notification.Application.Abstractions.Delivery;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AtlasPay.Notification.Infrastructure.Delivery;

public sealed class EmailNotificationSender(
    IEmailProvider emailProvider,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<EmailNotificationSender> logger)
    : INotificationSender
{
    public DeliveryChannel Channel => DeliveryChannel.Email;

    public Task SendAsync(
        NotificationEntity notification,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Production email recipient resolution is not configured.");
        }

        var recipient = configuration[
            "NotificationDelivery:DevelopmentRecipientEmail"];

        if (string.IsNullOrWhiteSpace(recipient))
        {
            throw new InvalidOperationException(
                "DevelopmentRecipientEmail is not configured.");
        }

        logger.LogInformation(
            "Simulated email sent to {Recipient}. Subject: {Subject}. Body: {Body}",
            recipient,
            notification.Type,
            notification.Payload);

        return Task.CompletedTask;

        var message = new EmailMessage(
            To: recipient,
            Subject: notification.Type.ToString(),
            Body: notification.Payload.ToString() ?? string.Empty);

        //await emailProvider.SendAsync(message, ct);
    }
}