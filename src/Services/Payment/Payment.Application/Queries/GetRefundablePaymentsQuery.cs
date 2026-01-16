using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get payments that can be refunded.
/// Uses the RefundablePaymentSpecification from the Domain layer.
/// </summary>
public record GetRefundablePaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetRefundablePaymentsQueryHandler(IPaymentRepository paymentRepository) 
    : IRequestHandler<GetRefundablePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetRefundablePaymentsQuery request, 
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetRefundablePaymentsAsync(cancellationToken);

        return payments.Select(MapToDto).ToList();
    }

    private static PaymentDto MapToDto(Domain.Aggregates.PaymentAggregate.Payment payment) => new()
    {
        Id = payment.Id.Value,
        OrderId = payment.OrderId.Value,
        CustomerId = payment.CustomerId.Value,
        Amount = payment.Amount.Amount,
        Currency = payment.Amount.Currency,
        Status = payment.Status.Name,
        Method = payment.Method.Name,
        CardDetails = payment.CardDetails != null ? new CardDetailsDto
        {
            Last4Digits = payment.CardDetails.Last4Digits,
            CardType = payment.CardDetails.CardType,
            ExpiryMonth = payment.CardDetails.ExpiryMonth,
            ExpiryYear = payment.CardDetails.ExpiryYear
        } : null,
        TransactionId = payment.TransactionId,
        FailureReason = payment.FailureReason,
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt,
        CompletedAt = payment.CompletedAt
    };
}
