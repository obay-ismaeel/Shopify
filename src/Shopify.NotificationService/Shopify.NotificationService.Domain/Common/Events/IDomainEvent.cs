namespace Shopify.NotificationService.Domain.Common.Events;
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
