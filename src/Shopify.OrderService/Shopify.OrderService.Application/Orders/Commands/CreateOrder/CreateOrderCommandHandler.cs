using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Domain.Common.Idempotency;
using Shopify.OrderService.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Orders.Commands.CreateOrder;
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository orderRepository;
    private readonly IIdempotencyRepository idempotencyRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<CreateOrderCommandHandler> logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IIdempotencyRepository idempotencyRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderCommandHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.idempotencyRepository = idempotencyRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // idempotency check
        var existingKey = await idempotencyRepository.GetAsync(
            request.IdempotencyKey, cancellationToken);

        if (existingKey is not null)
        {
            logger.LogWarning(
                "Duplicate request detected for idempotency key {Key}. " +
                "Returning original response for order {OrderId}",
                request.IdempotencyKey, existingKey.OrderId);

            var cachedResult = JsonSerializer.Deserialize<CreateOrderResult>(existingKey.ResponseBody)!;

            return cachedResult with { WasDuplicate = true };
        }

        // create the order
        var order = Order.Create(request.ProductId, request.Quantity);
        await orderRepository.AddAsync(order, cancellationToken);
        
        var result = new CreateOrderResult(
            order.Id,
            order.ProductId,
            order.Quantity,
            order.Status.ToString(),
            order.CreatedAt,
            WasDuplicate: false);

        // store idempotency key with the response 
        var idempotencyKey = IdempotencyKey.Create(
            request.IdempotencyKey,
            order.Id,
            JsonSerializer.Serialize(result));

        await idempotencyRepository.AddAsync(idempotencyKey, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
