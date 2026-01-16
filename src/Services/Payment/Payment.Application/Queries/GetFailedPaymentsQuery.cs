using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get failed payments that need retry or investigation.
/// Uses the FailedPaymentSpecification from the Domain layer.
/// </summary>
public record GetFailedPaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>;

public class GetFailedPaymentsQueryHandler(IPaymentRepository paymentRepository) 
    : IRequestHandler<GetFailedPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetFailedPaymentsQuery request, 
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetFailedPaymentsAsync(cancellationToken);

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
