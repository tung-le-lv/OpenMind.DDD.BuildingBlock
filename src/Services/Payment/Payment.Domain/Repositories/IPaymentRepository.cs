using BuildingBlocks.Domain.SeedWork;
using Payment.Domain.Aggregates.PaymentAggregate;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Repositories;

/// <summary>
/// Repository interface for Payment Aggregate Root.
/// </summary>
public interface IPaymentRepository : IRepository<Aggregates.PaymentAggregate.Payment, PaymentId>
{
    /// <summary>
    /// Gets payment by order ID.
    /// </summary>
    Task<Aggregates.PaymentAggregate.Payment?> GetByOrderIdAsync(
        OrderReference orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments for a customer.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetByCustomerIdAsync(
        CustomerReference customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by status.
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending payments (for processing).
    /// </summary>
    Task<IReadOnlyList<Aggregates.PaymentAggregate.Payment>> GetPendingPaymentsAsync(
        CancellationToken cancellationToken = default);
}
