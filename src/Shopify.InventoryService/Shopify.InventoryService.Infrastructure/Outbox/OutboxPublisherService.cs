using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Shopify.InventoryService.Infrastructure.Data;
using System.Text.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shopify.Contracts.Events;
using Shopify.InventoryService.Domain.Products;

namespace Shopify.InventoryService.Infrastructure.Outbox;
public class OutboxPublisherService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<OutboxPublisherService> logger;
    private const int BatchSize = 20;
    private const int MaxRetries = 3;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Publisher started. Polling every {Interval}s", PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in outbox publisher loop");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        logger.LogInformation("Processing {Count} outbox message(s)", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await PublishIntegrationEventAsync(message, publishEndpoint, cancellationToken);
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                logger.LogInformation("Published outbox message {MessageId} ({Type})", message.Id, message.Type);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogWarning(ex,
                    "Failed to publish outbox message {MessageId}. Attempt {Retry}/{Max}",
                    message.Id, message.RetryCount, MaxRetries);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task PublishIntegrationEventAsync(
        OutboxMessage message,
        IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken)
    {
        if (message.Type == typeof(StockReservedDomainEvent).FullName)
        {
            var e = JsonSerializer.Deserialize<StockReservedDomainEvent>(message.Content)!;
            await publishEndpoint.Publish(
                new InventoryUpdatedIntegrationEvent(e.OrderId, e.ProductId, e.QuantityReserved, e.RemainingStock),
                cancellationToken);
            return;
        }

        if (message.Type == typeof(StockReservationFailedDomainEvent).FullName)
        {
            var e = JsonSerializer.Deserialize<StockReservationFailedDomainEvent>(message.Content)!;
            await publishEndpoint.Publish(
                new OutOfStockIntegrationEvent(e.OrderId, e.ProductId, e.RequestedQuantity, e.AvailableStock),
                cancellationToken);
            return;
        }

        throw new InvalidOperationException(
            $"No integration event mapping for domain event type: {message.Type}");
    }
}