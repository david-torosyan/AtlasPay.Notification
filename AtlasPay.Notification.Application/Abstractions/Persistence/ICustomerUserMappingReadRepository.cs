namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface ICustomerUserMappingReadRepository
{
    Task<Guid?> GetUserIdAsync(
        Guid customerId,
        CancellationToken ct = default);

    Task<string?> GetEmailByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);
}
