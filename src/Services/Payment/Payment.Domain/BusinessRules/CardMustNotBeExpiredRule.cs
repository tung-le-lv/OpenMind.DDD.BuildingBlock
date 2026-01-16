using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Card must not be expired for payment processing.
/// </summary>
public class CardMustNotBeExpiredRule : IBusinessRule
{
    private readonly CardDetails? _cardDetails;

    public CardMustNotBeExpiredRule(CardDetails? cardDetails)
    {
        _cardDetails = cardDetails;
    }

    public bool IsBroken() => _cardDetails != null && _cardDetails.IsExpired();

    public string Message => "The card has expired. Please use a valid card.";
    
    public string Code => "CARD_EXPIRED";
}
