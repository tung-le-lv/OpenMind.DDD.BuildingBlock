using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Queries;

namespace Order.API.Controllers;

/// <summary>
/// API Controller for Order operations.
/// Follows CQRS pattern - Commands modify state, Queries read state.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery { OrderId = orderId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets all orders for a customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByCustomerQuery { CustomerId = customerId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets orders by status.
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByStatusQuery { Status = status };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets pending orders (awaiting payment).
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var query = new GetPendingOrdersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand
        {
            CustomerId = request.CustomerId,
            ShippingAddress = request.ShippingAddress,
            Currency = request.Currency,
            Notes = request.Notes
        };

        var orderId = await _mediator.Send(command, cancellationToken);

        _logger.LogInformation("Order {OrderId} created", orderId);

        return CreatedAtAction(nameof(GetById), new { orderId }, orderId);
    }

    /// <summary>
    /// Adds an item to an order.
    /// </summary>
    [HttpPost("{orderId:guid}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem(
        Guid orderId,
        [FromBody] AddOrderItemDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddOrderItemCommand
        {
            OrderId = orderId,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            UnitPrice = request.UnitPrice,
            Quantity = request.Quantity
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Removes an item from an order.
    /// </summary>
    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new RemoveOrderItemCommand
        {
            OrderId = orderId,
            ItemId = itemId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Updates shipping address.
    /// </summary>
    [HttpPut("{orderId:guid}/address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(
        Guid orderId,
        [FromBody] AddressDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateShippingAddressCommand
        {
            OrderId = orderId,
            NewAddress = request
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Submits an order for processing.
    /// </summary>
    [HttpPost("{orderId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(Guid orderId, CancellationToken cancellationToken)
    {
        var command = new SubmitOrderCommand { OrderId = orderId };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogInformation("Order {OrderId} submitted", orderId);

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        Guid orderId,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand
        {
            OrderId = orderId,
            Reason = request.Reason
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
                return NotFound();

            _logger.LogInformation("Order {OrderId} cancelled. Reason: {Reason}", orderId, request.Reason);

            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record CancelOrderRequest(string Reason);
