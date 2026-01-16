using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order cannot exceed maximum items limit.
/// </summary>
public class OrderCannotExceedMaxItemsRule : IBusinessRule
{
    private readonly int _currentItemCount;
    private readonly int _maxItems;

    public OrderCannotExceedMaxItemsRule(int currentItemCount, int maxItems = 100)
    {
        _currentItemCount = currentItemCount;
        _maxItems = maxItems;
    }

    public bool IsBroken() => _currentItemCount >= _maxItems;

    public string Message => $"Order cannot have more than {_maxItems} items. Current count: {_currentItemCount}.";
    
    public string Code => "ORDER_MAX_ITEMS_EXCEEDED";
}
