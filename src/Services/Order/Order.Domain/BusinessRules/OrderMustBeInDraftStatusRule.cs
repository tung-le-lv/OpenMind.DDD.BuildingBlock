using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order can only be modified when in Draft status.
/// </summary>
public class OrderMustBeInDraftStatusRule : IBusinessRule
{
    private readonly Aggregates.OrderAggregate.OrderStatus _currentStatus;

    public OrderMustBeInDraftStatusRule(Aggregates.OrderAggregate.OrderStatus currentStatus)
    {
        _currentStatus = currentStatus;
    }

    public bool IsBroken() => !_currentStatus.CanAddItems();

    public string Message => $"Order cannot be modified when in '{_currentStatus.Name}' status. Only draft orders can be modified.";
    
    public string Code => "ORDER_NOT_MODIFIABLE";
}
