using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NiesPro.AdminPanel.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace NiesPro.AdminPanel.Services;

/// <summary>
/// Service de monitoring en temps réel des microservices
/// </summary>
public interface IMonitoringService : IDisposable
{
    ObservableCollection<ServiceHealth> Services { get; }
    Task StartMonitoringAsync();
    Task StopMonitoringAsync();
    Task<MetricsData?> GetServiceMetricsAsync(string serviceName);
    event EventHandler<ServiceHealth>? ServiceStatusChanged;
}

public class MonitoringService : IMonitoringService, IDisposable
{
    private readonly IApiService _apiService;
    private readonly ILogger<MonitoringService> _logger;
    private readonly AppSettings _settings;
    private Timer? _monitoringTimer;
    private bool _isMonitoring;

    public ObservableCollection<ServiceHealth> Services { get; private set; }
    public event EventHandler<ServiceHealth>? ServiceStatusChanged;

    public MonitoringService(IApiService apiService, ILogger<MonitoringService> logger, IOptions<AppSettings> settings)
    {
        _apiService = apiService;
        _logger = logger;
        _settings = settings.Value;
        Services = new ObservableCollection<ServiceHealth>();

        InitializeServices();
    }

    private void InitializeServices()
    {
        var serviceEndpoints = new Dictionary<string, string>
        {
            { "Gateway", _settings.ApiEndpoints.Gateway },
            { "Auth", _settings.ApiEndpoints.Auth },
            { "Order", _settings.ApiEndpoints.Order },
            { "Catalog", _settings.ApiEndpoints.Catalog },
            { "Payment", _settings.ApiEndpoints.Payment }
        };

        foreach (var (name, url) in serviceEndpoints)
        {
            Services.Add(new ServiceHealth
            {
                Name = name,
                Url = url,
                Status = ServiceStatus.Unknown,
                LastCheck = DateTime.Now
            });
        }
    }

    public async Task StartMonitoringAsync()
    {
        if (_isMonitoring)
        {
            return;
        }

        _logger.LogInformation("Starting service monitoring");
        _isMonitoring = true;

        // Première vérification immédiate
        await CheckAllServicesAsync();

        // Programmer les vérifications périodiques
        var interval = TimeSpan.FromSeconds(_settings.Monitoring.RefreshIntervalSeconds);
        _monitoringTimer = new Timer(async _ =>
        {
            if (_isMonitoring)
            {
                await CheckAllServicesAsync();
            }
        }, null, interval, interval);

        _logger.LogInformation("Service monitoring started with {Interval}s interval", 
            _settings.Monitoring.RefreshIntervalSeconds);
    }

    public Task StopMonitoringAsync()
    {
        if (!_isMonitoring)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Stopping service monitoring");
        _isMonitoring = false;
        
        _monitoringTimer?.Dispose();
        _monitoringTimer = null;
        
        return Task.CompletedTask;
    }

    private async Task CheckAllServicesAsync()
    {
        // Créer une copie de la collection pour éviter les modifications concurrentes
        var servicesToCheck = Services.ToList();
        
        var tasks = servicesToCheck.Select(async service =>
        {
            try
            {
                var previousStatus = service.Status;
                var health = await _apiService.CheckServiceHealthAsync(service.Name, service.Url);
                
                // Mettre à jour le service dans la collection sur le thread UI
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var index = Services.IndexOf(service);
                    if (index >= 0)
                    {
                        Services[index] = health;
                        
                        // Notifier si le statut a changé
                        if (previousStatus != health.Status)
                        {
                            ServiceStatusChanged?.Invoke(this, health);
                            
                            _logger.LogInformation("Service {ServiceName} status changed from {OldStatus} to {NewStatus}",
                                service.Name, previousStatus, health.Status);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service {ServiceName}", service.Name);
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task<MetricsData?> GetServiceMetricsAsync(string serviceName)
    {
        try
        {
            var serviceKey = serviceName.ToLowerInvariant();
            var endpoint = serviceKey switch
            {
                "gateway" => "api/gateway/metrics",
                "auth" => "api/v1/auth/metrics",
                "order" => "api/v1/orders/metrics",
                "catalog" => "api/v1/products/metrics",
                "payment" => "api/v1/payments/metrics",
                _ => null
            };

            if (endpoint == null)
            {
                return null;
            }

            var metrics = await _apiService.GetAsync<MetricsData>(endpoint, serviceKey);
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics for service {ServiceName}", serviceName);
            return null;
        }
    }

    public void Dispose()
    {
        StopMonitoringAsync().Wait();
    }
}