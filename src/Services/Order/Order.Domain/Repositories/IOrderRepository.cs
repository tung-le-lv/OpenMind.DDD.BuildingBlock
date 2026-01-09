using BuildingBlocks.Domain.SeedWork;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Domain.Repositories;

/// <summary>
/// Repository interface for Order Aggregate Root.
/// 
/// DDD Repository Pattern:
/// 1. One repository per Aggregate Root
/// 2. Provides collection-like interface
/// 3. Abstracts persistence mechanism
/// 4. Works only with Aggregate Roots
/// </summary>
public interface IOrderRepository : IRepository<Aggregates.OrderAggregate.Order, OrderId>
{
    /// <summary>
    /// Gets orders for a specific customer.
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetByCustomerIdAsync(
        CustomerId customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by status.
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending orders (submitted but not yet paid).
    /// </summary>
    Task<IReadOnlyList<Aggregates.OrderAggregate.Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default);
}
