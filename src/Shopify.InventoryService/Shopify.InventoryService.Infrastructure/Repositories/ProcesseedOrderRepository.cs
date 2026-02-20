using Microsoft.EntityFrameworkCore;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Domain.Products;
using Shopify.InventoryService.Infrastructure.Data;

namespace Shopify.InventoryService.Infrastructure.Repositories;
public class ProcessedOrderRepository : IProcessedOrderRepository
{
    private readonly AppDbContext dbContext;

    public ProcessedOrderRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(Guid orderId, CancellationToken cancellationToken = default)
        => await dbContext.ProcessedOrders
            .AnyAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task AddAsync(ProcessedOrder processedOrder, CancellationToken cancellationToken = default)
        => await dbContext.ProcessedOrders.AddAsync(processedOrder, cancellationToken);
}
