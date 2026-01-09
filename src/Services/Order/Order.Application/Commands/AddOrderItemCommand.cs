using MediatR;

namespace Order.Application.Commands;

public record AddOrderItemCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
}
