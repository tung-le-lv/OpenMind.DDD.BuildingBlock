namespace BuildingBlocks.Integration.Events;

/// <summary>
/// Base class for Integration Events.
/// 
/// Integration Events are used for communication between Bounded Contexts (Eric Evans, DDD).
/// Unlike Domain Events which are internal to a Bounded Context, Integration Events:
/// 1. Cross Bounded Context boundaries
/// 2. Are published through a message broker
/// 3. Use a format that both contexts can understand
/// 4. Part of the Anti-Corruption Layer pattern
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}