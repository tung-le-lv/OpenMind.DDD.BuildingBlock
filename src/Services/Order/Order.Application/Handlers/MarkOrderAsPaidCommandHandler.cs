using MediatR;
using Order.Application.Commands;
using Order.Domain.Repositories;
using Order.Domain.ValueObjects;

namespace Order.Application.Handlers;

public class MarkOrderAsPaidCommandHandler(IOrderRepository orderRepository) : IRequestHandler<MarkOrderAsPaidCommand, bool>
{
    public async Task<bool> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(OrderId.From(request.OrderId), cancellationToken);
        if (order == null)
            return false;

        order.MarkAsPaid(request.PaidAt);

        orderRepository.Update(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}
