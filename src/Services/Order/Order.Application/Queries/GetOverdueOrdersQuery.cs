using MediatR;
using Order.Application.DTOs;
using Order.Domain.Repositories;

namespace Order.Application.Queries;

/// <summary>
/// Query to get orders that are overdue (submitted but not paid within threshold).
/// Uses the OverdueOrderSpecification from the Domain layer.
/// </summary>
public record GetOverdueOrdersQuery : IRequest<IReadOnlyList<OrderDto>>
{
    /// <summary>
    /// Number of hours after submission to consider an order overdue.
    /// Default is 24 hours.
    /// </summary>
    public int HoursThreshold { get; init; } = 24;
}

public class GetOverdueOrdersQueryHandler(IOrderRepository orderRepository) 
    : IRequestHandler<GetOverdueOrdersQuery, IReadOnlyList<OrderDto>>
{
    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetOverdueOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetOverdueOrdersAsync(
            request.HoursThreshold, 
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
