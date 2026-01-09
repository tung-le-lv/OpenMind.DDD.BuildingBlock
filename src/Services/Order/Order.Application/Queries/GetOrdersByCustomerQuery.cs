using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetOrdersByCustomerQuery : IRequest<IReadOnlyList<OrderDto>>
{
    public Guid CustomerId { get; init; }
}
