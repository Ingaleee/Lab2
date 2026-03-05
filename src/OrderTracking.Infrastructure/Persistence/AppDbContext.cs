using Microsoft.EntityFrameworkCore;
using OrderTracking.Domain.Common;
using OrderTracking.Domain.Orders;
using OrderTracking.Infrastructure.Idempotency;
using OrderTracking.Infrastructure.Outbox;

namespace OrderTracking.Infrastructure.Persistence;

/// <summary>
/// Represents the application's database context.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="Order"/> entities.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="OutboxMessage"/> entities.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for <see cref="ProcessedEvent"/> entities.
    /// </summary>
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    /// <summary>
    /// Configures the model for the context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Domain.Common.DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
