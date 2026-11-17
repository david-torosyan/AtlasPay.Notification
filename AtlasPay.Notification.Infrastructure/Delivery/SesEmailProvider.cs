using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using AtlasPay.Notification.Application.Abstractions.Delivery;
using Microsoft.Extensions.Configuration;

namespace AtlasPay.Notification.Infrastructure.Delivery;

public sealed class SesEmailProvider(
    IAmazonSimpleEmailService ses,
    IConfiguration configuration)
    : IEmailProvider
{
    public async Task SendAsync(
        EmailMessage message,
        CancellationToken ct)
    {
        var from = configuration["Email:FromAddress"];

        if (string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException(
                "Email sender address is not configured.");

        var request = new SendEmailRequest
        {
            Source = from,

            Destination = new Destination
            {
                ToAddresses = [message.To]
            },

            Message = new Message
            {
                Subject = new Content
                {
                    Charset = "UTF-8",
                    Data = message.Subject
                },

                Body = new Body
                {
                    Text = new Content
                    {
                        Charset = "UTF-8",
                        Data = message.Body
                    }
                }
            }
        };

        await ses.SendEmailAsync(request, ct);
    }
}