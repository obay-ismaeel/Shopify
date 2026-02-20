using MediatR;
using Shopify.OrderService.Application.Common.Exceptions;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Domain.Orders;

namespace Shopify.OrderService.Application.Orders.Queries.GetOrder;
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderRepository orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            
        if(order is null)
            throw new NotFoundException(nameof(Order), request.OrderId);

        return new OrderDto(
            order.Id,
            order.ProductId,
            order.Quantity,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt);
    }
}