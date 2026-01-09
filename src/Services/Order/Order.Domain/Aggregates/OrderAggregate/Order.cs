using BuildingBlocks.Domain;
using Order.Domain.Events;
using Order.Domain.Specifications;
using Order.Domain.ValueObjects;

namespace Order.Domain.Aggregates.OrderAggregate;

/// <summary>
/// Order Aggregate Root.
/// 
/// DDD Principles applied:
/// 1. Aggregate Root: Order is the single entry point for the Order aggregate
/// 2. Consistency Boundary: All invariants within the aggregate are enforced here
/// 3. Encapsulation: Internal entities (OrderItems) cannot be modified directly
/// 4. Domain Events: Business events are raised for important state changes
/// 5. Rich Domain Model: Business logic lives in the domain, not in services
/// </summary>
public class Order : AggregateRoot<OrderId>
{
    private List<OrderItem> _orderItems = null!;

    public CustomerId CustomerId { get; private set; }
    public Address ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Read-only access to order items - external code cannot modify the collection directly.
    /// </summary>
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Money TotalAmount => CalculateTotalAmount();
    public string Currency { get; private set; }

    private Order() 
    { 
        _orderItems = new List<OrderItem>();
    }

    #region Factory Methods

    /// <summary>
    /// Factory method ensures all invariants are met at creation time.
    /// </summary>
    public static Order Create(CustomerId customerId, Address shippingAddress, string currency = "USD")
    {
        var order = new Order
        {
            Id = OrderId.New(),
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId)),
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress)),
            Status = OrderStatus.Draft,
            Currency = currency,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, order.CustomerId));

        return order;
    }

    #endregion

    #region Behavior Methods - Rich Domain Model

    /// <summary>
    /// Adds an item to the order.
    /// Enforces the business rule that items can only be added to draft orders.
    /// </summary>
    public void AddItem(ProductId productId, string productName, Money unitPrice, int quantity)
    {
        // Enforce business rule
        if (!Status.CanAddItems())
            throw new InvalidOperationException($"Cannot add items to an order in {Status.Name} status");

        // Check if product already exists in order
        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            var newItem = OrderItem.Create(productId, productName, unitPrice, quantity);
            _orderItems.Add(newItem);
        }

        SetModified();

        // Raise domain event
        RaiseDomainEvent(new OrderItemAddedDomainEvent(Id, productId, productName, quantity));
    }

    public void RemoveItem(OrderItemId itemId)
    {
        if (!Status.CanAddItems())
            throw new InvalidOperationException($"Cannot remove items from an order in {Status.Name} status");

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Order item {itemId} not found");

        _orderItems.Remove(item);
        SetModified();
    }

    public void UpdateItemQuantity(OrderItemId itemId, int newQuantity)
    {
        if (!Status.CanAddItems())
            throw new InvalidOperationException($"Cannot update items in an order in {Status.Name} status");

        var item = _orderItems.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Order item {itemId} not found");

        if (newQuantity <= 0)
        {
            _orderItems.Remove(item);
        }
        else
        {
            item.SetQuantity(newQuantity);
        }

        SetModified();
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        if (!Status.CanAddItems())
            throw new InvalidOperationException($"Cannot update address for an order in {Status.Name} status");

        ShippingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        SetModified();
    }

    public void SetNotes(string notes)
    {
        Notes = notes;
        SetModified();
    }

    /// <summary>
    /// State transition that raises domain event - triggers integration event for Payment Bounded Context.
    /// </summary>
    public void Submit(decimal minimumOrderValue = 10.00m)
    {
        if (!Status.CanBeSubmitted())
            throw new InvalidOperationException($"Cannot submit an order in {Status.Name} status");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Cannot submit an order without items");

        var minimumValueSpec = new MinimumOrderValueSpecification(minimumOrderValue);
        if (!minimumValueSpec.IsSatisfiedBy(this))
            throw new InvalidOperationException($"Order total must be at least {minimumOrderValue:C}");

        Status = OrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        SetModified();

        // Raise domain event - this will trigger integration event for Payment service
        RaiseDomainEvent(new OrderSubmittedDomainEvent(
            Id,
            CustomerId,
            TotalAmount.Amount,
            Currency));
    }

    /// <summary>
    /// Called when payment is confirmed from the Payment Bounded Context.
    /// </summary>
    public void MarkAsPaid(DateTime paidAt)
    {
        if (!Status.CanBePaid())
            throw new InvalidOperationException($"Cannot mark order as paid in {Status.Name} status");

        Status = OrderStatus.Paid;
        PaidAt = paidAt;
        SetModified();

        RaiseDomainEvent(new OrderPaidDomainEvent(Id, paidAt));
    }

    public void MarkPaymentFailed(string reason)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot mark payment as failed for order in {Status.Name} status");

        Status = OrderStatus.PaymentFailed;
        SetModified();

        RaiseDomainEvent(new OrderPaymentFailedDomainEvent(Id, reason));
    }

    public void StartProcessing()
    {
        var readySpec = new OrderReadyForProcessingSpecification();
        if (!readySpec.IsSatisfiedBy(this))
            throw new InvalidOperationException("Only paid orders can be processed");

        Status = OrderStatus.Processing;
        SetModified();
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        SetModified();

        RaiseDomainEvent(new OrderShippedDomainEvent(Id));
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be marked as delivered");

        Status = OrderStatus.Delivered;
        SetModified();

        RaiseDomainEvent(new OrderDeliveredDomainEvent(Id));
    }

    public void Cancel(string reason)
    {
        var cancellableSpec = new CancellableOrderSpecification();
        if (!cancellableSpec.IsSatisfiedBy(this))
            throw new InvalidOperationException($"Cannot cancel an order in {Status.Name} status");

        Status = OrderStatus.Cancelled;
        SetModified();

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    #endregion

    #region Private Methods

    private Money CalculateTotalAmount()
    {
        if (!_orderItems.Any())
            return Money.Zero(Currency);

        return _orderItems
            .Select(x => x.Total)
            .Aggregate((current, next) => current + next);
    }

    private void SetModified()
    {
        ModifiedAt = DateTime.UtcNow;
        IncrementVersion();
    }

    #endregion
}
