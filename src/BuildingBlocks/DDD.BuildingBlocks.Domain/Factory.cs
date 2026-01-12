namespace BuildingBlocks.Domain;

/// <summary>
/// Factory Pattern implementation for DDD.
/// 
/// Factories encapsulate the knowledge needed to create complex objects or aggregates.
/// Key benefits:
/// 1. Encapsulates complex creation logic
/// 2. Ensures all invariants are satisfied at creation time
/// 3. Hides implementation details of aggregate construction
/// 4. Provides a clear entry point for object creation
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate to create</typeparam>
/// <typeparam name="TId">The type of the aggregate's identifier</typeparam>
public interface IFactory<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
}

/// <summary>
/// Abstract base class for factories that create aggregates.
/// Provides common functionality for aggregate creation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate to create</typeparam>
/// <typeparam name="TId">The type of the aggregate's identifier</typeparam>
public abstract class Factory<TAggregate, TId> : IFactory<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Validates the creation parameters before creating the aggregate.
    /// Override this method to add custom validation logic.
    /// </summary>
    protected virtual void Validate()
    {
    }

    /// <summary>
    /// Template method for creating an aggregate with validation.
    /// </summary>
    protected TAggregate CreateWithValidation(Func<TAggregate> createFunc)
    {
        Validate();
        return createFunc();
    }
}
