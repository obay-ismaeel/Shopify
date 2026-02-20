using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Application.Common.Interfaces;
public interface IProcessedOrderRepository
{
    Task<bool> ExistsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Products.ProcessedOrder processedOrder, CancellationToken cancellationToken = default);
}