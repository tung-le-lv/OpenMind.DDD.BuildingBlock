using BuildingBlocks.Domain.BusinessRules;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Payment amount must be positive.
/// </summary>
public class PaymentAmountMustBePositiveRule : IBusinessRule
{
    private readonly decimal _amount;

    public PaymentAmountMustBePositiveRule(decimal amount)
    {
        _amount = amount;
    }

    public bool IsBroken() => _amount <= 0;

    public string Message => $"Payment amount must be greater than zero. Provided: {_amount}.";
    
    public string Code => "INVALID_PAYMENT_AMOUNT";
}
