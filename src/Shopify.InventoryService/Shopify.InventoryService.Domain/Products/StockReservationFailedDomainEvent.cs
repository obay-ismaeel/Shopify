using Shopify.InventoryService.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Domain.Products;
public record StockReservationFailedDomainEvent(
    Guid ProductId,
    Guid OrderId,
    int RequestedQuantity,
    int AvailableStock) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}