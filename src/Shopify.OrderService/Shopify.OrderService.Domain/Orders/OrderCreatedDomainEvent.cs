using Shopify.OrderService.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Domain.Orders;
public record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}