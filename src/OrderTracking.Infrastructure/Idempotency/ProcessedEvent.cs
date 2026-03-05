namespace OrderTracking.Infrastructure.Idempotency;

/// <summary>Marks an integration event as processed to make consumer idempotent.</summary>
public sealed class ProcessedEvent
{
    /// <summary>Gets the event ID.</summary>
    public Guid EventId { get; init; }

    /// <summary>Gets the date and time when the event was processed.</summary>
    public DateTimeOffset ProcessedAt { get; init; }

    private ProcessedEvent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessedEvent"/> class.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="processedAt">The date and time when the event was processed.</param>
    public ProcessedEvent(Guid eventId, DateTimeOffset processedAt)
    {
        EventId = eventId;
        ProcessedAt = processedAt;
    }
}
