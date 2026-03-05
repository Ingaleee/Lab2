namespace OrderTracking.Presentation.Api.Realtime;

/// <summary>SignalR event names for <see cref="OrdersHub"/>.</summary>
public static class HubEvents
{
    /// <summary>Event name for order status changed.</summary>
    public const string OrderStatusChanged = "orderStatusChanged";
}
