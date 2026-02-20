using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopify.NotificationService.Application.Common.Interfaces;
using Shopify.NotificationService.Domain.Notifications;
using Shopify.NotificationService.Infrastructure.Data;

namespace Shopify.NotificationService.Infrastructure.Messaging;
public class RetryFailedNotificationsService : BackgroundService
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<RetryFailedNotificationsService> logger;
    private const int MaxRetries = 3;

    public RetryFailedNotificationsService(
        IServiceScopeFactory scopeFactory,
        ILogger<RetryFailedNotificationsService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Retry service started. Checking for failed notifications every {Interval}min",
            RetryInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RetryFailedNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in retry service loop");
            }

            await Task.Delay(RetryInterval, stoppingToken);
        }
    }

    private async Task RetryFailedNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        var failed = await db.Notifications
            .Where(n => n.Status == NotificationStatus.Failed
                     && n.RetryCount < MaxRetries)
            .ToListAsync(cancellationToken);

        if (failed.Count == 0) return;

        logger.LogInformation("Retrying {Count} failed notification(s)", failed.Count);

        foreach (var notification in failed)
        {
            try
            {
                notification.ResetForRetry();
                await sender.SendAsync(notification.Message, cancellationToken);
                notification.MarkAsSent();

                logger.LogInformation(
                    "✅ Retry succeeded for notification {NotificationId} (order {OrderId})",
                    notification.Id, notification.OrderId);
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed(ex.Message);

                logger.LogWarning(
                    "❌ Retry {RetryCount}/{MaxRetries} failed for notification {NotificationId} (order {OrderId}): {Reason}",
                    notification.RetryCount, MaxRetries, notification.Id, notification.OrderId, ex.Message);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}