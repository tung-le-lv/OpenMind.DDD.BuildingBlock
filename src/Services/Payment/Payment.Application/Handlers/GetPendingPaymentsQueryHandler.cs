using MediatR;
using Payment.Application.DTOs;
using Payment.Application.Queries;
using Payment.Domain.Repositories;

namespace Payment.Application.Handlers;

public class GetPendingPaymentsQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPendingPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPendingPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetPendingPaymentsAsync(cancellationToken);

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
        TransactionId = payment.TransactionId,
        FailureReason = payment.FailureReason,
        CreatedAt = payment.CreatedAt,
        ProcessedAt = payment.ProcessedAt,
        CompletedAt = payment.CompletedAt
    };
}
