using OrderTracking.Domain.Common;

namespace OrderTracking.Domain.Orders.Events;

/// <summary>
/// Domain event raised when an order status changes.
/// </summary>
public sealed class OrderStatusChangedDomainEvent : DomainEvent
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// Gets the previous status.
    /// </summary>
    public Orders.OrderStatus PreviousStatus { get; }

    /// <summary>
    /// Gets the new status.
    /// </summary>
    public Orders.OrderStatus NewStatus { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatusChangedDomainEvent"/> class.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="previousStatus">The previous status.</param>
    /// <param name="newStatus">The new status.</param>
    public OrderStatusChangedDomainEvent(
        Guid orderId,
        Orders.OrderStatus previousStatus,
        Orders.OrderStatus newStatus)
    {
        OrderId = orderId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }
}
