using BuildingBlocks.Domain.BusinessRules;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Order must have at least one item to be submitted.
/// </summary>
public class OrderMustHaveAtLeastOneItemRule : IBusinessRule
{
    private readonly int _itemCount;

    public OrderMustHaveAtLeastOneItemRule(int itemCount)
    {
        _itemCount = itemCount;
    }

    public bool IsBroken() => _itemCount < 1;

    public string Message => "Order must have at least one item before it can be submitted.";
    
    public string Code => "ORDER_EMPTY";
}
