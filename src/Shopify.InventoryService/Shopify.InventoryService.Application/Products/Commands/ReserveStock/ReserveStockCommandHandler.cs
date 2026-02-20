using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.InventoryService.Application.Common.Exceptions;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Domain.Products;

namespace Shopify.InventoryService.Application.Products.Commands.ReserveStock;
public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, ReserveStockResult>
{
    private readonly IProductRepository productRepository;
    private readonly IProcessedOrderRepository processedOrderRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<ReserveStockCommandHandler> logger;

    public ReserveStockCommandHandler(
        IProductRepository productRepository,
        IProcessedOrderRepository processedOrderRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReserveStockCommandHandler> logger)
    {
        this.productRepository = productRepository;
        this.processedOrderRepository = processedOrderRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<ReserveStockResult> Handle(
        ReserveStockCommand request,
        CancellationToken cancellationToken)
    {
        // idempotency check
        var alreadyProcessed = await processedOrderRepository.ExistsAsync(
            request.OrderId, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogWarning(
                "duplicate OrderCreated event detected for order {OrderId}. (skipping reservation)", request.OrderId);

            return new ReserveStockResult(
                Success: false,
                OrderId: request.OrderId,
                ProductId: request.ProductId,
                RequestedQuantity: request.Quantity,
                RemainingStock: 0,
                Message: "duplicate event — order already processed.");
        }

        // fetch product and reserve stock
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var reserved = product.Reserve(request.Quantity, request.OrderId);

        // even if reservation failed we mark the order as processed so a redelivered event dosen't try it again
        var processedOrder = ProcessedOrder.Create(request.OrderId, request.ProductId);
        await processedOrderRepository.AddAsync(processedOrder, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (reserved)
        {
            return new ReserveStockResult(
                Success: true,
                OrderId: request.OrderId,
                ProductId: request.ProductId,
                RequestedQuantity: request.Quantity,
                RemainingStock: product.Stock,
                Message: $"Stock reserved successfully for product '{product.Name}'.");
        }

        logger.LogWarning(
            "insufficient stock for order {OrderId}: requested {Requested}, available {Available} of '{ProductName}' (ID: {ProductId})",
            request.OrderId, request.Quantity, product.Stock, product.Name, request.ProductId);

        return new ReserveStockResult(
            Success: false,
            OrderId: request.OrderId,
            ProductId: request.ProductId,
            RequestedQuantity: request.Quantity,
            RemainingStock: product.Stock,
            Message: $"insufficient stock for '{product.Name}'. Requested: {request.Quantity}, Available: {product.Stock}.");
    }
}
