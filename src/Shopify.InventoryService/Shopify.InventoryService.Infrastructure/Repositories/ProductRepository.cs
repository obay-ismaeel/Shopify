using Microsoft.EntityFrameworkCore;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Domain.Products;
using Shopify.InventoryService.Infrastructure.Data;

namespace Shopify.InventoryService.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await dbContext.Products.AddAsync(product, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Products.ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
