using BuildingBlocks.Domain.SeedWork;

namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object representing Money in the Payment domain.
/// Note: This is a separate Money class from Order domain - each Bounded Context
/// has its own models even if they look similar. This maintains context autonomy.
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0, currency);
    public static Money FromDecimal(decimal amount, string currency = "USD") => new(amount, currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}

/// <summary>
/// Value Object representing Card Payment Details.
/// Contains only what's needed for processing - no full card numbers stored.
/// </summary>
public class CardDetails : ValueObject
{
    public string Last4Digits { get; }
    public string CardType { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string CardHolderName { get; }

    private CardDetails() { }

    public CardDetails(string last4Digits, string cardType, int expiryMonth, int expiryYear, string cardHolderName)
    {
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
            throw new ArgumentException("Last 4 digits must be exactly 4 characters", nameof(last4Digits));

        if (string.IsNullOrWhiteSpace(cardType))
            throw new ArgumentException("Card type is required", nameof(cardType));

        if (expiryMonth < 1 || expiryMonth > 12)
            throw new ArgumentException("Expiry month must be between 1 and 12", nameof(expiryMonth));

        if (expiryYear < DateTime.UtcNow.Year)
            throw new ArgumentException("Card has expired", nameof(expiryYear));

        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException("Card holder name is required", nameof(cardHolderName));

        Last4Digits = last4Digits;
        CardType = cardType;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CardHolderName = cardHolderName;
    }

    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        return ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Last4Digits;
        yield return CardType;
        yield return ExpiryMonth;
        yield return ExpiryYear;
        yield return CardHolderName;
    }

    public override string ToString() => $"{CardType} ****{Last4Digits}";
}
