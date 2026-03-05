using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OrderTracking.Infrastructure.Observability;

/// <summary>Shared telemetry primitives for the solution.</summary>
public static class Telemetry
{
    public const string ServiceNamespace = "order-tracking";

    public const string ActivitySourceName = "OrderTracking";

    public const string MeterName = "OrderTracking.Metrics";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName);
}
