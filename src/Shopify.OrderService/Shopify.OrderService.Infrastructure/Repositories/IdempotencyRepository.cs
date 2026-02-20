using Microsoft.EntityFrameworkCore;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Domain.Common.Idempotency;
using Shopify.OrderService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Infrastructure.Repositories;
public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly AppDbContext dbContext;

    public IdempotencyRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IdempotencyKey?> GetAsync(Guid key, CancellationToken cancellationToken = default)
        => await dbContext.IdempotencyKeys
            .FirstOrDefaultAsync(i => i.Key == key, cancellationToken);

    public async Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
        => await dbContext.IdempotencyKeys.AddAsync(idempotencyKey, cancellationToken);
}

