using BuildingBlocks.Domain;
using Payment.Domain.Events;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Aggregates.PaymentAggregate;

/// <summary>
/// Payment Aggregate Root.
/// 
/// DDD Principles:
/// 1. This is the entry point for the Payment Aggregate
/// 2. All state changes go through methods that enforce business rules
/// 3. Domain events are raised for significant state changes
/// 4. The aggregate maintains its own consistency
/// </summary>
public class Payment : AggregateRoot<PaymentId>
{
    /// <summary>
    /// Reference to Order Bounded Context - loose coupling via ID only.
    /// </summary>
    public OrderReference OrderId { get; private set; }

    public CustomerReference CustomerId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public CardDetails? CardDetails { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Payment() { }

    #region Factory Methods

    /// <summary>
    /// Factory method - ensures all required data is present and invariants are met.
    /// </summary>
    public static Payment CreateForOrder(
        OrderReference orderId,
        CustomerReference customerId,
        Money amount,
        PaymentMethod method,
        CardDetails? cardDetails = null)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be positive", nameof(amount));

        // Business rule: Card payments require card details
        if ((method == PaymentMethod.CreditCard || method == PaymentMethod.DebitCard) && cardDetails == null)
            throw new ArgumentException("Card details are required for card payments", nameof(cardDetails));

        var payment = new Payment
        {
            Id = PaymentId.New(),
            OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId)),
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId)),
            Amount = amount ?? throw new ArgumentNullException(nameof(amount)),
            Status = PaymentStatus.Pending,
            Method = method,
            CardDetails = cardDetails,
            CreatedAt = DateTime.UtcNow
        };

        payment.Emit(new PaymentCreatedDomainEvent(
            payment.Id,
            payment.OrderId,
            payment.Amount.Amount,
            payment.Amount.Currency));

        return payment;
    }

    #endregion

    #region Behavior Methods

    public void StartProcessing()
    {
        if (!Status.CanBeProcessed())
            throw new DomainException($"Cannot process payment in {Status.Name} status");

        // Business rule: Check if card is expired
        if (CardDetails != null && CardDetails.IsExpired())
            throw new DomainException("Card has expired");

        Status = PaymentStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
        IncrementVersion();

        Emit(new PaymentProcessingStartedDomainEvent(Id, OrderId));
    }

    /// <summary>
    /// Triggers integration event to notify Order Bounded Context.
    /// </summary>
    public void Complete(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new DomainException($"Cannot complete payment in {Status.Name} status");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required", nameof(transactionId));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
        IncrementVersion();

        // This domain event will trigger an integration event to Order service
        Emit(new PaymentCompletedDomainEvent(
            Id,
            OrderId,
            Amount.Amount,
            CompletedAt.Value));
    }

    /// <summary>
    /// Triggers integration event to notify Order Bounded Context.
    /// </summary>
    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new DomainException($"Cannot fail payment in {Status.Name} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required", nameof(reason));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        IncrementVersion();

        // This domain event will trigger an integration event to Order service
        Emit(new PaymentFailedDomainEvent(Id, OrderId, reason));
    }

    public void Refund(string reason)
    {
        if (!Status.CanBeRefunded())
            throw new DomainException($"Cannot refund payment in {Status.Name} status");

        Status = PaymentStatus.Refunded;
        IncrementVersion();

        Emit(new PaymentRefundedDomainEvent(Id, OrderId, Amount.Amount, reason));
    }

    public void Cancel(string reason)
    {
        if (!Status.CanBeCancelled())
            throw new DomainException($"Cannot cancel payment in {Status.Name} status");

        Status = PaymentStatus.Cancelled;
        FailureReason = reason;
        IncrementVersion();

        Emit(new PaymentCancelledDomainEvent(Id, OrderId, reason));
    }

    #endregion
}
