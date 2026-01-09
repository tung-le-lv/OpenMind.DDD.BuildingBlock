using MediatR;

namespace BuildingBlocks.Domain.SeedWork;

/// <summary>
/// Marker interface for Domain Events.
/// Domain Events capture things that happened in the domain that domain experts care about (Eric Evans, DDD).
/// 
/// Key characteristics:
/// 1. Named in past tense (e.g., OrderCreated, PaymentProcessed)
/// 2. Immutable - they represent historical facts
/// 3. Capture the essential data about what happened
/// 4. Published after the state change is committed
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event occurrence.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Type name of the event for serialization purposes.
    /// </summary>
    string EventType { get; }
}

/// <summary>
/// Base implementation of Domain Event with common properties.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
