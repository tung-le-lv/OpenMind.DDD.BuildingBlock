using BuildingBlocks.Domain.SeedWork;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Services;

/// <summary>
/// Domain Service for Payment Processing.
/// 
/// This domain service handles payment processing logic that doesn't
/// naturally belong to the Payment aggregate itself.
/// </summary>
public interface IPaymentProcessingService : IDomainService
{
    /// <summary>
    /// Validates if a payment can be processed.
    /// </summary>
    PaymentValidationResult ValidatePayment(Aggregates.PaymentAggregate.Payment payment);

    /// <summary>
    /// Calculates any processing fees.
    /// </summary>
    Money CalculateProcessingFee(Money amount, Aggregates.PaymentAggregate.PaymentMethod method);
}

/// <summary>
/// Result of payment validation.
/// </summary>
public record PaymentValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static PaymentValidationResult Success() => new() { IsValid = true };
    public static PaymentValidationResult Failure(string message) => new() { IsValid = false, ErrorMessage = message };
}

/// <summary>
/// Implementation of Payment Processing Domain Service.
/// </summary>
public class PaymentProcessingService : IPaymentProcessingService
{
    public PaymentValidationResult ValidatePayment(Aggregates.PaymentAggregate.Payment payment)
    {
        // Validate amount
        if (payment.Amount.Amount <= 0)
            return PaymentValidationResult.Failure("Payment amount must be positive");

        // Validate card if card payment
        if (payment.CardDetails != null && payment.CardDetails.IsExpired())
            return PaymentValidationResult.Failure("Card has expired");

        // Add more validation rules as needed
        // - Check fraud detection
        // - Check customer credit limit
        // - Validate currency support

        return PaymentValidationResult.Success();
    }

    public Money CalculateProcessingFee(Money amount, Aggregates.PaymentAggregate.PaymentMethod method)
    {
        // Different fees for different payment methods
        decimal feePercentage = method.Name switch
        {
            nameof(Aggregates.PaymentAggregate.PaymentMethod.CreditCard) => 0.029m, // 2.9%
            nameof(Aggregates.PaymentAggregate.PaymentMethod.DebitCard) => 0.015m,  // 1.5%
            nameof(Aggregates.PaymentAggregate.PaymentMethod.PayPal) => 0.034m,     // 3.4%
            nameof(Aggregates.PaymentAggregate.PaymentMethod.BankTransfer) => 0.005m, // 0.5%
            _ => 0.03m // Default 3%
        };

        var fee = amount.Amount * feePercentage;
        return new Money(Math.Round(fee, 2), amount.Currency);
    }
}
