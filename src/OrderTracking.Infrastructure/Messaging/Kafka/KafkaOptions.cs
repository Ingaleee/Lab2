namespace OrderTracking.Infrastructure.Messaging.Kafka;

/// <summary>Kafka connection and producer settings.</summary>
public sealed class KafkaOptions
{
    /// <summary>Configuration section name for <see cref="KafkaOptions"/>.</summary>
    public const string SectionName = "Kafka";

    /// <summary>Gets or sets Kafka bootstrap servers, e.g. "localhost:9092".</summary>
    public string BootstrapServers { get; init; } = string.Empty;

    /// <summary>Gets or sets Kafka client id.</summary>
    public string ClientId { get; init; } = "order-tracking-worker";

    /// <summary>Gets or sets topic for order status change events.</summary>
    public string OrderStatusTopic { get; init; } = "order.status.v1";

    /// <summary>Gets or sets consumer group ID.</summary>
    public string ConsumerGroupId { get; init; } = "order-tracking-api";

    /// <summary>Gets or sets whether to enable auto commit.</summary>
    public bool EnableAutoCommit { get; init; } = false;

    /// <summary>Gets or sets auto offset reset policy ("Earliest" or "Latest").</summary>
    public string AutoOffsetReset { get; init; } = "Earliest";
}
