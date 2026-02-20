using MediatR;
using Shopify.InventoryService.Application.Common.Exceptions;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Domain.Products;

namespace Shopify.InventoryService.Application.Products.Queries.GetProduct;
public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IProductRepository productRepository;

    public GetProductQueryHandler(IProductRepository productRepository)
    {
        this.productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if(product is null)
            throw new NotFoundException(nameof(Product), request.ProductId);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt);
    }
}