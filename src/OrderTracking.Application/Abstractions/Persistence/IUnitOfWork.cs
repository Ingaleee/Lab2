namespace OrderTracking.Application.Abstractions.Persistence;

/// <summary>Unit of work abstraction for persisting changes.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists changes to the underlying storage.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
