using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Card payments require card details.
/// </summary>
public class CardPaymentMustHaveCardDetailsRule : IBusinessRule
{
    private readonly PaymentMethod _method;
    private readonly bool _hasCardDetails;

    public CardPaymentMustHaveCardDetailsRule(PaymentMethod method, bool hasCardDetails)
    {
        _method = method;
        _hasCardDetails = hasCardDetails;
    }

    public bool IsBroken()
    {
        var isCardPayment = _method == PaymentMethod.CreditCard || _method == PaymentMethod.DebitCard;
        return isCardPayment && !_hasCardDetails;
    }

    public string Message => "Card details are required for credit card and debit card payments.";
    
    public string Code => "CARD_DETAILS_REQUIRED";
}
