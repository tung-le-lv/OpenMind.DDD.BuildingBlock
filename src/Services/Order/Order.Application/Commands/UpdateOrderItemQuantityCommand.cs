using MediatR;

namespace Order.Application.Commands;

public record UpdateOrderItemQuantityCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public Guid ItemId { get; init; }
    public int NewQuantity { get; init; }
}
