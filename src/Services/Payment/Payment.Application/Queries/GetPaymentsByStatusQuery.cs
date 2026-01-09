using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Queries;

public record GetPaymentsByStatusQuery : IRequest<IReadOnlyList<PaymentDto>>
{
    public string Status { get; init; } = string.Empty;
}
