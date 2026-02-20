using Microsoft.Extensions.Logging;
using Shopify.NotificationService.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Infrastructure.Messaging;
public class LogNotificationSender : INotificationSender
{
    private readonly ILogger<LogNotificationSender> logger;

    public LogNotificationSender(ILogger<LogNotificationSender> logger)
    {
        this.logger = logger;
    }

    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        // simulate latency
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

        logger.LogInformation(
            "📬 [NOTIFICATION SENT] {Message} | Timestamp: {Timestamp}",
            message,
            DateTime.UtcNow);
    }
}