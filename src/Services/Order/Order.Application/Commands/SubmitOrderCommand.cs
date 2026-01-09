using MediatR;

namespace Order.Application.Commands;

public record SubmitOrderCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
}
