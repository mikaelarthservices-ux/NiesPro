using System.Net;
using System.Text.Json;
using FluentValidation;
using NiesPro.Contracts.Common;

namespace Catalog.API.Middleware;

/// <summary>
/// Global exception handling middleware for Catalog API
/// Provides centralized error handling following NiesPro standards
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.CreateError(
                    "Validation failed",
                    validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList()
                )
            ),
            ArgumentException argumentEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.CreateError(argumentEx.Message)
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.CreateError("Resource not found")
            ),
            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.CreateError(invalidOpEx.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.CreateError("Unauthorized access")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.CreateError("An internal server error occurred")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension methods for middleware registration
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// Add global exception handling middleware
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}