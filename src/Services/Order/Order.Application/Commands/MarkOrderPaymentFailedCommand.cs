using MediatR;

namespace Order.Application.Commands;

public record MarkOrderPaymentFailedCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
