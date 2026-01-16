using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Payment can only be processed when in a valid status.
/// </summary>
public class PaymentMustBeProcessableRule : IBusinessRule
{
    private readonly PaymentStatus _currentStatus;

    public PaymentMustBeProcessableRule(PaymentStatus currentStatus)
    {
        _currentStatus = currentStatus;
    }

    public bool IsBroken() => !_currentStatus.CanBeProcessed();

    public string Message => $"Payment cannot be processed when in '{_currentStatus.Name}' status.";
    
    public string Code => "PAYMENT_NOT_PROCESSABLE";
}
