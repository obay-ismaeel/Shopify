using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Orders.Commands.CreateOrder;
public record CreateOrderCommand(Guid IdempotencyKey, Guid ProductId, int Quantity) : IRequest<CreateOrderResult>;

public record CreateOrderResult(Guid OrderId, Guid ProductId, int Quantity, string Status, DateTime CreatedAt, bool WasDuplicate = false);
