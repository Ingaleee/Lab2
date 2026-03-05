namespace OrderTracking.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when an order status changes (v1).
/// </summary>
public sealed class OrderStatusChangedIntegrationEventV1
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous order status.
    /// </summary>
    public string OldStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new order status.
    /// </summary>
    public string NewStatus { get; set; } = string.Empty;
}
