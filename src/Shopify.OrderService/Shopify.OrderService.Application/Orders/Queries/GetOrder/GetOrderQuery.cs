using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Orders.Queries.GetOrder;
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;

public record OrderDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);