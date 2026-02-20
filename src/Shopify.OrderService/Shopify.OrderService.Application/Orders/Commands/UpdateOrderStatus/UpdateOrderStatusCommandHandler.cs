using MediatR;
using Microsoft.Extensions.Logging;
using Shopify.OrderService.Application.Common.Exceptions;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Domain.Orders;

namespace Shopify.OrderService.Application.Orders.Commands.UpdateOrderStatus;
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IOrderRepository orderRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<UpdateOrderStatusCommandHandler> logger;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        this.orderRepository = orderRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<UpdateOrderStatusResult> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var previousStatus = order.Status.ToString();

        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.MarkAsConfirmed();
                logger.LogInformation(
                    "✅ Order {OrderId} confirmed — inventory reserved successfully",
                    request.OrderId);
                break;

            case OrderStatus.Cancelled:
                order.MarkAsCancelled();
                logger.LogWarning(
                    "❌ Order {OrderId} cancelled — insufficient stock",
                    request.OrderId);
                break;

            default:
                throw new InvalidOperationException(
                    $"Status transition to '{request.NewStatus}' is not supported via this command.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusResult(
            OrderId: order.Id,
            PreviousStatus: previousStatus,
            NewStatus: order.Status.ToString());
    }
}
