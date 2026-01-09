using BuildingBlocks.Domain.SeedWork;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Events;

/// <summary>
/// Domain Event raised when a payment is created.
/// </summary>
public record PaymentCreatedDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public PaymentCreatedDomainEvent(PaymentId paymentId, OrderReference orderId, decimal amount, string currency)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
    }
}

/// <summary>
/// Domain Event raised when payment processing starts.
/// </summary>
public record PaymentProcessingStartedDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }

    public PaymentProcessingStartedDomainEvent(PaymentId paymentId, OrderReference orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}

/// <summary>
/// Domain Event raised when a payment is completed.
/// This event triggers integration event to notify Order Bounded Context.
/// </summary>
public record PaymentCompletedDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }
    public decimal Amount { get; }
    public DateTime CompletedAt { get; }

    public PaymentCompletedDomainEvent(PaymentId paymentId, OrderReference orderId, decimal amount, DateTime completedAt)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        CompletedAt = completedAt;
    }
}

/// <summary>
/// Domain Event raised when a payment fails.
/// This event triggers integration event to notify Order Bounded Context.
/// </summary>
public record PaymentFailedDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }
    public string Reason { get; }

    public PaymentFailedDomainEvent(PaymentId paymentId, OrderReference orderId, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}

/// <summary>
/// Domain Event raised when a payment is refunded.
/// </summary>
public record PaymentRefundedDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }
    public decimal Amount { get; }
    public string Reason { get; }

    public PaymentRefundedDomainEvent(PaymentId paymentId, OrderReference orderId, decimal amount, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
    }
}

/// <summary>
/// Domain Event raised when a payment is cancelled.
/// </summary>
public record PaymentCancelledDomainEvent : DomainEventBase
{
    public PaymentId PaymentId { get; }
    public OrderReference OrderId { get; }
    public string Reason { get; }

    public PaymentCancelledDomainEvent(PaymentId paymentId, OrderReference orderId, string reason)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Reason = reason;
    }
}
