using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Products.Queries.GetProduct;
public record GetProductQuery(Guid ProductId) : IRequest<ProductDto>;

public record ProductDto(
    Guid Id,
    string Name,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt);
