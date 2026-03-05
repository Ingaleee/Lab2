namespace OrderTracking.Contracts.Orders;

/// <summary>
/// Response containing order information.
/// </summary>
public sealed class OrderResponse
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
