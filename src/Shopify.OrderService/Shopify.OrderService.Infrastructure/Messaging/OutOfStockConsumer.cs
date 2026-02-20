using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.Contracts.Events;
using Shopify.OrderService.Application.Orders.Commands.UpdateOrderStatus;
using Shopify.OrderService.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Infrastructure.Messaging;
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

        logger.LogWarning(
            "Received OutOfStock for order {OrderId} — marking as Cancelled. " +
            "Requested: {Requested}, Available: {Available}",
            msg.OrderId, msg.RequestedQuantity, msg.AvailableStock);

        var result = await sender.Send(
            new UpdateOrderStatusCommand(msg.OrderId, OrderStatus.Cancelled),
            context.CancellationToken);

        logger.LogInformation(
            "Order {OrderId} status updated: {Previous} → {New}",
            msg.OrderId, result.PreviousStatus, result.NewStatus);
    }
}