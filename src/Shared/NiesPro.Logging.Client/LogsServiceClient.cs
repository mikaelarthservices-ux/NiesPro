using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NiesPro.Logging.Client;

/// <summary>
/// Client pour l'intégration avec le service Logs centralisé
/// OBLIGATOIRE pour tous les microservices NiesPro
/// </summary>
public interface ILogsServiceClient
{
    Task LogAsync(LogLevel level, string message, string? exception = null, Dictionary<string, object>? properties = null);
    Task LogErrorAsync(Exception exception, string message, Dictionary<string, object>? properties = null);
    Task LogInformationAsync(string message, Dictionary<string, object>? properties = null);
    Task LogWarningAsync(string message, Dictionary<string, object>? properties = null);
}

/// <summary>
/// Client pour l'audit centralisé
/// OBLIGATOIRE pour tous les CUD operations
/// </summary>
public interface IAuditServiceClient
{
    Task AuditAsync(string userId, string userName, string action, string entityName, string? entityId = null, Dictionary<string, object>? metadata = null);
    Task AuditCreateAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null);
    Task AuditUpdateAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null);
    Task AuditDeleteAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null);
}

/// <summary>
/// Client pour les métriques de performance
/// OBLIGATOIRE pour tous les endpoints critiques
/// </summary>
public interface IMetricsServiceClient
{
    Task RecordMetricAsync(string metricName, double value, string unit = "count", Dictionary<string, string>? tags = null);
    Task RecordTimingAsync(string operationName, TimeSpan duration, Dictionary<string, string>? tags = null);
    IDisposable StartTimer(string operationName, Dictionary<string, string>? tags = null);
}

/// <summary>
/// Client pour les alertes système
/// OBLIGATOIRE pour les conditions critiques
/// </summary>
public interface IAlertServiceClient
{
    Task CreateAlertAsync(string title, string description, AlertSeverity severity, Dictionary<string, object>? context = null);
    Task CreateCriticalAlertAsync(string title, string description, Dictionary<string, object>? context = null);
    Task CreateWarningAlertAsync(string title, string description, Dictionary<string, object>? context = null);
}

/// <summary>
/// Niveaux de sévérité des alertes
/// </summary>
public enum AlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Configuration du service Logs
/// </summary>
public class LogsServiceConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int RetryAttempts { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableHealthChecks { get; set; } = true;
}

/// <summary>
/// Extensions pour l'injection de dépendance
/// </summary>
public static class LogsServiceExtensions
{
    /// <summary>
    /// Ajoute les clients Logs/Audit OBLIGATOIRES
    /// </summary>
    public static IServiceCollection AddNiesProLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        var logsConfig = configuration.GetSection("LogsService").Get<LogsServiceConfiguration>()
            ?? throw new InvalidOperationException("Configuration LogsService manquante dans appsettings.json");

        services.Configure<LogsServiceConfiguration>(configuration.GetSection("LogsService"));

        // HTTP Client configuré
        services.AddHttpClient<ILogsServiceClient, LogsServiceClient>(client =>
        {
            client.BaseAddress = new Uri(logsConfig.BaseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", logsConfig.ApiKey);
            client.DefaultRequestHeaders.Add("X-Service-Name", logsConfig.ServiceName);
            client.Timeout = TimeSpan.FromSeconds(logsConfig.TimeoutSeconds);
        });

        // Enregistrement des clients
        services.AddScoped<ILogsServiceClient, LogsServiceClient>();
        services.AddScoped<IAuditServiceClient, AuditServiceClient>();
        services.AddScoped<IMetricsServiceClient, MetricsServiceClient>();
        services.AddScoped<IAlertServiceClient, AlertServiceClient>();

        // Health checks si activés
        if (logsConfig.EnableHealthChecks)
        {
            services.AddHealthChecks()
                .AddCheck<LogsServiceHealthCheck>("logs-service");
        }

        return services;
    }
}

/// <summary>
/// Implémentation du client Logs
/// </summary>
public class LogsServiceClient : ILogsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly LogsServiceConfiguration _config;
    private readonly ILogger<LogsServiceClient> _logger;

    public LogsServiceClient(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<LogsServiceConfiguration> config, ILogger<LogsServiceClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task LogAsync(LogLevel level, string message, string? exception = null, Dictionary<string, object>? properties = null)
    {
        try
        {
            var logEntry = new
            {
                ServiceName = _config.ServiceName,
                Level = level.ToString(),
                Message = message,
                Category = "Application",
                Severity = MapLogLevelToSeverity(level),
                Exception = exception,
                Properties = properties ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(logEntry);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/api/logs", content);
        }
        catch (Exception ex)
        {
            // Fallback logging - ne pas faire échouer l'application
            _logger.LogError(ex, "Erreur envoi log vers service centralisé");
        }
    }

    public async Task LogErrorAsync(Exception exception, string message, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Error, message, exception.ToString(), properties);
    }

    public async Task LogInformationAsync(string message, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Information, message, null, properties);
    }

    public async Task LogWarningAsync(string message, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Warning, message, null, properties);
    }

    private int MapLogLevelToSeverity(LogLevel level) => level switch
    {
        LogLevel.Critical or LogLevel.Error => 3,
        LogLevel.Warning => 2,
        LogLevel.Information or LogLevel.Debug or LogLevel.Trace => 1,
        _ => 1
    };
}

