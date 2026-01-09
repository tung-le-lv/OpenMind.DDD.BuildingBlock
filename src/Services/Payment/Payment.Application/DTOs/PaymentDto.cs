namespace Payment.Application.DTOs;

public record PaymentDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public CardDetailsDto? CardDetails { get; init; }
    public string? TransactionId { get; init; }
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
