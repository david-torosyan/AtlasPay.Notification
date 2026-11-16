using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired(false);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
           .IsRequired();

        builder.HasIndex(x => new
        {
            x.EventId,
            x.UserId,
            x.Type,
            x.AccountId
        }).IsUnique();

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CreatedAt
        }).IsDescending(false, true);

        builder.HasMany(x => x.Deliveries)
            .WithOne()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Deliveries)
            .HasField("_deliveries");

        builder.Navigation(x => x.Deliveries)
           .UsePropertyAccessMode(
               PropertyAccessMode.Field);
    }
}
