using BuildingBlocks.Domain.BusinessRules;
using Order.Domain.ValueObjects;

namespace Order.Domain.BusinessRules;

/// <summary>
/// Business rule: Item quantity must be positive.
/// </summary>
public class ItemQuantityMustBePositiveRule : IBusinessRule
{
    private readonly int _quantity;

    public ItemQuantityMustBePositiveRule(int quantity)
    {
        _quantity = quantity;
    }

    public bool IsBroken() => _quantity <= 0;

    public string Message => $"Item quantity must be greater than zero. Provided: {_quantity}.";
    
    public string Code => "INVALID_ITEM_QUANTITY";
}
