namespace BuildingBlocks.Integration.EventBus;

using BuildingBlocks.Integration.Events;

/// <summary>
/// Interface for the Event Bus.
/// The Event Bus is responsible for publishing Integration Events between Bounded Contexts.
/// This is part of the Anti-Corruption Layer pattern in DDD.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the message broker.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    /// <summary>
    /// Subscribes to an integration event type.
    /// </summary>
    void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>;
}

/// <summary>
/// Interface for handling Integration Events.
/// </summary>
public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
