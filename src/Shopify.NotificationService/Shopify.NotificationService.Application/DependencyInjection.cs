using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shopify.NotificationService.Application.Common.Behaviors;
using System;
using System.Collections.Generic;


namespace Shopify.NotificationService.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
