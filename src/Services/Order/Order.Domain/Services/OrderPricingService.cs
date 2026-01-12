using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.ValueObjects;

namespace Order.Domain.Services;

/// <summary>
/// Domain Service for Order pricing calculations.
/// 
/// DDD Domain Service:
/// 1. Contains domain logic that doesn't naturally fit in an Entity or Value Object
/// 2. Operates on multiple aggregates or external data
/// 3. Stateless
/// 4. Named using Ubiquitous Language
/// </summary>
public interface IOrderPricingService : IDomainService
{
    /// <summary>
    /// Calculates the total price including discounts and taxes.
    /// </summary>
    Money CalculateFinalPrice(Aggregates.OrderAggregate.Order order, decimal discountPercentage = 0);

    /// <summary>
    /// Validates if a discount code is applicable to the order.
    /// </summary>
    bool IsDiscountApplicable(Aggregates.OrderAggregate.Order order, string discountCode);
}

/// <summary>
/// Implementation of the Order Pricing Domain Service.
/// </summary>
public class OrderPricingService : IOrderPricingService
{
    public Money CalculateFinalPrice(Aggregates.OrderAggregate.Order order, decimal discountPercentage = 0)
    {
        var subtotal = order.TotalAmount;

        if (discountPercentage is > 0 and <= 100)
        {
            var discountAmount = subtotal.Amount * (discountPercentage / 100);
            return new Money(subtotal.Amount - discountAmount, subtotal.Currency);
        }

        return subtotal;
    }

    public bool IsDiscountApplicable(Aggregates.OrderAggregate.Order order, string discountCode)
    {
        // Domain logic for discount validation
        // In a real application, this might check against discount rules
        return !string.IsNullOrEmpty(discountCode) &&
               order.Status == OrderStatus.Draft &&
               order.TotalAmount.Amount >= 50; // Minimum order amount for discount
    }
}
