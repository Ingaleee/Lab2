namespace OrderTracking.Domain.Common;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Gets the event ID.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
