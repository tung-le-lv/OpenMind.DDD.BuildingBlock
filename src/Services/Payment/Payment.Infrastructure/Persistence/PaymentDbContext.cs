using BuildingBlocks.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.ValueObjects;
using Payment.Infrastructure.Persistence.Configurations;

namespace Payment.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Payment Bounded Context.
/// </summary>
public class PaymentDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public DbSet<Domain.Aggregates.PaymentAggregate.Payment> Payments => Set<Domain.Aggregates.PaymentAggregate.Payment>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        await base.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity<PaymentId>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
