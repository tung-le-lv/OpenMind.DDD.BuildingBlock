using BuildingBlocks.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(PaymentDbContext context) : IPaymentRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Domain.Aggregates.PaymentAggregate.Payment> AddAsync(
        Domain.Aggregates.PaymentAggregate.Payment aggregate,
        CancellationToken cancellationToken = default)
    {
        var entry = await context.Payments.AddAsync(aggregate, cancellationToken);
        return entry.Entity;
    }

    public void Update(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        context.Entry(aggregate).State = EntityState.Modified;
    }

    public void Remove(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        context.Payments.Remove(aggregate);
    }

    public async Task<bool> ExistsAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await context.Payments.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByOrderIdAsync(
        OrderReference orderId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByCustomerIdAsync(
        CustomerReference customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
