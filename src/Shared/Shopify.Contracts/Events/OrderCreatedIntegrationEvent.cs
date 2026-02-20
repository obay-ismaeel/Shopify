using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.Contracts.Events;
public record OrderCreatedIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public Guid OrderId { get; init; }

    public Guid ProductId { get; init; }

    public int Quantity { get; init; }

    public OrderCreatedIntegrationEvent() { }

    public OrderCreatedIntegrationEvent(Guid orderId, Guid productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
    }
}