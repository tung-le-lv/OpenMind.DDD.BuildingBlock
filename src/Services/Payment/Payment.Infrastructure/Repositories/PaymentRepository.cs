using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Specifications;
using MongoDB.Driver;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.Repositories;
using Payment.Domain.Specifications;
using Payment.Domain.ValueObjects;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(PaymentMongoDbContext context) : IPaymentRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Domain.Aggregates.PaymentAggregate.Payment> AddAsync(
        Domain.Aggregates.PaymentAggregate.Payment aggregate,
        CancellationToken cancellationToken = default)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();
        context.AddCommand(() => context.Payments.InsertOneAsync(aggregate, cancellationToken: cancellationToken));
        return Task.FromResult(aggregate);
    }

    public void Update(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        context.AddDomainEvents(aggregate.DomainEvents);
        aggregate.ClearDomainEvents();
        context.AddCommand(() => context.Payments.ReplaceOneAsync(
            p => p.Id == aggregate.Id,
            aggregate));
    }

    public void Remove(Domain.Aggregates.PaymentAggregate.Payment aggregate)
    {
        context.AddCommand(() => context.Payments.DeleteOneAsync(p => p.Id == aggregate.Id));
    }

    public async Task<bool> ExistsAsync(PaymentId id, CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.Id == id)
            .AnyAsync(cancellationToken);
    }

    public async Task<Domain.Aggregates.PaymentAggregate.Payment?> GetByOrderIdAsync(
        OrderReference orderId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByCustomerIdAsync(
        CustomerReference customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.CustomerId == customerId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .Find(p => p.Status == status)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new PendingPaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> FindAsync(
        Specification<Domain.Aggregates.PaymentAggregate.Payment> specification,
        CancellationToken cancellationToken = default)
    {
        var expression = specification.ToExpression();
        return await context.Payments
            .Find(expression)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetRefundablePaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new RefundablePaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetFailedPaymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var specification = new FailedPaymentSpecification();
        return await FindAsync(specification, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Aggregates.PaymentAggregate.Payment>> GetHighValuePaymentsAsync(
        decimal threshold = 1000m,
        CancellationToken cancellationToken = default)
    {
        var specification = new HighValuePaymentSpecification(threshold);
        return await FindAsync(specification, cancellationToken);
    }
}

