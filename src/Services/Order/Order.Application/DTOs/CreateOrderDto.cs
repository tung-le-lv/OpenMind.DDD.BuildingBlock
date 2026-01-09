namespace Order.Application.DTOs;

public record CreateOrderDto
{
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public string Currency { get; init; } = "USD";
    public string? Notes { get; init; }
}
