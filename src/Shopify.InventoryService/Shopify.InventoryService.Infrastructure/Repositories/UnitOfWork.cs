using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.InventoryService.Infrastructure.Repositories;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}