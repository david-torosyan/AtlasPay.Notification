using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface IAccountNotificationPreferenceRepository
{
    Task<AccountNotificationPreference?> GetAsync(
        Guid userId,
        Guid accountId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default);

    void Add(AccountNotificationPreference preference);
}