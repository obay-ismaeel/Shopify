using Shopify.OrderService.Domain.Common.Events;
using Shopify.OrderService.Domain.Common.Exceptions;

namespace Shopify.OrderService.Domain.Orders;
public class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Order() { }

    private Order(Guid id, Guid productId, int quantity)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Order Create(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new OrderDomainException("Product ID cannot be empty.");

        if (quantity <= 0)
            throw new OrderDomainException("Quantity must be greater than zero.");

        var order = new Order(Guid.NewGuid(), productId, quantity);
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, order.ProductId, order.Quantity));

        return order;
    }

    public void MarkAsConfirmed()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException($"Cannot confirm order in status '{Status}'.");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCancelled()
    {
        if (Status == OrderStatus.Confirmed)
            throw new OrderDomainException("Cannot cancel an already confirmed order.");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}