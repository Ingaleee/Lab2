using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderTracking.Infrastructure.Idempotency;

namespace OrderTracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the <see cref="ProcessedEvent"/> entity for Entity Framework Core.
/// </summary>
public sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    /// <summary>
    /// Configures the entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventId)
            .HasColumnName("event_id");

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("ix_processed_events_processed_at");
    }
}
