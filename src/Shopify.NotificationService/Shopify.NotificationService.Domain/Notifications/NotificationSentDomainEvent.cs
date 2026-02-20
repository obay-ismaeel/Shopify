using Shopify.NotificationService.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Domain.Notifications;
public record NotificationSentDomainEvent(
    Guid NotificationId,
    Guid OrderId,
    string Type,
    string Message) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
