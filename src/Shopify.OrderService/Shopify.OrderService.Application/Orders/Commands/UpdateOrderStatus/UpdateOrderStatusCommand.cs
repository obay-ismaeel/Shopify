using MediatR;
using Shopify.OrderService.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Orders.Commands.UpdateOrderStatus;
public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus) : IRequest<UpdateOrderStatusResult>;

public record UpdateOrderStatusResult(
    Guid OrderId,
    string PreviousStatus,
    string NewStatus);
