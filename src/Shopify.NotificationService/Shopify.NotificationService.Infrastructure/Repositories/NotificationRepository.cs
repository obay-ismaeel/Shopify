using Microsoft.EntityFrameworkCore;
using Shopify.NotificationService.Application.Common.Interfaces;
using Shopify.NotificationService.Domain.Notifications;
using Shopify.NotificationService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Infrastructure.Repositories;
public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext dbContext;

    public NotificationRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<bool> ExistsForOrderAsync(
        Guid orderId,
        NotificationType type,
        CancellationToken cancellationToken = default)
        => await dbContext.Notifications
            .AnyAsync(n => n.OrderId == orderId && n.Type == type, cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => await dbContext.Notifications.AddAsync(notification, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetFailedAsync(
        int maxRetries,
        CancellationToken cancellationToken = default)
        => await dbContext.Notifications
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < maxRetries)
            .OrderBy(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
}