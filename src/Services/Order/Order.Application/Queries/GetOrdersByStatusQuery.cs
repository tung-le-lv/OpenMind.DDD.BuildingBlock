using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrdersByStatusQuery : IRequest<IReadOnlyList<OrderDto>>
{
    public string Status { get; init; } = string.Empty;
}
