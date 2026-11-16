using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence.Repositories;

public sealed class ProcessedEventRepository(
    NotificationDbContext dbContext)
    : IProcessedEventRepository
{
    public void Add(ProcessedEvent processedEvent)
        => dbContext.ProcessedEvents.Add(processedEvent);

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken ct = default)
        => dbContext.ProcessedEvents.AnyAsync(x => x.EventId == eventId, ct);
}
