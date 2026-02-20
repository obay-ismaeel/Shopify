using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.Contracts.Events;
public record OutOfStockIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }

    public Guid ProductId { get; init; }

    public int RequestedQuantity { get; init; }

    public int AvailableStock { get; init; }

    public OutOfStockIntegrationEvent() { }

    public OutOfStockIntegrationEvent(
        Guid orderId,
        Guid productId,
        int requestedQuantity,
        int availableStock)
    {
        OrderId = orderId;
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
    }
}