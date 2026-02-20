using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Domain.Products;
public class ProcessedOrder
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private ProcessedOrder() { }

    public static ProcessedOrder Create(Guid orderId, Guid productId)
        => new()
        {
            OrderId = orderId,
            ProductId = productId,
            ProcessedAt = DateTime.UtcNow
        };
}
