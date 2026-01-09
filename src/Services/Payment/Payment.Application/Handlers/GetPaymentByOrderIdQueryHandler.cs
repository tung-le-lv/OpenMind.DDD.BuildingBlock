using MediatR;
using Payment.Application.DTOs;
using Payment.Application.Queries;
using Payment.Domain.Repositories;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Handlers;

public class GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByOrderIdAsync(
            OrderReference.From(request.OrderId),
            cancellationToken);

        if (payment == null)
            return null;

        return MapToDto(payment);
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
        CardDetails = payment.CardDetails != null
            ? new CardDetailsDto
            {
                Last4Digits = payment.CardDetails.Last4Digits,
                CardType = payment.CardDetails.CardType,
                ExpiryMonth = payment.CardDetails.ExpiryMonth,
                ExpiryYear = payment.CardDetails.ExpiryYear,
                CardHolderName = payment.CardDetails.CardHolderName
            }
            : null,
        TransactionId = payment.TransactionId,
        FailureReason = payment.FailureReason,
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt,
        CompletedAt = payment.CompletedAt
    };
}
