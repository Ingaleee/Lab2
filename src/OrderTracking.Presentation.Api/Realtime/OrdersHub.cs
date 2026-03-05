using Microsoft.AspNetCore.SignalR;

namespace OrderTracking.Presentation.Api.Realtime;

/// <summary>SignalR hub for real-time order updates.</summary>
public sealed class OrdersHub : Hub
{
    /// <summary>
    /// Joins the orders list group.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task JoinOrdersList() =>
        Groups.AddToGroupAsync(Context.ConnectionId, OrdersHubGroups.OrdersList);

    /// <summary>
    /// Leaves the orders list group.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task LeaveOrdersList() =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, OrdersHubGroups.OrdersList);

    /// <summary>
    /// Joins the group for a specific order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task JoinOrder(string orderId)
    {
        if (!Guid.TryParse(orderId, out var id))
            return Task.CompletedTask;

        return Groups.AddToGroupAsync(Context.ConnectionId, OrdersHubGroups.Order(id));
    }

    /// <summary>
    /// Leaves the group for a specific order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task LeaveOrder(string orderId)
    {
        if (!Guid.TryParse(orderId, out var id))
            return Task.CompletedTask;

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, OrdersHubGroups.Order(id));
    }
}
