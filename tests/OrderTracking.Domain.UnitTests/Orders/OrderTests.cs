using OrderTracking.Domain.Orders;
using OrderTracking.Domain.Orders.Events;

namespace OrderTracking.Domain.UnitTests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Create_ShouldCreateNewOrder_WithNewStatus_AndDomainEvent()
    {
        // Arrange
        var orderNumber = "ORD-001";
        var description = "Test order";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = Order.Create(orderNumber, description, now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        var order = result.Value;
        Assert.Equal(orderNumber, order.OrderNumber);
        Assert.Equal(description, order.Description);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(now, order.CreatedAt);
        Assert.Equal(now, order.UpdatedAt);
        
        var domainEvents = order.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.IsType<OrderCreatedDomainEvent>(domainEvents[0]);
        
        var createdEvent = (OrderCreatedDomainEvent)domainEvents[0];
        Assert.Equal(order.Id, createdEvent.OrderId);
        Assert.Equal(orderNumber, createdEvent.OrderNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrderNumber_ShouldFail(string? orderNumber)
    {
        // Arrange
        var description = "Test order";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = Order.Create(orderNumber!, description, now);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order number cannot be empty.", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldFail(string? description)
    {
        // Arrange
        var orderNumber = "ORD-001";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = Order.Create(orderNumber, description!, now);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Description cannot be empty.", result.Error);
    }

    [Fact]
    public void ChangeStatus_NewToInProgress_ShouldSucceed_AndRaiseEvent()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ClearDomainEvents();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = order.ChangeStatus(OrderStatus.InProgress, now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.InProgress, order.Status);
        Assert.Equal(now, order.UpdatedAt);
        
        var domainEvents = order.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.IsType<OrderStatusChangedDomainEvent>(domainEvents[0]);
        
        var statusChangedEvent = (OrderStatusChangedDomainEvent)domainEvents[0];
        Assert.Equal(order.Id, statusChangedEvent.OrderId);
        Assert.Equal(OrderStatus.New, statusChangedEvent.PreviousStatus);
        Assert.Equal(OrderStatus.InProgress, statusChangedEvent.NewStatus);
    }

    [Fact]
    public void ChangeStatus_NewToDelivered_ShouldFail()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        var initialUpdatedAt = order.UpdatedAt;

        // Act
        var result = order.ChangeStatus(OrderStatus.Delivered, DateTimeOffset.UtcNow);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot transition", result.Error);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(initialUpdatedAt, order.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_DeliveredToAny_ShouldFail()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ChangeStatus(OrderStatus.InProgress, DateTimeOffset.UtcNow);
        order.ChangeStatus(OrderStatus.Delivered, DateTimeOffset.UtcNow);
        order.ClearDomainEvents();
        var initialUpdatedAt = order.UpdatedAt;

        // Act
        var result = order.ChangeStatus(OrderStatus.Cancelled, DateTimeOffset.UtcNow);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot transition", result.Error);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(initialUpdatedAt, order.UpdatedAt);
        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void ChangeStatus_CancelledToAny_ShouldFail()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ChangeStatus(OrderStatus.Cancelled, DateTimeOffset.UtcNow);
        order.ClearDomainEvents();
        var initialUpdatedAt = order.UpdatedAt;

        // Act
        var result = order.ChangeStatus(OrderStatus.InProgress, DateTimeOffset.UtcNow);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot transition", result.Error);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(initialUpdatedAt, order.UpdatedAt);
        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        var initialUpdatedAt = order.UpdatedAt;
        var newTime = initialUpdatedAt.AddHours(1);

        // Act
        var result = order.ChangeStatus(OrderStatus.InProgress, newTime);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newTime, order.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_ShouldFail()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        var initialUpdatedAt = order.UpdatedAt;

        // Act
        var result = order.ChangeStatus(OrderStatus.New, DateTimeOffset.UtcNow);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order is already in the specified status.", result.Error);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.Equal(initialUpdatedAt, order.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_NewToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ClearDomainEvents();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = order.ChangeStatus(OrderStatus.Cancelled, now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        
        var domainEvents = order.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.IsType<OrderStatusChangedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void ChangeStatus_InProgressToDelivered_ShouldSucceed()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ChangeStatus(OrderStatus.InProgress, DateTimeOffset.UtcNow);
        order.ClearDomainEvents();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = order.ChangeStatus(OrderStatus.Delivered, now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Delivered, order.Status);
        
        var domainEvents = order.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.IsType<OrderStatusChangedDomainEvent>(domainEvents[0]);
    }

    [Fact]
    public void ChangeStatus_InProgressToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = Order.Create("ORD-001", "Test order", DateTimeOffset.UtcNow).Value!;
        order.ChangeStatus(OrderStatus.InProgress, DateTimeOffset.UtcNow);
        order.ClearDomainEvents();
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = order.ChangeStatus(OrderStatus.Cancelled, now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        
        var domainEvents = order.DomainEvents.ToList();
        Assert.Single(domainEvents);
        Assert.IsType<OrderStatusChangedDomainEvent>(domainEvents[0]);
    }
}
