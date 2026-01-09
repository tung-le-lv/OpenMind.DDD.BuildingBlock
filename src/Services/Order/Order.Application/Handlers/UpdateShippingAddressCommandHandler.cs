using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Handlers;

public class UpdateShippingAddressCommandHandler(IOrderRepository orderRepository) : IRequestHandler<UpdateShippingAddressCommand, bool>
{
    public async Task<bool> Handle(UpdateShippingAddressCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        var newAddress = new Address(
            request.NewAddress.Street,
            request.NewAddress.City,
            request.NewAddress.State,
            request.NewAddress.Country,
            request.NewAddress.ZipCode);

        order.UpdateShippingAddress(newAddress);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
