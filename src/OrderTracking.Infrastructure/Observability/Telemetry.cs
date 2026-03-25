using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using OrderTracking.Domain.Orders;

namespace OrderTracking.Infrastructure.Observability;

/// <summary>Shared telemetry primitives for the solution.</summary>
public static class Telemetry
{
    public const string ServiceNamespace = "order-tracking";

    public const string ActivitySourceName = "OrderTracking";

    public const string MeterName = "OrderTracking.Metrics";

    private static readonly ConcurrentDictionary<string, long> StatusTransitionCounts = new();

    private static long _ordersCreated;
    private static long _ordersCatalogListRequests;
    private static long _ordersCatalogDetailViews;
    private static long _kafkaStatusEventsConsumed;
    private static long _kafkaStatusEventsSkippedIdempotent;
    private static long _outboxMessagesPublished;
    private static long _outboxPublishFailures;
    private static long _outboxMessagesPoisoned;
    private static long _ordersCompleted;
    private static long _ordersCancelled;

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);

    static Telemetry()
    {
        foreach (var (from, to) in new[]
                 {
                     (nameof(OrderStatus.New), nameof(OrderStatus.InProgress)),
                     (nameof(OrderStatus.New), nameof(OrderStatus.Cancelled)),
                     (nameof(OrderStatus.InProgress), nameof(OrderStatus.Delivered)),
                     (nameof(OrderStatus.InProgress), nameof(OrderStatus.Cancelled)),
                 })
        {
            StatusTransitionCounts.TryAdd($"{from}\u001f{to}", 0);
        }

        Meter.CreateObservableCounter<long>(
            "order_tracking.orders.created",
            () => Volatile.Read(ref _ordersCreated));
        Meter.CreateObservableCounter<long>(
            "order_tracking.orders.status_updates",
            ObserveStatusTransitions);
        Meter.CreateObservableCounter<long>(
            "order_tracking.catalog.orders_list_requests",
            () => Volatile.Read(ref _ordersCatalogListRequests));
        Meter.CreateObservableCounter<long>(
            "order_tracking.catalog.order_detail_views",
            () => Volatile.Read(ref _ordersCatalogDetailViews));
        Meter.CreateObservableCounter<long>(
            "order_tracking.kafka.status_events.consumed",
            () => Volatile.Read(ref _kafkaStatusEventsConsumed));
        Meter.CreateObservableCounter<long>(
            "order_tracking.kafka.status_events.skipped_idempotent",
            () => Volatile.Read(ref _kafkaStatusEventsSkippedIdempotent));
        Meter.CreateObservableCounter<long>(
            "order_tracking.outbox.kafka_published",
            () => Volatile.Read(ref _outboxMessagesPublished));
        Meter.CreateObservableCounter<long>(
            "order_tracking.outbox.publish_failures",
            () => Volatile.Read(ref _outboxPublishFailures));
        Meter.CreateObservableCounter<long>(
            "order_tracking.outbox.poisoned",
            () => Volatile.Read(ref _outboxMessagesPoisoned));
        Meter.CreateObservableCounter<long>(
            "order_tracking.orders.completed",
            () => Volatile.Read(ref _ordersCompleted));
        Meter.CreateObservableCounter<long>(
            "order_tracking.orders.cancelled",
            () => Volatile.Read(ref _ordersCancelled));
    }

    public static void RecordOrderCreated() => Interlocked.Increment(ref _ordersCreated);

    public static void RecordOrderCatalogListRequest() => Interlocked.Increment(ref _ordersCatalogListRequests);

    public static void RecordOrderCatalogDetailView() => Interlocked.Increment(ref _ordersCatalogDetailViews);

    public static void RecordOrderStatusTransition(string fromStatus, string toStatus)
    {
        var key = $"{fromStatus}\u001f{toStatus}";
        StatusTransitionCounts.AddOrUpdate(key, 1, static (_, v) => v + 1);
    }

    public static void RecordKafkaStatusEventConsumed() => Interlocked.Increment(ref _kafkaStatusEventsConsumed);

    public static void RecordKafkaStatusEventSkippedIdempotent() =>
        Interlocked.Increment(ref _kafkaStatusEventsSkippedIdempotent);

    public static void RecordOutboxMessagePublished() => Interlocked.Increment(ref _outboxMessagesPublished);

    public static void RecordOutboxPublishFailure() => Interlocked.Increment(ref _outboxPublishFailures);

    public static void RecordOutboxMessagePoisoned() => Interlocked.Increment(ref _outboxMessagesPoisoned);

    public static void RecordOrderCompleted() => Interlocked.Increment(ref _ordersCompleted);

    public static void RecordOrderCancelled() => Interlocked.Increment(ref _ordersCancelled);

    private static IEnumerable<Measurement<long>> ObserveStatusTransitions()
    {
        foreach (var kv in StatusTransitionCounts)
        {
            var sep = kv.Key.IndexOf('\u001f', StringComparison.Ordinal);
            if (sep <= 0 || sep >= kv.Key.Length - 1)
            {
                continue;
            }

            var from = kv.Key.AsSpan(0, sep).ToString();
            var to = kv.Key.AsSpan(sep + 1).ToString();
            yield return new Measurement<long>(
                kv.Value,
                new KeyValuePair<string, object?>("from_status", from),
                new KeyValuePair<string, object?>("to_status", to));
        }
    }
}
