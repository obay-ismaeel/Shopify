using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopify.InventoryService.Application.Common.Interfaces;
using Shopify.InventoryService.Infrastructure.Data;
using Shopify.InventoryService.Infrastructure.Repositories;
using MassTransit;
using Shopify.InventoryService.Infrastructure.Messaging;
using Shopify.InventoryService.Infrastructure.Outbox;
using Shopify.InventoryService.Domain.Products;


namespace Shopify.InventoryService.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedOrderRepository, ProcessedOrderRepository>();

        services.AddHostedService<OutboxPublisherService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"],
                    ushort.Parse(configuration["RabbitMq:Port"] ?? "5672"),
                    "/", 
                    h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r =>
                    r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15)));

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static async Task ApplyMigrationsAndSeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
        await SeedProductsAsync(db);
    }

    private static async Task SeedProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var products = new[]
        {
            (Id: Guid.Parse("00000000-0000-0000-0000-000000000001"), Name: "Widget A", Stock: 100),
            (Id: Guid.Parse("00000000-0000-0000-0000-000000000002"), Name: "Widget B", Stock: 50),
            (Id: Guid.Parse("00000000-0000-0000-0000-000000000003"), Name: "Widget C (low stock)", Stock: 3),
            (Id: Guid.Parse("00000000-0000-0000-0000-000000000004"), Name: "Widget D (no stock)", Stock: 0),
        };

        foreach (var p in products)
        {
            var product = Product.Create(p.Name, p.Stock);

            typeof(Product)
                .GetProperty(nameof(Product.Id))!
                .SetValue(product, p.Id);

            await db.Products.AddAsync(product);
        }

        await db.SaveChangesAsync();
    }
}
