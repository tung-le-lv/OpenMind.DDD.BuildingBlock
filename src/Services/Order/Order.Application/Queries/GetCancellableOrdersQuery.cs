using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

/// <summary>
/// Query to get orders that can be cancelled.
/// Uses the CancellableOrderSpecification from the Domain layer.
/// </summary>
public record GetCancellableOrdersQuery : IRequest<IReadOnlyList<OrderDto>>;

public class GetCancellableOrdersQueryHandler(IOrderRepository orderRepository) 
    : IRequestHandler<GetCancellableOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetCancellableOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetCancellableOrdersAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    private static OrderDto MapToDto(Domain.Aggregates.OrderAggregate.Order order) => new()
    {
        Id = order.Id.Value,
        CustomerId = order.CustomerId.Value,
        Status = order.Status.Name,
        TotalAmount = order.TotalAmount.Amount,
        Currency = order.Currency,
        ShippingAddress = new AddressDto
        {
            Street = order.ShippingAddress.Street,
            City = order.ShippingAddress.City,
            State = order.ShippingAddress.State,
            Country = order.ShippingAddress.Country,
            ZipCode = order.ShippingAddress.ZipCode
        },
        Items = order.OrderItems.Select(item => new OrderItemDto
        {
            Id = item.Id.Value,
            ProductId = item.ProductId.Value,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice.Amount,
            Quantity = item.Quantity,
            Discount = item.Discount.Amount,
            Total = item.Total.Amount
        }).ToList(),
        Notes = order.Notes,
        CreatedAt = order.CreatedAt,
        SubmittedAt = order.SubmittedAt,
        PaidAt = order.PaidAt
    };
}
