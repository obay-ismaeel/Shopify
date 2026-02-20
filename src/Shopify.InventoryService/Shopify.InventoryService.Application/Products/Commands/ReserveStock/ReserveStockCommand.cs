using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Products.Commands.ReserveStock;
public record ReserveStockCommand(
    Guid OrderId,
    Guid ProductId,
    int Quantity) : IRequest<ReserveStockResult>;

public record ReserveStockResult(
    bool Success,
    Guid OrderId,
    Guid ProductId,
    int RequestedQuantity,
    int RemainingStock,
    string Message);