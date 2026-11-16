using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public class NotificationDeliveryConfiguration
    : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.NotificationId)
            .IsRequired();

        builder.Property(x => x.Channel)
           .HasConversion<string>()
           .HasMaxLength(30)
           .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
           .HasDefaultValue(0)
           .IsRequired();

        builder.Property(x => x.NextRetryAt)
            .IsRequired(false);

        builder.Property(x => x.LockedUntil)
            .IsRequired(false);

        builder.Property(x => x.LockToken)
            .IsRequired(false);

        builder.Property(x => x.SentAt)
            .IsRequired(false);

        builder.Property(x => x.LastError)
          .HasColumnType("text");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
           .IsRequired();

        builder.HasIndex(x => new
        {
            x.NotificationId,
            x.Channel
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.NextRetryAt,
            x.CreatedAt
        })
        .HasFilter("\"Status\" = 'Pending'");

        builder.HasIndex(x => x.LockedUntil)
            .HasFilter("\"Status\" = 'Processing'");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_NotificationDelivery_AttemptCount",
                "\"AttemptCount\" >= 0");
        });
    }
}
