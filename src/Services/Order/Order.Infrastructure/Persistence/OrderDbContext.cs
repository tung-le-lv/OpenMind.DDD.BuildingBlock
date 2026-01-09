using BuildingBlocks.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence.Configurations;

namespace Order.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Order Bounded Context.
/// Implements IUnitOfWork to coordinate persistence and domain event dispatch.
/// </summary>
public class OrderDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public DbSet<Domain.Aggregates.OrderAggregate.Order> Orders => Set<Domain.Aggregates.OrderAggregate.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrderDbContext(DbContextOptions<OrderDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
    }

    /// <summary>
    /// Saves changes and dispatches domain events.
    /// This ensures events are only dispatched after successful persistence.
    /// </summary>
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        // This allows handlers to modify data before it's committed
        await DispatchDomainEventsAsync(cancellationToken);

        // Save changes
        await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Get all entities with domain events
        var domainEntities = ChangeTracker
            .Entries<Entity<OrderId>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var orderItemEntities = ChangeTracker
            .Entries<Entity<OrderItemId>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        // Collect all domain events
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .Concat(orderItemEntities.SelectMany(x => x.Entity.DomainEvents))
            .ToList();

        // Clear events from entities
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());
        orderItemEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // Dispatch events through MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
