using BuildingBlocks.Domain;
using Order.Domain.Aggregates.OrderAggregate;
using Order.Domain.Specifications;
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
    /// Uses MinimumOrderValueSpecification to ensure minimum order value.
    /// </summary>
    bool IsDiscountApplicable(Aggregates.OrderAggregate.Order order, string discountCode);

    /// <summary>
    /// Checks if an order qualifies for free shipping.
    /// Uses MinimumOrderValueSpecification with a threshold.
    /// </summary>
    bool QualifiesForFreeShipping(Aggregates.OrderAggregate.Order order, decimal freeShippingThreshold = 100m);
}

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
        if (string.IsNullOrEmpty(discountCode))
            return false;

        // Use Specification pattern to check minimum order value for discount eligibility
        var minimumValueSpec = new MinimumOrderValueSpecification(50m);
        var draftStatusSpec = order.Status == OrderStatus.Draft;
        
        return draftStatusSpec && minimumValueSpec.IsSatisfiedBy(order);
    }

    public bool QualifiesForFreeShipping(Aggregates.OrderAggregate.Order order, decimal freeShippingThreshold = 100m)
    {
        // Use Specification pattern to check if order meets free shipping threshold
        var freeShippingSpec = new MinimumOrderValueSpecification(freeShippingThreshold);
        return freeShippingSpec.IsSatisfiedBy(order);
    }
}
