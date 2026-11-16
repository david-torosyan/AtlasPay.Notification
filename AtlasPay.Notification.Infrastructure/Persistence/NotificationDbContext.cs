using AtlasPay.Notification.Application.Abstractions.Persistence;
using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Notification.Infrastructure.Persistence;

public class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<NotificationEntity> Notifications
        => Set<NotificationEntity>();

    public DbSet<NotificationPreference> NotificationPreferences
        => Set<NotificationPreference>();

    public DbSet<NotificationChannel> NotificationChannels
        => Set<NotificationChannel>();

    public DbSet<ProcessedEvent> ProcessedEvents
        => Set<ProcessedEvent>();

    public DbSet<CustomerUserMapping> CustomerUsers
        => Set<CustomerUserMapping>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);
    }
}
