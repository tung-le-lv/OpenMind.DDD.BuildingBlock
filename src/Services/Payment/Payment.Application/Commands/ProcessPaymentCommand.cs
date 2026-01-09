using MediatR;

namespace Payment.Application.Commands;

public record ProcessPaymentCommand : IRequest<bool>
{
    public Guid PaymentId { get; init; }
}
