namespace OrderTracking.Infrastructure.Messaging.Kafka;

/// <summary>Standard headers for <see cref="Infrastructure.Outbox.OutboxMessage"/> → Kafka messages.</summary>
public static class KafkaHeaders
{
    /// <summary>Event ID header name.</summary>
    public const string EventId = "event_id";

    /// <summary>Event type header name.</summary>
    public const string EventType = "event_type";

    /// <summary>Event occurred at timestamp header name.</summary>
    public const string OccurredAt = "occurred_at";
}
