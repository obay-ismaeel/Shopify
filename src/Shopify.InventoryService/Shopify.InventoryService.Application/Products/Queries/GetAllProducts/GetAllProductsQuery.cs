using MediatR;
using Shopify.InventoryService.Application.Products.Queries.GetProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Products.Queries.GetAllProducts;
public record GetAllProductsQuery(int Page, int PageSize) : IRequest<PagedResult<ProductDto>>;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
