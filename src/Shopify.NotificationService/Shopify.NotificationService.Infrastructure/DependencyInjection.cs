using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopify.NotificationService.Application.Common.Interfaces;
using Shopify.NotificationService.Infrastructure.Data;
using Shopify.NotificationService.Infrastructure.Messaging;
using Shopify.NotificationService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shopify.NotificationService.Infrastructure;
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

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<INotificationSender, LogNotificationSender>();

        services.AddHostedService<RetryFailedNotificationsService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<InventoryUpdatedConsumer>();
            x.AddConsumer<OutOfStockConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], 
                    "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r =>
                    r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15)));

                cfg.ReceiveEndpoint("OutOfStock-NotificationService", e => {
                    e.ConfigureConsumer<OutOfStockConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("InventoryUpdated-NotificationService", e => {
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
