using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public sealed class DeferredEventConfiguration
    : IEntityTypeConfiguration<DeferredEvent>
{
    public void Configure(
        EntityTypeBuilder<DeferredEvent> builder)
    {
        builder.ToTable("deferred_events");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventId)
            .ValueGeneratedNever();

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .IsRequired();

        builder.Property(x => x.NextRetryAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.NextRetryAt);
    }
}