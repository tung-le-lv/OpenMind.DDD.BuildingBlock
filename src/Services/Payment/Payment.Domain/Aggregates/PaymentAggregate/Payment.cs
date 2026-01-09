using BuildingBlocks.Domain.SeedWork;
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
    /// Reference to the Order in the Order Bounded Context.
    /// We only store the ID to maintain loose coupling.
    /// </summary>
    public OrderReference OrderId { get; private set; }

    /// <summary>
    /// Reference to the Customer.
    /// </summary>
    public CustomerReference CustomerId { get; private set; }

    /// <summary>
    /// Amount to be paid.
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Current payment status.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// Payment method used.
    /// </summary>
    public PaymentMethod Method { get; private set; }

    /// <summary>
    /// Card details (if card payment).
    /// </summary>
    public CardDetails? CardDetails { get; private set; }

    /// <summary>
    /// External transaction ID from payment gateway.
    /// </summary>
    public string? TransactionId { get; private set; }

    /// <summary>
    /// Failure reason if payment failed.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// When the payment was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the payment was processed.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// When the payment was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    private Payment() { }

    #region Factory Methods

    /// <summary>
    /// Creates a new Payment for an Order.
    /// Factory method ensures all required data is present.
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

        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent(
            payment.Id,
            payment.OrderId,
            payment.Amount.Amount,
            payment.Amount.Currency));

        return payment;
    }

    #endregion

    #region Behavior Methods

    /// <summary>
    /// Starts processing the payment.
    /// </summary>
    public void StartProcessing()
    {
        if (!Status.CanBeProcessed())
            throw new InvalidOperationException($"Cannot process payment in {Status.Name} status");

        // Business rule: Check if card is expired
        if (CardDetails != null && CardDetails.IsExpired())
            throw new InvalidOperationException("Card has expired");

        Status = PaymentStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
        IncrementVersion();

        RaiseDomainEvent(new PaymentProcessingStartedDomainEvent(Id, OrderId));
    }

    /// <summary>
    /// Marks the payment as completed.
    /// This triggers an integration event to notify the Order service.
    /// </summary>
    public void Complete(string transactionId)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment in {Status.Name} status");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required", nameof(transactionId));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
        IncrementVersion();

        // This domain event will trigger an integration event to Order service
        RaiseDomainEvent(new PaymentCompletedDomainEvent(
            Id,
            OrderId,
            Amount.Amount,
            CompletedAt.Value));
    }

    /// <summary>
    /// Marks the payment as failed.
    /// </summary>
    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail payment in {Status.Name} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required", nameof(reason));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        IncrementVersion();

        // This domain event will trigger an integration event to Order service
        RaiseDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, reason));
    }

    /// <summary>
    /// Refunds the payment.
    /// </summary>
    public void Refund(string reason)
    {
        if (!Status.CanBeRefunded())
            throw new InvalidOperationException($"Cannot refund payment in {Status.Name} status");

        Status = PaymentStatus.Refunded;
        IncrementVersion();

        RaiseDomainEvent(new PaymentRefundedDomainEvent(Id, OrderId, Amount.Amount, reason));
    }

    /// <summary>
    /// Cancels the payment.
    /// </summary>
    public void Cancel(string reason)
    {
        if (!Status.CanBeCancelled())
            throw new InvalidOperationException($"Cannot cancel payment in {Status.Name} status");

        Status = PaymentStatus.Cancelled;
        FailureReason = reason;
        IncrementVersion();

        RaiseDomainEvent(new PaymentCancelledDomainEvent(Id, OrderId, reason));
    }

    #endregion
}
