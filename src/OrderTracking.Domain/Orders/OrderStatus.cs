namespace OrderTracking.Domain.Orders;

/// <summary>
/// Represents the status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet processed.
    /// </summary>
    New = 0,

    /// <summary>
    /// Order is being processed.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    Delivered = 2,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 3
}
