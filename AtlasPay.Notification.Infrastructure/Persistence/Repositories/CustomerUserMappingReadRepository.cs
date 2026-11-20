using AtlasPay.Notification.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class CustomerUserMappingReadRepository(
    NotificationDbContext dbContext)
    : ICustomerUserMappingReadRepository
{
    public Task<Guid?> GetUserIdAsync(
        Guid customerId,
        CancellationToken ct = default)
            => dbContext.CustomerUserMapping
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .Select(x => (Guid?)x.UserId)
                .SingleOrDefaultAsync(ct);
}