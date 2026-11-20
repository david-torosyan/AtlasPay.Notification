using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Application.Exceptions;
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Infrastructure.Persistence.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence;

public class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<NotificationEntity> Notifications
        => Set<NotificationEntity>();

    public DbSet<NotificationDelivery> NotificationDeliveries
        => Set<NotificationDelivery>();

    public DbSet<NotificationPreference> NotificationPreferences
        => Set<NotificationPreference>();

    public DbSet<NotificationChannel> NotificationChannels
        => Set<NotificationChannel>();

    public DbSet<ProcessedEvent> ProcessedEvents
        => Set<ProcessedEvent>();

    public DbSet<CustomerUserMapping> CustomerUserMapping
        => Set<CustomerUserMapping>();

    public DbSet<AccountNotificationPreference> AccountNotificationPreferences
        => Set<AccountNotificationPreference>();

    public DbSet<DeferredEvent> DeferredEvents
        => Set<DeferredEvent>();

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
        CancellationToken ct = default)
    {
        var transaction = await Database.BeginTransactionAsync(ct);

        return new UnitOfWorkTransaction(transaction);
    }

    public override async Task<int> SaveChangesAsync(
    CancellationToken ct = default)
    {
        try
        {
            return await base.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (NotificationDuplicateDetector.IsDuplicate(ex))
        {
            throw new NotificationAlreadyExistsException(ex);
        }
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);
    }
}
