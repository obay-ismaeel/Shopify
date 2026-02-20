using Shopify.NotificationService.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Application.Common.Interfaces;
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used for idempotency checks — prevents sending duplicate notifications
    /// if the same event is delivered more than once by the broker.
    /// </summary>
    Task<bool> ExistsForOrderAsync(Guid orderId, NotificationType type, CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetFailedAsync(int maxRetries, CancellationToken cancellationToken = default);
}
