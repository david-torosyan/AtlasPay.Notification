using AtlasPay.Notification.Application.Abstractions.Delivery;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace AtlasPay.Notification.Infrastructure.Delivery;

public sealed class SmtpEmailProvider(IConfiguration configuration)
    : IEmailProvider
{
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken ct)
    {
        var from = configuration["Email:FromAddress"];
        var host = configuration["Email:Smtp:Host"];
        var configuredPort = configuration["Email:Smtp:Port"];

        if (string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException(
                "Email sender address is not configured.");

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException(
                "SMTP host is not configured.");

        if (!int.TryParse(configuredPort, out var port) || port is < 1 or > 65535)
            throw new InvalidOperationException(
                "SMTP port is not configured correctly.");

        using var mailMessage = new MailMessage(
            from,
            message.To,
            message.Subject,
            message.Body);

        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = false,
            UseDefaultCredentials = false
        };

        await smtpClient.SendMailAsync(mailMessage, ct);
    }
}
