using MediatR;

namespace Order.Application.Commands;

public record MarkOrderAsPaidCommand : IRequest<bool>
{
    public Guid OrderId { get; init; }
    public DateTime PaidAt { get; init; }
}
