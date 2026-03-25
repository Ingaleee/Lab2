using System.Diagnostics.Metrics;
using System.Threading;

namespace OrderTracking.Infrastructure.Observability;

public static class OrderBookGaugeRegistration
{
    private static int _registered;

    public static void Register(OrderBookMetricsSnapshot snapshot)
    {
        if (Interlocked.Exchange(ref _registered, 1) != 0)
            return;

        Telemetry.Meter.CreateObservableGauge(
            "order_tracking.orders.stock_by_status",
            snapshot.MeasureStockByStatus);

        Telemetry.Meter.CreateObservableGauge(
            "order_tracking.orders.work_queue_open",
            () => snapshot.WorkQueueOpen);

        Telemetry.Meter.CreateObservableGauge(
            "order_tracking.orders.sla_at_risk",
            snapshot.MeasureSlaRisk);
    }
}
