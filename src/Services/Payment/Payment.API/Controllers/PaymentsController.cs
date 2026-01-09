using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands;
using Payment.Application.DTOs;
using Payment.Application.Queries;

namespace Payment.API.Controllers;

/// <summary>
/// API Controller for Payment operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid paymentId, CancellationToken cancellationToken)
    {
        var query = new GetPaymentByIdQuery { PaymentId = paymentId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets payment by order ID.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetPaymentByOrderIdQuery { OrderId = orderId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets payments for a customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetPaymentsByCustomerQuery { CustomerId = customerId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets pending payments.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var query = new GetPendingPaymentsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto request, CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            Method = request.Method,
            CardDetails = request.CardDetails
        };

        try
        {
            var paymentId = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Payment {PaymentId} created", paymentId);
            return CreatedAtAction(nameof(GetById), new { paymentId }, paymentId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Processes a pending payment.
    /// </summary>
    [HttpPost("{paymentId:guid}/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process(Guid paymentId, CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand { PaymentId = paymentId };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogInformation("Payment {PaymentId} processing started", paymentId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Completes a payment (simulates gateway callback).
    /// </summary>
    [HttpPost("{paymentId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(
        Guid paymentId,
        [FromBody] CompletePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompletePaymentCommand
        {
            PaymentId = paymentId,
            TransactionId = request.TransactionId
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogInformation("Payment {PaymentId} completed", paymentId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Fails a payment.
    /// </summary>
    [HttpPost("{paymentId:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fail(
        Guid paymentId,
        [FromBody] FailPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FailPaymentCommand
        {
            PaymentId = paymentId,
            Reason = request.Reason
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogWarning("Payment {PaymentId} failed. Reason: {Reason}", paymentId, request.Reason);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Refunds a completed payment.
    /// </summary>
    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund(
        Guid paymentId,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefundPaymentCommand
        {
            PaymentId = paymentId,
            Reason = request.Reason
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogInformation("Payment {PaymentId} refunded. Reason: {Reason}", paymentId, request.Reason);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record CompletePaymentRequest(string TransactionId);
public record FailPaymentRequest(string Reason);
public record RefundPaymentRequest(string Reason);
