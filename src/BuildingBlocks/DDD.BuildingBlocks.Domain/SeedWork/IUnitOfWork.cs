namespace BuildingBlocks.Domain.SeedWork;

/// <summary>
/// Unit of Work pattern interface.
/// Maintains a list of objects affected by a business transaction and coordinates
/// the writing out of changes and resolution of concurrency problems (Martin Fowler).
/// 
/// In DDD context:
/// 1. Ensures Aggregate boundaries are respected during persistence
/// 2. Dispatches Domain Events after successful commit
/// 3. Provides transactional boundaries
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// Also dispatches any domain events raised by the aggregates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes with domain events being dispatched.
    /// </summary>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
