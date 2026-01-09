using BuildingBlocks.Integration.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Order.IntegrationEvents;
using Payment.Application.Commands;
using Payment.Application.DTOs;

namespace Payment.Application.IntegrationEventHandlers;

/// <summary>
/// Receives events from the Order Bounded Context and creates a payment request in the Payment domain.
/// This is part of the Anti-Corruption Layer pattern.
/// </summary>
public class OrderSubmittedIntegrationEventHandler(
    IMediator mediator,
    ILogger<OrderSubmittedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderSubmittedIntegrationEvent>
{
    public async Task HandleAsync(OrderSubmittedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling OrderSubmitted event for Order {OrderId}, Amount: {Amount} {Currency}",
            @event.OrderId,
            @event.TotalAmount,
            @event.Currency);

        var command = new CreatePaymentCommand
        {
            OrderId = @event.OrderId,
            CustomerId = @event.CustomerId,
            Amount = @event.TotalAmount,
            Currency = @event.Currency,
            Method = "CreditCard",
            CardDetails = new CardDetailsDto
            {
                Last4Digits = "4242",
                CardType = "Visa",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.UtcNow.Year + 2,
                CardHolderName = "Demo Customer"
            }
        };

        var paymentId = await mediator.Send(command, cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} created for Order {OrderId}",
            paymentId,
            @event.OrderId);

        var processCommand = new ProcessPaymentCommand { PaymentId = paymentId };
        await mediator.Send(processCommand, cancellationToken);

        var completeCommand = new CompletePaymentCommand
        {
            PaymentId = paymentId,
            TransactionId = $"TXN-{Guid.NewGuid():N}"
        };
        await mediator.Send(completeCommand, cancellationToken);

        logger.LogInformation("Payment {PaymentId} processed and completed", paymentId);
    }
}
