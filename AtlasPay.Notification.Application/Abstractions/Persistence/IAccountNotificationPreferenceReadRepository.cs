using AtlasPay.Notification.Application.Features.AccountNotificationPreferences.Queries;
using AtlasPay.Notification.Domain.Enums;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface IAccountNotificationPreferenceReadRepository
{
    Task<IReadOnlyList<AccountNotificationPreferenceResponse>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<bool?> GetEnabledAsync(
        Guid userId,
        Guid accountId,
        NotificationType notificationType,
        DeliveryChannel channel,
        CancellationToken ct = default);
}