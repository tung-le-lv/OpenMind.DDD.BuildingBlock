namespace BuildingBlocks.Domain.SeedWork;

/// <summary>
/// Generic Repository interface following DDD principles (Eric Evans).
/// 
/// Repository Pattern in DDD:
/// 1. Provides an illusion of an in-memory collection of Aggregate Roots
/// 2. Abstracts the underlying persistence mechanism
/// 3. One Repository per Aggregate Root
/// 4. Repositories work only with Aggregate Roots, not with entities inside the aggregate
/// 5. Returns fully reconstituted Aggregates
/// </summary>
public interface IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// The Unit of Work that coordinates changes across repositories.
    /// </summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Retrieves an Aggregate by its identity.
    /// Returns null if not found.
    /// </summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new Aggregate to the repository.
    /// The Aggregate will be persisted when UnitOfWork.SaveChangesAsync is called.
    /// </summary>
    Task<TAggregate> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Aggregate in the repository.
    /// </summary>
    void Update(TAggregate aggregate);

    /// <summary>
    /// Removes an Aggregate from the repository.
    /// </summary>
    void Remove(TAggregate aggregate);

    /// <summary>
    /// Checks if an Aggregate with the given identity exists.
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
