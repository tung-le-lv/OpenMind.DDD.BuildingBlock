using BuildingBlocks.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext context) : IOrderRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.OrderAggregate.Order?> GetByIdAsync(
        OrderId id,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Domain.Aggregates.OrderAggregate.Order> AddAsync(
        Domain.Aggregates.OrderAggregate.Order aggregate,
        CancellationToken cancellationToken = default)
    {
        var entry = await context.Orders.AddAsync(aggregate, cancellationToken);
        return entry.Entity;
    }

    public void Update(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        context.Entry(aggregate).State = EntityState.Modified;
    }

    public void Remove(Domain.Aggregates.OrderAggregate.Order aggregate)
    {
        context.Orders.Remove(aggregate);
    }

    public async Task<bool> ExistsAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await context.Orders.AnyAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.OrderAggregate.Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Status == OrderStatus.Submitted)
            .OrderBy(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
