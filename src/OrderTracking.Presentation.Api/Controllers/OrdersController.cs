using Microsoft.AspNetCore.Mvc;
using OrderTracking.Application.Abstractions.Outbox;
using OrderTracking.Application.Abstractions.Persistence;
using OrderTracking.Application.Abstractions.Time;
using OrderTracking.Application.Common.Errors;
using OrderTracking.Contracts.IntegrationEvents;
using OrderTracking.Contracts.Orders;
using OrderTracking.Domain.Orders;
using OrderTracking.Presentation.Api.Generated;

namespace OrderTracking.Presentation.Api.Controllers;

/// <summary>
/// Controller for managing orders.
/// Inherits the API contract from the generated <see cref="OrdersControllerBase"/>
/// which is produced from the OpenAPI specification (openapi.yaml).
/// </summary>
[ApiController]
public sealed class OrdersController : OrdersControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly IOutboxStore _outboxStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    public OrdersController(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        IOutboxStore outboxStore)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _outboxStore = outboxStore;
    }

    /// <inheritdoc />
    public override async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest body,
        CancellationToken cancellationToken)
    {
        var orderResult = Order.Create(body.OrderNumber, body.Description, _clock.UtcNow);
        if (!orderResult.IsSuccess || orderResult.Value is null)
        {
            return BadRequest(new { error = orderResult.Error });
        }

        await _orderRepository.AddAsync(orderResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = MapToResponse(orderResult.Value);
        return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, response);
    }

    /// <inheritdoc />
    public override async Task<ActionResult<ICollection<OrderResponse>>> GetOrders(
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        var response = orders.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <inheritdoc />
    public override async Task<ActionResult<OrderResponse>> GetOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException($"Order with ID '{id}' not found.");
        }

        return Ok(MapToResponse(order));
    }

    /// <inheritdoc />
    public override async Task<ActionResult<OrderResponse>> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest body,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new NotFoundException($"Order with ID '{id}' not found.");
        }

        if (!Enum.TryParse<OrderStatus>(body.Status, true, out var newStatus))
        {
            throw new ConflictException($"Invalid status value: '{body.Status}'.");
        }

        var oldStatus = order.Status;
        var result = order.ChangeStatus(newStatus, _clock.UtcNow);
        if (!result.IsSuccess)
        {
            throw new ConflictException(result.Error!);
        }

        var integrationEvent = new OrderStatusChangedIntegrationEventV1
        {
            EventId = Guid.NewGuid(),
            OccurredAt = _clock.UtcNow,
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OldStatus = oldStatus.ToString(),
            NewStatus = newStatus.ToString()
        };

        await _outboxStore.EnqueueAsync(integrationEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(MapToResponse(order));
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Description = order.Description,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}
