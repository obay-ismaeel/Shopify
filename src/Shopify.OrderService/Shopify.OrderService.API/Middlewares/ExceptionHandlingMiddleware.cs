using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shopify.OrderService.Application.Common.Exceptions;
using Shopify.OrderService.Domain.Common.Exceptions;
using System.Text.Json;

namespace Shopify.OrderService.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    // postgreSql unique constraint violation error code
    private const string PostgresUniqueViolationCode = "23505";
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
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, extensions) = exception switch
        {
            Application.Common.Exceptions.ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                "One or more validation errors occurred.",
                (object?)new { errors = ve.Errors }),

            OrderDomainException de => (
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                de.Message,
                (object?)null),

            NotFoundException nfe => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                nfe.Message,
                (object?)null),

            DbUpdateException dbe when IsUniqueConstraintViolation(dbe) => (
            StatusCodes.Status409Conflict,
            "Conflict",
            "A request with this idempotency key is already being processed. " +
            "Please retry in a moment to retrieve the result.",
            (object?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                (object?)null)
        };

        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier,
            extensions
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    => ex.InnerException is PostgresException pgEx
       && pgEx.SqlState == PostgresUniqueViolationCode;
}

