using Microsoft.EntityFrameworkCore;
using Shopify.OrderService.Domain.Common.Idempotency;
using Shopify.OrderService.Domain.Orders;
using Shopify.OrderService.Infrastructure.Outbox;
using System.Text.Json;

namespace Shopify.OrderService.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        var aggregatesWithEvents = ChangeTracker
            .Entries<Order>()
            .Select(e => e.Entity)
            .Where(e => e.GetDomainEvents().Any())
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var domainEvent in aggregate.GetDomainEvents())
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = null
                };

                OutboxMessages.Add(outboxMessage);
            }

            aggregate.ClearDomainEvents();
        }
    }
}