using OrderTracking.Domain.Common;

namespace OrderTracking.Domain.Orders.Events;

/// <summary>
/// Domain event raised when an order is created.
/// </summary>
public sealed class OrderCreatedDomainEvent : DomainEvent
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// Gets the order number.
    /// </summary>
    public string OrderNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderCreatedDomainEvent"/> class.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="orderNumber">The order number.</param>
    public OrderCreatedDomainEvent(Guid orderId, string orderNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
    }
}
