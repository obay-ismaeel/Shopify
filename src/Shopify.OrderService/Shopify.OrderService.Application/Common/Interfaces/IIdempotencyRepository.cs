using Shopify.OrderService.Domain.Common.Idempotency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Application.Common.Interfaces;
public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetAsync(Guid key, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);
}
