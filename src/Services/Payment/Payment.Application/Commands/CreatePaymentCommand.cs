using MediatR;
using Payment.Application.DTOs;

namespace Payment.Application.Commands;

public record CreatePaymentCommand : IRequest<Guid>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Method { get; init; } = "CreditCard";
    public CardDetailsDto? CardDetails { get; init; }
}
