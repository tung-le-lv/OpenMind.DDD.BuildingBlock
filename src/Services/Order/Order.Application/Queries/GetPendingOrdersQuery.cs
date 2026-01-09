using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public record GetPendingOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;
