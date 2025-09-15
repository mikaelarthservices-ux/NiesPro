using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Order.API.Middleware;

/// <summary>
/// Middleware pour la gestion centralisée des exceptions
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest, 
                JsonSerializer.Serialize(new
                {
                    Error = "Validation failed",
                    Details = validationEx.Errors.Select(e => new
                    {
                        Property = e.PropertyName,
                        Message = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    })
                })
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest, 
                JsonSerializer.Serialize(new { Error = exception.Message })
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest, 
                JsonSerializer.Serialize(new { Error = exception.Message })
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound, 
                JsonSerializer.Serialize(new { Error = "Resource not found" })
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized, 
                JsonSerializer.Serialize(new { Error = "Unauthorized access" })
            ),
            _ => (
                HttpStatusCode.InternalServerError, 
                JsonSerializer.Serialize(new { Error = "An internal server error occurred" })
            )
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(message);
    }
}

/// <summary>
/// Middleware pour le logging des requêtes
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString();

        // Ajouter l'ID de requête aux headers de réponse
        context.Response.Headers["X-Request-ID"] = requestId;

        _logger.LogInformation(
            "Request started: {RequestId} {Method} {Path} {QueryString}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Request completed: {RequestId} {Method} {Path} {StatusCode} in {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
    }
}

/// <summary>
/// Extensions pour l'enregistrement des middlewares
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Ajouter le middleware de gestion d'exceptions
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    /// <summary>
    /// Ajouter le middleware de logging des requêtes
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}