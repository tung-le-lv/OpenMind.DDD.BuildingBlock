namespace Payment.Application.DTOs;

public record ProcessPaymentDto
{
    public Guid PaymentId { get; init; }
}
