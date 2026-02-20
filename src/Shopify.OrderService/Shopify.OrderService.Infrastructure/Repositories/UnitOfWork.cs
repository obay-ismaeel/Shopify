using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Infrastructure.Data;

namespace Shopify.OrderService.Infrastructure.Repositories;
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