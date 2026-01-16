using MediatR;
using Payment.Application.DTOs;
using Payment.Domain.Repositories;

namespace Payment.Application.Queries;

/// <summary>
/// Query to get high-value payments that may require additional verification.
/// Uses the HighValuePaymentSpecification from the Domain layer.
/// </summary>
public record GetHighValuePaymentsQuery : IRequest<IReadOnlyList<PaymentDto>>
{
    /// <summary>
    /// The amount threshold above which a payment is considered high-value.
    /// Default is 1000.
    /// </summary>
    public decimal Threshold { get; init; } = 1000m;
}

public class GetHighValuePaymentsQueryHandler(IPaymentRepository paymentRepository) 
    : IRequestHandler<GetHighValuePaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(
        GetHighValuePaymentsQuery request, 
        CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetHighValuePaymentsAsync(
            request.Threshold, 
            cancellationToken);

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
