using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Shopify.OrderService.Application.Orders.Commands.CreateOrder;
using Shopify.OrderService.Application.Orders.Queries.GetOrder;
using Shopify.OrderService.Domain.Common.Idempotency;

namespace Shopify.OrderService.API.Controllers.Orders;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ISender sender;

    public OrdersController(ISender sender)
    {
        this.sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        // generate key if no key is provided 
        var key = idempotencyKey ?? Guid.NewGuid();
        
        var command = new CreateOrderCommand(key, request.ProductId, request.Quantity);
        var result = await sender.Send(command, cancellationToken);

        // duplicate response returns 200 not 201
        if (result.WasDuplicate)
            return Ok(result);

        return CreatedAtAction(
            nameof(GetOrder),
            new { orderId = result.OrderId },
            result);
    }


    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(orderId);
        var result = await sender.Send(query, cancellationToken);
        return Ok(result);
    }
}

public record CreateOrderRequest(Guid ProductId, int Quantity);
