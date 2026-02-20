using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shopify.Contracts.Events;
using Shopify.OrderService.Domain.Orders;
using Shopify.OrderService.Infrastructure.Data;
using System.Text.Json;

namespace Shopify.OrderService.Infrastructure.Outbox;
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

        if (messages.Count == 0) 
            return;

        logger.LogInformation("Processing {Count} outbox message(s)", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await PublishIntegrationEventAsync(message, publishEndpoint, cancellationToken);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;

                logger.LogInformation(
                    "Published outbox message {MessageId} of type {Type}",
                    message.Id,
                    message.Type);
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;

                logger.LogWarning(ex,
                    "Failed to publish outbox message {MessageId}. Retry {RetryCount}/{MaxRetries}",
                    message.Id,
                    message.RetryCount,
                    MaxRetries);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
    private async Task PublishIntegrationEventAsync(
        OutboxMessage message,
        IPublishEndpoint publishEndpoint,
        CancellationToken cancellationToken)
    {
        var typeName = message.Type;

        if (typeName == typeof(OrderCreatedDomainEvent).FullName)
        {
            var domainEvent = JsonSerializer.Deserialize<OrderCreatedDomainEvent>(message.Content)
                ?? throw new InvalidOperationException($"Failed to deserialize {typeName}");

            var integrationEvent = new OrderCreatedIntegrationEvent(
                domainEvent.OrderId,
                domainEvent.ProductId,
                domainEvent.Quantity);

            await publishEndpoint.Publish(integrationEvent, cancellationToken);
            return;
        }

        throw new InvalidOperationException(
            $"No integration event mapping found for domain event type: {typeName}");
    }
}