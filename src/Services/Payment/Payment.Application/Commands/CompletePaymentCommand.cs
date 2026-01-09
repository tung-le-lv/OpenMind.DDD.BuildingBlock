using MediatR;

namespace Payment.Application.Commands;

public record CompletePaymentCommand : IRequest<bool>
{
    public Guid PaymentId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}
