using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public sealed class AccountNotificationPreferenceConfiguration
    : IEntityTypeConfiguration<AccountNotificationPreference>
{
    public void Configure(
        EntityTypeBuilder<AccountNotificationPreference> builder)
    {
        builder.ToTable("account_notification_preferences");

        builder.HasKey(x => new
        {
            x.UserId,
            x.AccountId,
            x.NotificationType,
            x.Channel
        });

        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}