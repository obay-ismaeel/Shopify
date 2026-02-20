using Shopify.InventoryService.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Common.Interfaces;
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);
}