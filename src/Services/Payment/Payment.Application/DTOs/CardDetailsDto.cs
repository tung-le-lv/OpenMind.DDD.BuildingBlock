namespace Payment.Application.DTOs;

public record CardDetailsDto
{
    public string Last4Digits { get; init; } = string.Empty;
    public string CardType { get; init; } = string.Empty;
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string CardHolderName { get; init; } = string.Empty;
}
