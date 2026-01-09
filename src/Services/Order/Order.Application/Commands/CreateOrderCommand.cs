using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands;

public record CreateOrderCommand : IRequest<Guid>
{
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public string Currency { get; init; } = "USD";
    public string? Notes { get; init; }
}
