using MediatR;

namespace Payment.Application.Commands;

public record FailPaymentCommand : IRequest<bool>
{
    public Guid PaymentId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
