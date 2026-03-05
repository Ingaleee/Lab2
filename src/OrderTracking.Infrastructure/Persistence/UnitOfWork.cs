using OrderTracking.Application.Abstractions.Persistence;

namespace OrderTracking.Infrastructure.Persistence;

/// <summary>
/// Implements the Unit of Work pattern for Entity Framework Core.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
