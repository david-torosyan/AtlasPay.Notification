using AtlasPay.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasPay.Notification.Infrastructure.Persistence.Configurations;

public sealed class CustomerUserMappingConfiguration
    : IEntityTypeConfiguration<CustomerUserMapping>
{
    public void Configure(
        EntityTypeBuilder<CustomerUserMapping> builder)
    {
        builder.ToTable("customer_users");

        builder.HasKey(x => x.CustomerId);

        builder.Property(x => x.CustomerId)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}