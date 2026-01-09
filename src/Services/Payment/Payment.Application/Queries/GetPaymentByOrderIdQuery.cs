using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentByOrderIdQuery : IRequest<PaymentDto?>
{
    public Guid OrderId { get; init; }
}
