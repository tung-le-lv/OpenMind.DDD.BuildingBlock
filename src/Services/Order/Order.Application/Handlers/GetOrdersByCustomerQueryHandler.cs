using MediatR;
using Order.Application.DTOs;
using Order.Application.Queries;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Handlers;

public class GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(
            CustomerId.From(request.CustomerId), 
            cancellationToken);

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
