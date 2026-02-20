using MediatR;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Application.Products.Queries.GetProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Products.Queries.GetAllProducts;
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        this.productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items
            .Select(p => new ProductDto(p.Id, p.Name, p.Stock, p.CreatedAt, p.UpdatedAt))
            .ToList();

        return new PagedResult<ProductDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}