using Microsoft.EntityFrameworkCore;
using OrderTracking.Application.Abstractions.Persistence;
using OrderTracking.Domain.Orders;

namespace OrderTracking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="Order"/> entities using Entity Framework Core.
/// </summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public OrderRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Adds a new order to the database.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _db.Orders.AddAsync(order, cancellationToken);
    }

    /// <summary>
    /// Retrieves an order by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order if found, otherwise null.</returns>
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all orders, ordered by creation date descending.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only list of orders.</returns>
    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Orders
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
