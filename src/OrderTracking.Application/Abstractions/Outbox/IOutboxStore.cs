namespace OrderTracking.Application.Abstractions.Outbox;

/// <summary>
/// Outbox store abstraction to enqueue integration events for reliable delivery.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Enqueues an integration event to be published asynchronously (e.g., to Kafka).
    /// </summary>
    Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}
