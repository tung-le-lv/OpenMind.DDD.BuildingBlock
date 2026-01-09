using BuildingBlocks.Domain.SeedWork;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Entity representing an item within an Order.
/// 
/// DDD Principle: This is an Entity within the Order Aggregate.
/// It can only be accessed through the Order Aggregate Root.
/// It has its own identity but its lifecycle is managed by the Order.
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    /// <summary>
    /// Reference to the product in the Product Bounded Context.
    /// We store only the ID to maintain loose coupling between contexts.
    /// </summary>
    public ProductId ProductId { get; private set; }

    /// <summary>
    /// Product name at the time of ordering (snapshot).
    /// This protects the Order from changes in the Product context.
    /// </summary>
    public string ProductName { get; private set; }

    /// <summary>
    /// Unit price at the time of ordering (snapshot).
    /// </summary>
    public Money UnitPrice { get; private set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Optional discount applied to this item.
    /// </summary>
    public Money Discount { get; private set; }

    /// <summary>
    /// Calculated total for this item.
    /// </summary>
    public Money Total => CalculateTotal();

    private OrderItem() { } // EF Core

    /// <summary>
    /// Factory method for creating OrderItems.
    /// Encapsulates creation logic and enforces invariants.
    /// </summary>
    internal static OrderItem Create(
        ProductId productId,
        string productName,
        Money unitPrice,
        int quantity,
        Money? discount = null)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new OrderItem
        {
            Id = OrderItemId.New(),
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId)),
            ProductName = productName,
            UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice)),
            Quantity = quantity,
            Discount = discount ?? Money.Zero(unitPrice.Currency)
        };
    }

    /// <summary>
    /// Updates the quantity of this item.
    /// </summary>
    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
    }

    /// <summary>
    /// Adds quantity to this item.
    /// </summary>
    internal void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be positive", nameof(quantity));

        Quantity += quantity;
    }

    /// <summary>
    /// Applies a discount to this item.
    /// </summary>
    internal void ApplyDiscount(Money discount)
    {
        if (discount.Amount > UnitPrice.Amount * Quantity)
            throw new InvalidOperationException("Discount cannot exceed item total");

        Discount = discount;
    }

    private Money CalculateTotal()
    {
        var subtotal = UnitPrice.Multiply(Quantity);
        return subtotal - Discount;
    }
}
