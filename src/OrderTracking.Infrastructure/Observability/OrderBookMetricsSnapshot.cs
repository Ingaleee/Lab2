using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using OrderTracking.Domain.Orders;

namespace OrderTracking.Infrastructure.Observability;

public sealed class OrderBookMetricsSnapshot
{
    private readonly object _gate = new();

    private long _openPipeline;
    private long _overdueNew;
    private long _staleInProgress;
    private readonly long[] _byStatus = new long[4];

    public long WorkQueueOpen => Volatile.Read(ref _openPipeline);

    public void Update(
        IReadOnlyDictionary<OrderStatus, long> byStatus,
        long overdueNew,
        long staleInProgress)
    {
        lock (_gate)
        {
            for (var i = 0; i < _byStatus.Length; i++)
            {
                _byStatus[i] = 0;
            }

            foreach (var (status, count) in byStatus)
            {
                _byStatus[(int)status] = count;
            }

            _openPipeline = _byStatus[(int)OrderStatus.New] + _byStatus[(int)OrderStatus.InProgress];
            _overdueNew = overdueNew;
            _staleInProgress = staleInProgress;
        }
    }

    public IEnumerable<Measurement<long>> MeasureStockByStatus()
    {
        lock (_gate)
        {
            yield return new Measurement<long>(_byStatus[(int)OrderStatus.New], new KeyValuePair<string, object?>("status", nameof(OrderStatus.New)));
            yield return new Measurement<long>(_byStatus[(int)OrderStatus.InProgress], new KeyValuePair<string, object?>("status", nameof(OrderStatus.InProgress)));
            yield return new Measurement<long>(_byStatus[(int)OrderStatus.Delivered], new KeyValuePair<string, object?>("status", nameof(OrderStatus.Delivered)));
            yield return new Measurement<long>(_byStatus[(int)OrderStatus.Cancelled], new KeyValuePair<string, object?>("status", nameof(OrderStatus.Cancelled)));
        }
    }

    public IEnumerable<Measurement<long>> MeasureSlaRisk()
    {
        lock (_gate)
        {
            yield return new Measurement<long>(_overdueNew, new KeyValuePair<string, object?>("reason", "new_queue_stale"));
            yield return new Measurement<long>(_staleInProgress, new KeyValuePair<string, object?>("reason", "in_progress_stalled"));
        }
    }
}
