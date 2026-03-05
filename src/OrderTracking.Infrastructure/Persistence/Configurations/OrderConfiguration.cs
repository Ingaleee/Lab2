using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderTracking.Domain.Orders;

namespace OrderTracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="Order"/> entity for Entity Framework Core.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures the entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<OrderStatus>(v))
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
