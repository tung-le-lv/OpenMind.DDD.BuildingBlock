using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using MongoDB.Driver;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Repositories;
using Order.Domain.Specifications;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Repositories;

public class OrderRepository(OrderMongoDbContext context) : IOrderRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.OrderAggregate.Order?> GetByIdAsync(
        OrderId id,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.OrderAggregate.Order> AddAsync(
        Domain.Aggregates.OrderAggregate.Order aggregate,
        CancellationToken cancellationToken = default)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();
        context.AddCommand(() => context.Orders.InsertOneAsync(aggregate, cancellationToken: cancellationToken));
        return Task.FromResult(aggregate);
    }

    public void Update(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();
        context.AddCommand(() => context.Orders.ReplaceOneAsync(
            o => o.Id == aggregate.Id,
            aggregate));
    }

    public void Remove(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        context.AddCommand(() => context.Orders.DeleteOneAsync(o => o.Id == aggregate.Id));
    }

    public async Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.Id == id)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.CustomerId == customerId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.Status == status)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Find(o => o.Status == OrderStatus.Submitted)
            .SortBy(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> FindAsync(
        Specification<Domain.Aggregates.OrderAggregate.Order> specification,
        CancellationToken cancellationToken = default)
    {
        var expression = specification.ToExpression();
        return await context.Orders
            .Find(expression)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetOverdueOrdersAsync(
        int hoursThreshold = 24,
        CancellationToken cancellationToken = default)
    {
        var specification = new OverdueOrderSpecification(hoursThreshold);
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetCancellableOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new CancellableOrderSpecification();
        return await FindAsync(specification, cancellationToken);
    }
}

