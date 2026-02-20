using Microsoft.EntityFrameworkCore;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Domain.Orders;
using Shopify.OrderService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.OrderService.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AddAsync(order, cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(o => o.Id == id, cancellationToken);
}
