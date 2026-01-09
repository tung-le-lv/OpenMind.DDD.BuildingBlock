using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Handlers;

public class MarkOrderPaymentFailedCommandHandler(IOrderRepository orderRepository) : IRequestHandler<MarkOrderPaymentFailedCommand, bool>
{
    public async Task<bool> Handle(MarkOrderPaymentFailedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        order.MarkPaymentFailed(request.Reason);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
