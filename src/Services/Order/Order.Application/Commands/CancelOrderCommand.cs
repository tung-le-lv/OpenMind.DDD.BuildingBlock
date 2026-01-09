using MediatR;

namespace Order.Application.Commands;

public record CancelOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
