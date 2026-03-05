using OrderTracking.Domain.Common;
using OrderTracking.Domain.Orders.Events;

namespace OrderTracking.Domain.Orders;

/// <summary>
/// Represents an order aggregate root.
/// </summary>
public sealed class Order : Entity
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the order number.
    /// </summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the order description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the current order status.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Gets the date and time when the order was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when the order was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    private Order() { }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="orderNumber">The order number.</param>
    /// <param name="description">The order description.</param>
    /// <param name="now">The current date and time.</param>
    /// <returns>A result containing the created order or an error message.</returns>
    public static Result<Order> Create(string orderNumber, string description, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return Result<Order>.Failure("Order number cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<Order>.Failure("Description cannot be empty.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Description = description,
            Status = OrderStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };

        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, order.OrderNumber));

        return Result<Order>.Success(order);
    }

    /// <summary>
    /// Changes the order status.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    /// <param name="now">The current date and time.</param>
    /// <returns>A result indicating success or failure.</returns>
    public Result ChangeStatus(OrderStatus newStatus, DateTimeOffset now)
    {
        if (Status == newStatus)
        {
            return Result.Failure("Order is already in the specified status.");
        }

        if (!CanTransitionTo(newStatus))
        {
            return Result.Failure($"Cannot transition from {Status} to {newStatus}.");
        }

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = now;

        AddDomainEvent(new OrderStatusChangedDomainEvent(Id, previousStatus, newStatus));

        return Result.Success();
    }

    /// <summary>
    /// Determines whether the order can transition to the specified status.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    /// <returns>True if the transition is allowed; otherwise, false.</returns>
    public bool CanTransitionTo(OrderStatus newStatus)
    {
        return Status switch
        {
            OrderStatus.New => newStatus == OrderStatus.InProgress || newStatus == OrderStatus.Cancelled,
            OrderStatus.InProgress => newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}
