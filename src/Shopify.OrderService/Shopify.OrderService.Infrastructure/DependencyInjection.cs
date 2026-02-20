using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopify.OrderService.Application.Common.Interfaces;
using Shopify.OrderService.Infrastructure.Data;
using Shopify.OrderService.Infrastructure.Outbox;
using Shopify.OrderService.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Shopify.OrderService.Infrastructure.Messaging;

namespace Shopify.OrderService.Infrastructure;
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

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<OutboxPublisherService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryUpdatedConsumer>();
            x.AddConsumer<OutOfStockConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                // Retry policy for transient broker failures
                cfg.UseMessageRetry(r =>
                    r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15)));

                cfg.ReceiveEndpoint("OutOfStock-OrderService", e => {
                    e.ConfigureConsumer<OutOfStockConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("InventoryUpdated-OrderService", e => {
                    e.ConfigureConsumer<InventoryUpdatedConsumer>(ctx);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}