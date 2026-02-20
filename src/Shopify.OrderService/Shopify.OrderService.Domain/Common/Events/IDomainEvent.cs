using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Domain.Common.Events;
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
