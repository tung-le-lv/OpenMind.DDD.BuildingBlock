using System.Linq.Expressions;
using BuildingBlocks.Domain;

namespace Order.Domain.Specifications;

public class MaxItemsCountSpecification(int maxItems) : Specification<Aggregates.OrderAggregate.Order>
{
    public override Expression<Func<Aggregates.OrderAggregate.Order, bool>> ToExpression()
    {
        return order => order.OrderItems.Count <= maxItems;
    }
}
