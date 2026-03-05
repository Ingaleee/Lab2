namespace OrderTracking.Infrastructure.Outbox;

/// <summary>
/// Outbox message stored in DB to ensure reliable async publishing (e.g. to Kafka).
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>Gets the unique identifier of the message.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the date and time when the event occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Gets the logical event type name (contract type).</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the serialized event payload (jsonb).</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>Gets the current <see cref="OutboxMessageStatus"/> of the message.</summary>
    public OutboxMessageStatus Status { get; private set; } = OutboxMessageStatus.Pending;

    /// <summary>Gets the date and time when the message was processed, or null if not yet processed.</summary>
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>Gets the number of processing attempts.</summary>
    public int Attempts { get; private set; }

    /// <summary>Gets the last error message, if any.</summary>
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxMessage"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the message.</param>
    /// <param name="occurredAt">The date and time when the event occurred.</param>
    /// <param name="type">The logical event type name.</param>
    /// <param name="payload">The serialized event payload.</param>
    public OutboxMessage(Guid id, DateTimeOffset occurredAt, string type, string payload)
    {
        Id = id;
        OccurredAt = occurredAt;
        Type = type;
        Payload = payload;
        Status = OutboxMessageStatus.Pending;
    }

    /// <summary>
    /// Marks the message as processed.
    /// </summary>
    /// <param name="processedAt">The date and time when the message was processed.</param>
    public void MarkProcessed(DateTimeOffset processedAt)
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = processedAt;
        LastError = null;
    }

    /// <summary>
    /// Marks the message as failed (status remains <see cref="OutboxMessageStatus.Pending"/>).
    /// </summary>
    /// <param name="error">The error message.</param>
    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error;
    }

    /// <summary>
    /// Marks the message as poisoned after multiple failed attempts.
    /// </summary>
    /// <param name="error">The error message.</param>
    public void MarkPoisoned(string error)
    {
        Attempts++;
        LastError = error;
        Status = OutboxMessageStatus.Poisoned;
    }
}
