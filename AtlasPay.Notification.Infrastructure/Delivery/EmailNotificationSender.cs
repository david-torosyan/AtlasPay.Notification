using AtlasPay.Notification.Application.Abstractions.Delivery;
using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Infrastructure.Delivery;

public sealed class EmailNotificationSender(
    IEmailProvider emailProvider,
    ICustomerUserMappingReadRepository customerUserRepository)
    : INotificationSender
{
    public DeliveryChannel Channel => DeliveryChannel.Email;

    public async Task SendAsync(
        NotificationEntity notification,
        CancellationToken ct)
    {
        var recipient = await customerUserRepository.GetEmailByUserIdAsync(
            notification.UserId,
            ct);

        if (string.IsNullOrWhiteSpace(recipient))
            throw new InvalidOperationException(
                $"Email is not configured for user {notification.UserId}.");

        var message = new EmailMessage(
            To: recipient,
            Subject: notification.Type.ToString(),
            Body: notification.Payload.ToString() ?? string.Empty);

        await emailProvider.SendAsync(message, ct);
    }
}
