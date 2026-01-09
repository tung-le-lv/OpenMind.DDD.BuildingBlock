using MediatR;

namespace Order.Application.Commands;

public record RemoveOrderItemCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
}
