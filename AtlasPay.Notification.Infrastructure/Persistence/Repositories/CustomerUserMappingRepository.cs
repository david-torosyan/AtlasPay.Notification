using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class CustomerUserMappingRepository(
    NotificationDbContext dbContext)
    : ICustomerUserMappingRepository
{
    public void Add(CustomerUserMapping mapping)
        => dbContext.CustomerUserMapping.Add(mapping);

    public Task<CustomerUserMapping?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct)
        => dbContext.CustomerUserMapping
            .SingleOrDefaultAsync(x => x.CustomerId == customerId, ct);
}
