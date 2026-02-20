using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.Contracts.Events;
using Shopify.OrderService.Application.Orders.Commands.UpdateOrderStatus;
using Shopify.OrderService.Domain.Orders;

namespace Shopify.OrderService.Infrastructure.Messaging;
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
            "Received InventoryUpdated for order {OrderId} — marking as Confirmed",
            msg.OrderId);

        var result = await sender.Send(
            new UpdateOrderStatusCommand(msg.OrderId, OrderStatus.Confirmed),
            context.CancellationToken);

        logger.LogInformation(
            "Order {OrderId} status updated: {Previous} -> {New}",
            msg.OrderId, result.PreviousStatus, result.NewStatus);
    }
}