using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopify.Contracts.Events;
using Shopify.InventoryService.Application.Products.Commands.ReserveStock;

namespace Shopify.InventoryService.Infrastructure.Messaging;
public class OrderCreatedConsumer : IConsumer<OrderCreatedIntegrationEvent>
{
    private const int MaxConcurrencyRetries = 3;
    private readonly ISender sender;
    private readonly ILogger<OrderCreatedConsumer> logger;

    public OrderCreatedConsumer(
        ISender sender,
        ILogger<OrderCreatedConsumer> logger)
    {
        this.sender = sender;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var msg = context.Message;
        var attempt = 0;

        while (true)
        {
            try
            {
                attempt++;

                var result = await sender.Send(new ReserveStockCommand(msg.OrderId, msg.ProductId, msg.Quantity), 
                    context.CancellationToken);

                return;
            }
            catch (DbUpdateConcurrencyException ex) when (attempt < MaxConcurrencyRetries)
            {
                // wait a little because another consumer updated the same product row concurrently
                logger.LogWarning(
                    "concurrency conflict on order {OrderId} attempt {Attempt}/{Max}. retrying...",
                    msg.OrderId, attempt, MaxConcurrencyRetries);

                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), context.CancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError(ex,
                    "concurrency conflict on order {OrderId} exceeded max retries. Abort execution", msg.OrderId);
                throw;
            }
        }
        

        //logger.LogInformation(
        //    "Received OrderCreated — OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
        //    msg.OrderId, msg.ProductId, msg.Quantity);

        //var result = await sender.Send(
        //    new ReserveStockCommand(msg.OrderId, msg.ProductId, msg.Quantity),
        //    context.CancellationToken);

        //logger.LogInformation(
        //    "Reservation result for order {OrderId}: {Status} — {Message}",
        //    msg.OrderId,
        //    result.Success ? "SUCCESS" : "FAILED",
        //    result.Message);
    }
}