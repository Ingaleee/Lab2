using OrderTracking.Application.Abstractions.Outbox;
using OrderTracking.Infrastructure.Persistence;

namespace OrderTracking.Infrastructure.Outbox;

/// <summary>EF Core implementation of <see cref="IOutboxStore"/>.</summary>
public sealed class EfOutboxStore : IOutboxStore
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfOutboxStore"/> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public EfOutboxStore(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Enqueues an integration event to the outbox for reliable async publishing.
    /// </summary>
    /// <typeparam name="TEvent">The type of the integration event.</typeparam>
    /// <param name="event">The integration event to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var id = TryGetEventId(@event) ?? Guid.NewGuid();

        var type = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        var payload = OutboxJsonSerializer.Serialize(@event);

        var message = new OutboxMessage(
            id: id,
            occurredAt: DateTimeOffset.UtcNow,
            type: type,
            payload: payload);

        _db.OutboxMessages.Add(message);

        return Task.CompletedTask;
    }

    private static Guid? TryGetEventId<TEvent>(TEvent @event) where TEvent : class
    {
        var prop = typeof(TEvent).GetProperty("EventId");
        if (prop?.PropertyType == typeof(Guid))
        {
            var value = prop.GetValue(@event);
            if (value is Guid guid && guid != Guid.Empty)
                return guid;
        }
        return null;
    }
}
