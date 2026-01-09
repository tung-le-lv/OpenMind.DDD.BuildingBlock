using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentByIdQuery : IRequest<PaymentDto?>
{
    public Guid PaymentId { get; init; }
}
