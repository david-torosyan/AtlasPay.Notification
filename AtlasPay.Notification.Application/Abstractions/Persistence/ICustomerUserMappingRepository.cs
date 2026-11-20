
using AtlasPay.Notification.Domain.Entities;

namespace AtlasPay.Notification.Application.Abstractions.Persistence;

public interface ICustomerUserMappingRepository
{
    Task<CustomerUserMapping?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct);

    void Add(CustomerUserMapping mapping);
}
