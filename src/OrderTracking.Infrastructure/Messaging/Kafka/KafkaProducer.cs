using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrderTracking.Infrastructure.Messaging.Kafka;

/// <summary>Confluent.Kafka implementation of <see cref="IKafkaProducer"/>.</summary>
public sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaProducer"/> class.
    /// </summary>
    /// <param name="options">The Kafka options.</param>
    /// <param name="logger">The logger.</param>
    public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var cfg = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = options.Value.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,
            LingerMs = 5,
            MessageSendMaxRetries = 5,
            RetryBackoffMs = 200,
        };

        _producer = new ProducerBuilder<string, string>(cfg).Build();
    }

    /// <summary>
    /// Publishes a message to the specified Kafka topic.
    /// </summary>
    /// <param name="topic">The topic name.</param>
    /// <param name="key">The message key.</param>
    /// <param name="payload">The message payload.</param>
    /// <param name="headers">Optional message headers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishAsync(
        string topic,
        string key,
        string payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var msg = new Message<string, string>
        {
            Key = key,
            Value = payload,
            Headers = new Headers()
        };

        if (headers is not null)
        {
            foreach (var (k, v) in headers)
            {
                msg.Headers.Add(k, System.Text.Encoding.UTF8.GetBytes(v));
            }
        }

        DeliveryResult<string, string> result;
        try
        {
            result = await _producer.ProduceAsync(topic, msg, cancellationToken);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka publish failed. Topic={Topic}, Key={Key}", topic, key);
            throw;
        }

        _logger.LogInformation(
            "Kafka published. Topic={Topic}, Partition={Partition}, Offset={Offset}, Key={Key}",
            result.Topic, result.Partition.Value, result.Offset.Value, key);
    }

    /// <summary>
    /// Disposes the producer.
    /// </summary>
    public void Dispose() => _producer.Dispose();
}
