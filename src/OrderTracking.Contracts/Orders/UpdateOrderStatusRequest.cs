namespace OrderTracking.Contracts.Orders;

/// <summary>
/// Request to update an order status.
/// </summary>
public sealed class UpdateOrderStatusRequest
{
    /// <summary>
    /// Gets or sets the new order status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
