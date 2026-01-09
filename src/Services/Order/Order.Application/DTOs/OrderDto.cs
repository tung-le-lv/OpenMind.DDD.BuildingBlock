namespace Order.Application.DTOs;

public record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public AddressDto ShippingAddress { get; init; } = null!;
    public List<OrderItemDto> Items { get; init; } = new();
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
