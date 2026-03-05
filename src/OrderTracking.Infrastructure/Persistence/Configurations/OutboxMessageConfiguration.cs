using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderTracking.Infrastructure.Outbox;

namespace OrderTracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="OutboxMessage"/> entity for Entity Framework Core.
/// </summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Configures the entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.Status, x.OccurredAt })
            .HasDatabaseName("ix_outbox_status_occurred_at");

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("ix_outbox_processed_at");
    }
}
