using Shopify.InventoryService.Domain.Common.Events;
using Shopify.InventoryService.Domain.Common.Exceptions;

namespace Shopify.InventoryService.Domain.Products;
public class Product
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // for handling concurrency
    public uint RowVersion { get; private set; }

    private Product() { }

    public static Product Create(string name, int initialStock)
    {
        if (initialStock < 0)
            throw new InventoryDomainException("Initial stock cannot be negative.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InventoryDomainException("Product Name can't be empty");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Stock = initialStock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool Reserve(int quantity, Guid orderId)
    {
        if (quantity <= 0)
            throw new InventoryDomainException("Reservation quantity must be greater than zero.");

        if (Stock < quantity)
        {
            RaiseDomainEvent(new StockReservationFailedDomainEvent(
                Id, orderId, quantity, Stock));
            return false;
        }

        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new StockReservedDomainEvent(
            Id, orderId, quantity, Stock));

        return true;
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
