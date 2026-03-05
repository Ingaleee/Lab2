namespace OrderTracking.Infrastructure.Messaging.Kafka;

/// <summary>Abstraction over Kafka producer.</summary>
public interface IKafkaProducer
{
    /// <summary>
    /// Publishes a message to the specified Kafka topic.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="key">The message key.</param>
    /// <param name="payload">The message payload.</param>
    /// <param name="headers">Optional message headers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(
        string topic,
        string key,
        string payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
