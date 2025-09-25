using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace NiesPro.Logging.Client.Middleware;

/// <summary>
/// Middleware OBLIGATOIRE de logging automatique pour tous les services NiesPro
/// </summary>
public class NiesProLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NiesProLoggingMiddleware> _logger;
    private readonly ILogsServiceClient _logsClient;
    private readonly IMetricsServiceClient _metricsClient;

    public NiesProLoggingMiddleware(
        RequestDelegate next, 
        ILogger<NiesProLoggingMiddleware> logger,
        ILogsServiceClient logsClient,
        IMetricsServiceClient metricsClient)
    {
        _next = next;
        _logger = logger;
        _logsClient = logsClient;
        _metricsClient = metricsClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Génération du CorrelationId OBLIGATOIRE
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        // Extraction des informations contextuelles
        var requestPath = context.Request.Path;
        var httpMethod = context.Request.Method;
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userId = context.User?.Identity?.Name ?? "Anonymous";

        // Métriques de timing
        var stopwatch = Stopwatch.StartNew();
        
        // Properties communes pour tous les logs
        var baseProperties = new Dictionary<string, object>
        {
            { "CorrelationId", correlationId },
            { "RequestPath", requestPath },
            { "HttpMethod", httpMethod },
            { "UserAgent", userAgent ?? "Unknown" },
            { "IpAddress", ipAddress ?? "Unknown" },
            { "UserId", userId }
        };

        try
        {
            // LOG OBLIGATOIRE - Début de requête
            await _logsClient.LogInformationAsync(
                $"Requête reçue: {httpMethod} {requestPath}", 
                baseProperties);

            // Exécution de la requête
            await _next(context);

            stopwatch.Stop();

            // Ajout du status code aux properties
            var successProperties = new Dictionary<string, object>(baseProperties)
            {
                { "StatusCode", context.Response.StatusCode },
                { "Duration", stopwatch.ElapsedMilliseconds }
            };

            // LOG OBLIGATOIRE - Fin de requête réussie
            await _logsClient.LogInformationAsync(
                $"Requête terminée: {context.Response.StatusCode} en {stopwatch.ElapsedMilliseconds}ms", 
                successProperties);

            // MÉTRIQUE OBLIGATOIRE - Performance
            await _metricsClient.RecordTimingAsync(
                $"request_duration",
                stopwatch.Elapsed,
                new Dictionary<string, string>
                {
                    { "method", httpMethod },
                    { "path", requestPath },
                    { "status", context.Response.StatusCode.ToString() }
                });

            // MÉTRIQUE OBLIGATOIRE - Compteur requêtes
            await _metricsClient.RecordMetricAsync(
                "request_count",
                1,
                "count",
                new Dictionary<string, string>
                {
                    { "method", httpMethod },
                    { "path", requestPath },
                    { "status", context.Response.StatusCode.ToString() }
                });

        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Properties pour erreur
            var errorProperties = new Dictionary<string, object>(baseProperties)
            {
                { "Duration", stopwatch.ElapsedMilliseconds },
                { "ExceptionType", ex.GetType().Name },
                { "ExceptionMessage", ex.Message }
            };

            // LOG OBLIGATOIRE - Erreur
            await _logsClient.LogErrorAsync(
                ex,
                $"Erreur requête: {httpMethod} {requestPath}",
                errorProperties);

            // MÉTRIQUE OBLIGATOIRE - Erreur
            await _metricsClient.RecordMetricAsync(
                "request_errors",
                1,
                "count",
                new Dictionary<string, string>
                {
                    { "method", httpMethod },
                    { "path", requestPath },
                    { "exception", ex.GetType().Name }
                });

            // Rethrow pour maintenir le comportement normal
            throw;
        }
    }
}

/// <summary>
/// Middleware OBLIGATOIRE d'audit automatique pour tous les CUD operations
/// </summary>
public class NiesProAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditServiceClient _auditClient;

    public NiesProAuditMiddleware(RequestDelegate next, IAuditServiceClient auditClient)
    {
        _next = next;
        _auditClient = auditClient;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Audit OBLIGATOIRE pour les opérations CUD
        if (ShouldAudit(context))
        {
            await AuditOperation(context);
        }
    }

    private bool ShouldAudit(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;
        var statusCode = context.Response.StatusCode;

        // Auditer seulement les opérations CUD réussies
        return (method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE") &&
               statusCode >= 200 && statusCode < 300 &&
               !path.Contains("/health") && 
               !path.Contains("/swagger");
    }

    private async Task AuditOperation(HttpContext context)
    {
        try
        {
            var userId = context.User?.Identity?.Name ?? "System";
            var userName = context.User?.Identity?.Name ?? "System";
            var httpMethod = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            var correlationId = context.Items["CorrelationId"]?.ToString();

            // Déterminer l'action et l'entité depuis le path
            var (action, entityName) = ParseActionFromPath(httpMethod, path);

            var metadata = new Dictionary<string, object>
            {
                { "HttpMethod", httpMethod },
                { "RequestPath", path },
                { "StatusCode", context.Response.StatusCode },
                { "CorrelationId", correlationId ?? "Unknown" },
                { "IpAddress", context.Connection.RemoteIpAddress?.ToString() ?? "Unknown" },
                { "UserAgent", context.Request.Headers["User-Agent"].ToString() }
            };

            await _auditClient.AuditAsync(userId, userName, action, entityName, null, metadata);
        }
        catch
        {
            // Silent fail pour ne pas casser l'application
        }
    }

    private (string action, string entityName) ParseActionFromPath(string httpMethod, string path)
    {
        // Extraction de l'entité depuis le path (ex: /api/products -> Products)
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var entityName = segments.Length > 1 ? segments[1] : "Unknown";

        // Capitaliser le nom de l'entité
        if (entityName.Length > 0)
        {
            entityName = char.ToUpper(entityName[0]) + entityName[1..];
        }

        var action = httpMethod switch
        {
            "POST" => $"CREATE_{entityName.ToUpper()}",
            "PUT" or "PATCH" => $"UPDATE_{entityName.ToUpper()}",
            "DELETE" => $"DELETE_{entityName.ToUpper()}",
            _ => $"ACCESS_{entityName.ToUpper()}"
        };

        return (action, entityName);
    }
}

/// <summary>
/// Extensions pour ajouter les middleware OBLIGATOIRES
/// </summary>
public static class NiesProLoggingMiddlewareExtensions
{
    /// <summary>
    /// Ajoute le middleware de logging OBLIGATOIRE
    /// DOIT être appelé par tous les services NiesPro
    /// </summary>
    public static IApplicationBuilder UseNiesProLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NiesProLoggingMiddleware>();
    }

    /// <summary>
    /// Ajoute le middleware d'audit OBLIGATOIRE
    /// DOIT être appelé par tous les services NiesPro
    /// </summary>
    public static IApplicationBuilder UseNiesProAudit(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NiesProAuditMiddleware>();
    }

    /// <summary>
    /// Ajoute TOUS les middlewares NiesPro OBLIGATOIRES
    /// </summary>
    public static IApplicationBuilder UseNiesProCompleteLogging(this IApplicationBuilder builder)
    {
        return builder
            .UseNiesProLogging()
            .UseNiesProAudit();
    }
}