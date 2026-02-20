using Microsoft.EntityFrameworkCore;
using Shopify.InventoryService.Domain.Products;
using Shopify.InventoryService.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedOrder> ProcessedOrders => Set<ProcessedOrder>();

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
        var aggregates = ChangeTracker
            .Entries<Product>()
            .Select(e => e.Entity)
            .Where(e => e.GetDomainEvents().Any())
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.GetDomainEvents())
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    CreatedAt = DateTime.UtcNow
                });
            }

            aggregate.ClearDomainEvents();
        }
    }
}