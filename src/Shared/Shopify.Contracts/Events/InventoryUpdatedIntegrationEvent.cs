using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.Contracts.Events;
public record InventoryUpdatedIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }

    public Guid ProductId { get; init; }

    public int QuantityReserved { get; init; }

    public int RemainingStock { get; init; }

    public InventoryUpdatedIntegrationEvent() { }

    public InventoryUpdatedIntegrationEvent(
        Guid orderId,
        Guid productId,
        int quantityReserved,
        int remainingStock)
    {
        OrderId = orderId;
        ProductId = productId;
        QuantityReserved = quantityReserved;
        RemainingStock = remainingStock;
    }
}