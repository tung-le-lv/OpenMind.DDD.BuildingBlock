using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid OrderId { get; init; }
}
