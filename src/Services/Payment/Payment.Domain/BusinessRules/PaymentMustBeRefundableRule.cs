using BuildingBlocks.Domain.BusinessRules;
using Payment.Domain.Aggregates.PaymentAggregate;

namespace Payment.Domain.BusinessRules;

/// <summary>
/// Business rule: Payment can only be refunded when in completed status.
/// </summary>
public class PaymentMustBeRefundableRule : IBusinessRule
{
    private readonly PaymentStatus _currentStatus;

    public PaymentMustBeRefundableRule(PaymentStatus currentStatus)
    {
        _currentStatus = currentStatus;
    }

    public bool IsBroken() => !_currentStatus.CanBeRefunded();

    public string Message => $"Payment cannot be refunded when in '{_currentStatus.Name}' status. Only completed payments can be refunded.";
    
    public string Code => "PAYMENT_NOT_REFUNDABLE";
}
