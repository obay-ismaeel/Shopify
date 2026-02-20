using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.NotificationService.Application.Notifications.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Shopify.Contracts.Events;
using Shopify.NotificationService.Domain.Notifications;

namespace Shopify.NotificationService.Infrastructure.Messaging;
public class InventoryUpdatedConsumer : IConsumer<InventoryUpdatedIntegrationEvent>
{
    private readonly ISender sender;
    private readonly ILogger<InventoryUpdatedConsumer> logger;

    public InventoryUpdatedConsumer(
        ISender sender,
        ILogger<InventoryUpdatedConsumer> logger)
    {
        this.sender = sender;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryUpdatedIntegrationEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Received InventoryUpdated — OrderId: {OrderId}, ProductId: {ProductId}, Remaining: {Remaining}",
            msg.OrderId, msg.ProductId, msg.RemainingStock);

        var message = $"✅ Your order {msg.OrderId} has been confirmed! " +
                      $"{msg.QuantityReserved} unit(s) of product {msg.ProductId} reserved. " +
                      $"Remaining stock: {msg.RemainingStock}.";

        var result = await sender.Send(
            new SendNotificationCommand(msg.OrderId, NotificationType.OrderConfirmed, message),
            context.CancellationToken);

        if (result.WasDuplicate)
            logger.LogWarning("Duplicate InventoryUpdated event for order {OrderId} — skipped.", msg.OrderId);
        else
            logger.LogInformation("Notification result for order {OrderId}: {Status}", msg.OrderId, result.Status);
    }
}
