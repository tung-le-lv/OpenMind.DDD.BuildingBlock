using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order cannot be cancelled after it has been shipped.
/// </summary>
public class OrderCannotBeCancelledAfterShippingRule : IBusinessRule
{
    private readonly Aggregates.OrderAggregate.OrderStatus _currentStatus;

    public OrderCannotBeCancelledAfterShippingRule(Aggregates.OrderAggregate.OrderStatus currentStatus)
    {
        _currentStatus = currentStatus;
    }

    public bool IsBroken() => !_currentStatus.CanBeCancelled();

    public string Message => $"Order cannot be cancelled when in '{_currentStatus.Name}' status. Only pending or submitted orders can be cancelled.";
    
    public string Code => "ORDER_CANNOT_BE_CANCELLED";
}
