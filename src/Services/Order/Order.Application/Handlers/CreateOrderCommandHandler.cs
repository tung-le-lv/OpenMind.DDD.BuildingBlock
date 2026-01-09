using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Handlers;

/// <summary>
/// Application Service responsibilities:
/// 1. Orchestrate domain operations
/// 2. Handle transactions (via Unit of Work)
/// 3. Map between DTOs and domain objects
/// 4. NOT contain business logic (that belongs in domain)
/// </summary>
public class CreateOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var shippingAddress = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.ZipCode);

        var order = Domain.Aggregates.OrderAggregate.Order.Create(
            CustomerId.From(request.CustomerId),
            shippingAddress,
            request.Currency);

        if (!string.IsNullOrEmpty(request.Notes))
        {
            order.SetNotes(request.Notes);
        }

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return order.Id.Value;
    }
}
