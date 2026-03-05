namespace OrderTracking.Contracts.Orders;

/// <summary>
/// Request to create a new order.
/// </summary>
public sealed class CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
