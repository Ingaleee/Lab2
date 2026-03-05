using OrderTracking.Domain.Orders;

namespace OrderTracking.Application.Abstractions.Persistence;

/// <summary>Repository for working with orders aggregate.</summary>
public interface IOrderRepository
{
    /// <summary>Adds a new order to the store.</summary>
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>Returns order by id or null if it does not exist.</summary>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all orders.</summary>
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
}
