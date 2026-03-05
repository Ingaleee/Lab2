namespace OrderTracking.Presentation.Api.Realtime;

/// <summary>SignalR group names for <see cref="OrdersHub"/>.</summary>
public static class OrdersHubGroups
{
    /// <summary>Group for all orders list updates.</summary>
    public const string OrdersList = "orders:list";

    /// <summary>
    /// Gets the group name for a specific order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>The group name.</returns>
    public static string Order(Guid orderId) => $"order:{orderId}";
}
