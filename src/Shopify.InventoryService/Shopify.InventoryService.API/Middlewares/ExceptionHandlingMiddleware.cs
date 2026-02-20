using Shopify.InventoryService.Application.Common.Exceptions;
using Shopify.InventoryService.Domain.Common.Exceptions;
using System.Text.Json;

namespace Shopify.InventoryService.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail) = exception switch
        {
            NotFoundException nfe => (404, "Resource Not Found", nfe.Message),
            InventoryDomainException de => (400, "Business Rule Violation", de.Message),
            _ => (500, "Internal Server Error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        },
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}