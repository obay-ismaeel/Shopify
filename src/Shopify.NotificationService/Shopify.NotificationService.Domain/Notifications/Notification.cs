using Shopify.NotificationService.Domain.Common.Events;
using Shopify.NotificationService.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Domain.Notifications;
public class Notification
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string Message { get; private set; } = default!;
    public int RetryCount { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Notification() { }

    public static Notification Create(Guid orderId, NotificationType type, string message)
    {
        if (orderId == Guid.Empty)
            throw new NotificationDomainException("Order ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(message))
            throw new NotificationDomainException("Notification message cannot be empty.");

        return new Notification
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Type = type,
            Status = NotificationStatus.Pending,
            Message = message,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the notification as successfully sent and raises a domain event.
    /// </summary>
    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            throw new NotificationDomainException("Notification has already been sent.");

        Status = NotificationStatus.Sent;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new NotificationSentDomainEvent(
            Id, OrderId, Type.ToString(), Message));
    }

    /// <summary>
    /// Records a send failure and increments the retry counter.
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
        RetryCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resets a failed notification back to Pending so the retry
    /// mechanism can attempt delivery again.
    /// </summary>
    public void ResetForRetry()
    {
        if (Status != NotificationStatus.Failed)
            throw new NotificationDomainException("Only failed notifications can be retried.");

        Status = NotificationStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
