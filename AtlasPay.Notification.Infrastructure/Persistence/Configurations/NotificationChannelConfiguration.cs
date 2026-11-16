
using AtlasPay.Notification.Domain.Entities;
using AtlasPay.Notification.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public class NotificationChannelConfiguration
    : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(
        EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ToTable("notification_channels");

        builder.HasKey(x => x.Channel);

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(30)
            .ValueGeneratedNever();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasData(
            new
            {
                Channel = DeliveryChannel.Email,
                IsActive = true,
                UpdatedAt = new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Channel = DeliveryChannel.Sms,
                IsActive = true,
                UpdatedAt = new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Channel = DeliveryChannel.Push,
                IsActive = true,
                UpdatedAt = new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}
