using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopify.InventoryService.Application.Products.Queries.GetAllProducts;
using Shopify.InventoryService.Application.Products.Queries.GetProduct;

namespace Shopify.InventoryService.API.Controllers.Products;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than 0.");

        if (pageSize < 1 || pageSize > 50)
            return BadRequest("PageSize must be between 1 and 50.");

        var result = await sender.Send(
            new GetAllProductsQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductQuery(productId), cancellationToken);
        return Ok(result);
    }
}