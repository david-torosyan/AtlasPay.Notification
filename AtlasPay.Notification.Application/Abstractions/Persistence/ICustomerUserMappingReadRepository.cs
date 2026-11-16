namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface ICustomerUserMappingReadRepository
{
    Task<Guid?> GetUserIdAsync(
        Guid customerId,
        CancellationToken ct = default);
}