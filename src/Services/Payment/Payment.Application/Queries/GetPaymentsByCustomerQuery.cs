using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentsByCustomerQuery : IRequest<IReadOnlyList<PaymentDto>>
{
    public Guid CustomerId { get; init; }
}
