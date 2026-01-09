using BuildingBlocks.Domain;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Order Status using the Enumeration pattern from DDD.
/// This provides type-safe, behavior-rich status values.
/// </summary>
public class OrderStatus(int id, string name) : Enumeration(id, name)
{
    /// <summary>Order has been created but not yet submitted</summary>
    public static OrderStatus Draft = new(1, nameof(Draft));

    /// <summary>Order has been submitted and awaiting payment</summary>
    public static OrderStatus Submitted = new(2, nameof(Submitted));

    /// <summary>Payment has been confirmed</summary>
    public static OrderStatus Paid = new(3, nameof(Paid));

    /// <summary>Order is being processed for shipping</summary>
    public static OrderStatus Processing = new(4, nameof(Processing));

    /// <summary>Order has been shipped</summary>
    public static OrderStatus Shipped = new(5, nameof(Shipped));

    /// <summary>Order has been delivered</summary>
    public static OrderStatus Delivered = new(6, nameof(Delivered));

    /// <summary>Order has been cancelled</summary>
    public static OrderStatus Cancelled = new(7, nameof(Cancelled));

    /// <summary>Payment has failed</summary>
    public static OrderStatus PaymentFailed = new(8, nameof(PaymentFailed));

    /// <summary>
    /// Business rule: determines if the order can be cancelled from this status.
    /// </summary>
    public bool CanBeCancelled()
    {
        return this == Draft || this == Submitted || this == PaymentFailed;
    }

    /// <summary>
    /// Business rule: determines if items can be added in this status.
    /// </summary>
    public bool CanAddItems()
    {
        return Equals(this, Draft);
    }

    /// <summary>
    /// Business rule: determines if the order can be submitted.
    /// </summary>
    public bool CanBeSubmitted()
    {
        return Equals(this, Draft);
    }

    /// <summary>
    /// Business rule: determines if the order can transition to paid.
    /// </summary>
    public bool CanBePaid()
    {
        return Equals(this, Submitted);
    }
}
