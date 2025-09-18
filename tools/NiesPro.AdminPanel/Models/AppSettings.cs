namespace NiesPro.AdminPanel.Models;

/// <summary>
/// Configuration de l'application
/// </summary>
public class AppSettings
{
    public LoggingSettings Logging { get; set; } = new();
    public ApiEndpointsSettings ApiEndpoints { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public MonitoringSettings Monitoring { get; set; } = new();
    public UISettings UI { get; set; } = new();
}

public class LoggingSettings
{
    public Dictionary<string, string> LogLevel { get; set; } = new();
}

public class ApiEndpointsSettings
{
    public string Gateway { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
}

public class AuthenticationSettings
{
    public int TokenRefreshMinutes { get; set; } = 30;
    public int DefaultTimeout { get; set; } = 30;
}

public class MonitoringSettings
{
    public int RefreshIntervalSeconds { get; set; } = 5;
    public int HealthCheckTimeoutSeconds { get; set; } = 10;
    public int MaxRetryAttempts { get; set; } = 3;
}

public class UISettings
{
    public string Theme { get; set; } = "Dark";
    public string PrimaryColor { get; set; } = "#2196F3";
    public string AccentColor { get; set; } = "#FF4081";
}