/// <summary>
/// Implémentation du client Audit
/// </summary>
public class AuditServiceClient : IAuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly LogsServiceConfiguration _config;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<LogsServiceConfiguration> config, ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task AuditAsync(string userId, string userName, string action, string entityName, string? entityId = null, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var auditEntry = new
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                ActionType = DetermineActionType(action),
                EntityName = entityName,
                EntityId = entityId,
                ServiceName = _config.ServiceName,
                Metadata = metadata ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(auditEntry);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/api/audits", content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur envoi audit vers service centralisé");
        }
    }

    public async Task AuditCreateAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null)
    {
        await AuditAsync(userId, userName, $"CREATE_{entityName.ToUpper()}", entityName, entityId, metadata);
    }

    public async Task AuditUpdateAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null)
    {
        await AuditAsync(userId, userName, $"UPDATE_{entityName.ToUpper()}", entityName, entityId, metadata);
    }

    public async Task AuditDeleteAsync(string userId, string userName, string entityName, string entityId, Dictionary<string, object>? metadata = null)
    {
        await AuditAsync(userId, userName, $"DELETE_{entityName.ToUpper()}", entityName, entityId, metadata);
    }

    private int DetermineActionType(string action)
    {
        return action.ToUpper() switch
        {
            var a when a.Contains("CREATE") => 1, // Create
            var a when a.Contains("UPDATE") || a.Contains("MODIFY") => 2, // Update  
            var a when a.Contains("DELETE") || a.Contains("REMOVE") => 3, // Delete
            var a when a.Contains("READ") || a.Contains("GET") || a.Contains("VIEW") => 4, // Read
            _ => 5 // Other
        };
    }
}

/// <summary>
/// Implémentation du client Métriques
/// </summary>
public class MetricsServiceClient : IMetricsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly LogsServiceConfiguration _config;

    public MetricsServiceClient(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<LogsServiceConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task RecordMetricAsync(string metricName, double value, string unit = "count", Dictionary<string, string>? tags = null)
    {
        try
        {
            var metric = new
            {
                ServiceName = _config.ServiceName,
                MetricName = metricName,
                Value = value,
                Unit = unit,
                Type = 1, // Counter
                Tags = tags ?? new Dictionary<string, string>(),
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(metric);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/api/metrics", content);
        }
        catch
        {
            // Silent fail pour les métriques
        }
    }

    public async Task RecordTimingAsync(string operationName, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        await RecordMetricAsync($"{operationName}_duration", duration.TotalMilliseconds, "milliseconds", tags);
    }

    public IDisposable StartTimer(string operationName, Dictionary<string, string>? tags = null)
    {
        return new MetricTimer(this, operationName, tags);
    }

    private class MetricTimer : IDisposable
    {
        private readonly MetricsServiceClient _client;
        private readonly string _operationName;
        private readonly Dictionary<string, string>? _tags;
        private readonly System.Diagnostics.Stopwatch _stopwatch;

        public MetricTimer(MetricsServiceClient client, string operationName, Dictionary<string, string>? tags)
        {
            _client = client;
            _operationName = operationName;
            _tags = tags;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _ = _client.RecordTimingAsync(_operationName, _stopwatch.Elapsed, _tags);
        }
    }
}

/// <summary>
/// Implémentation du client Alertes
/// </summary>
public class AlertServiceClient : IAlertServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly LogsServiceConfiguration _config;

    public AlertServiceClient(HttpClient httpClient, Microsoft.Extensions.Options.IOptions<LogsServiceConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task CreateAlertAsync(string title, string description, AlertSeverity severity, Dictionary<string, object>? context = null)
    {
        try
        {
            var alert = new
            {
                Title = title,
                Description = description,
                Severity = (int)severity,
                ServiceName = _config.ServiceName,
                Status = 1, // Open
                Context = context ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(alert);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            await _httpClient.PostAsync("/api/alerts", content);
        }
        catch
        {
            // Silent fail pour les alertes
        }
    }

    public async Task CreateCriticalAlertAsync(string title, string description, Dictionary<string, object>? context = null)
    {
        await CreateAlertAsync(title, description, AlertSeverity.Critical, context);
    }

    public async Task CreateWarningAlertAsync(string title, string description, Dictionary<string, object>? context = null)
    {
        await CreateAlertAsync(title, description, AlertSeverity.Medium, context);
    }
}

/// <summary>
/// Health check pour le service Logs
/// </summary>
public class LogsServiceHealthCheck : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
{
    private readonly HttpClient _httpClient;

    public LogsServiceHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Service Logs accessible");
            }
            
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Service Logs inaccessible: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Service Logs inaccessible", ex);
        }
    }
}