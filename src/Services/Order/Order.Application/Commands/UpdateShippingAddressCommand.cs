using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Commands;

public record UpdateShippingAddressCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public AddressDto NewAddress { get; init; } = null!;
}
