using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.Contracts.Events;
using Shopify.NotificationService.Application.Notifications.Commands;
using Shopify.NotificationService.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Infrastructure.Messaging;
public class OutOfStockConsumer : IConsumer<OutOfStockIntegrationEvent>
{
    private readonly ISender sender;
    private readonly ILogger<OutOfStockConsumer> logger;

    public OutOfStockConsumer(
        ISender sender,
        ILogger<OutOfStockConsumer> logger)
    {
        this.sender = sender;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OutOfStockIntegrationEvent> context)
    {
        var msg = context.Message;

        var message = $"Your order {msg.OrderId} is REJECTED. " +
                      $"Product {msg.ProductId} has insufficient stock. " +
                      $"Requested: {msg.RequestedQuantity}, Available: {msg.AvailableStock}.";

        var result = await sender.Send(
            new SendNotificationCommand(msg.OrderId, NotificationType.OrderRejected, message),
            context.CancellationToken);

        if (result.WasDuplicate)
            logger.LogWarning("Duplicate OutOfStock event for order {OrderId} — skipped.", msg.OrderId);
        else
            logger.LogInformation("Notification result for order {OrderId}: {Status}", msg.OrderId, result.Status);
    }
}
