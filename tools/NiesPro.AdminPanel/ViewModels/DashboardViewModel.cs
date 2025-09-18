using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NiesPro.AdminPanel.Services;
using NiesPro.AdminPanel.Models;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Diagnostics;

namespace NiesPro.AdminPanel.ViewModels;

/// <summary>
/// ViewModel professionnel du dashboard - données réelles uniquement
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private int totalUsers = 0;

    [ObservableProperty]
    private int totalOrders = 0;

    [ObservableProperty]
    private int totalProducts = 0;

    [ObservableProperty]
    private decimal dailyRevenue = 0;

    [ObservableProperty]
    private string dailyRevenueText = "N/A";

    [ObservableProperty]
    private ObservableCollection<ServiceItem> serviceStatuses = new();

    [ObservableProperty]
    private ObservableCollection<string> recentActivities = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string currentUser = "Admin";

    [ObservableProperty]
    private bool isServiceOperationInProgress = false;

    [ObservableProperty]
    private string serviceOperationStatus = "";

    public DashboardViewModel(IApiService apiService, IMonitoringService monitoringService, ILogger<DashboardViewModel> logger)
    {
        _apiService = apiService;
        _monitoringService = monitoringService;
        _logger = logger;
        
        InitializeServiceStatuses();
        LoadPlaceholderActivities();
        
        // Lancer automatiquement la vérification des services au démarrage
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000); // Petit délai pour permettre à l'application de se stabiliser
            await RefreshServicesAsync();
        });
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading real dashboard data from APIs...");
            
            // Charger les données réelles depuis les APIs NiesPro et vérifier les services
            await Task.WhenAll(
                LoadUsersCountAsync(),
                LoadOrdersCountAsync(), 
                LoadProductsCountAsync(),
                LoadDailyRevenueAsync(),
                LoadRecentActivitiesAsync(),
                RefreshServicesAsync() // Ajouter la vérification des services
            );
            
            _logger.LogInformation("Dashboard data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadUsersCountAsync()
    {
        try
        {
            // Appel réel à l'API Auth pour obtenir la liste paginée et compter
            var response = await _apiService.GetAsync<PaginatedResult<UserDto>>("api/v1/users?pageSize=1", "auth");
            TotalUsers = response?.TotalCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load users count from Auth API");
            TotalUsers = 0;
        }
    }

    private async Task LoadOrdersCountAsync()
    {
        try
        {
            // Appel réel à l'API Order pour obtenir la liste paginée et compter
            var response = await _apiService.GetAsync<PaginatedResult<OrderDto>>("api/v1/orders?pageSize=1", "order");
            TotalOrders = response?.TotalCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load orders count from Order API");
            TotalOrders = 0;
        }
    }

    private async Task LoadProductsCountAsync()
    {
        try
        {
            // Appel réel à l'API Catalog pour obtenir la liste paginée et compter
            var response = await _apiService.GetAsync<PaginatedResult<ProductDto>>("api/v1/products?pageSize=1", "catalog");
            TotalProducts = response?.TotalCount ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load products count from Catalog API");
            TotalProducts = 0;
        }
    }

    private async Task LoadDailyRevenueAsync()
    {
        try
        {
            // Appel réel à l'API Payment pour obtenir les métriques de paiement
            var response = await _apiService.GetAsync<PaymentMetricsDto>("api/v1/analytics/payment-metrics?period=Today", "payment");
            if (response != null)
            {
                DailyRevenue = response.TotalAmount;
                DailyRevenueText = $"€{DailyRevenue:N0}";
            }
            else
            {
                DailyRevenueText = "N/A";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load daily revenue from Payment API");
            DailyRevenue = 0;
            DailyRevenueText = "N/A";
        }
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            // Appel réel pour récupérer l'activité récente
            var response = await _apiService.GetAsync<ActivityResponse[]>("Gateway/api/activities/recent");
            
            RecentActivities.Clear();
            if (response != null && response.Length > 0)
            {
                foreach (var activity in response.Take(5))
                {
                    var timeAgo = DateTime.Now - activity.Timestamp;
                    var timeText = timeAgo.TotalMinutes < 60 
                        ? $"{(int)timeAgo.TotalMinutes} min" 
                        : $"{(int)timeAgo.TotalHours}h";
                    
                    RecentActivities.Add($"• {activity.Description} (il y a {timeText})");
                }
            }
            else
            {
                RecentActivities.Add("Aucune activité récente disponible");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load recent activities");
            RecentActivities.Clear();
            RecentActivities.Add("Impossible de charger l'activité récente - vérifiez la connexion aux services");
        }
    }

    private void InitializeServiceStatuses()
    {
        ServiceStatuses.Add(new ServiceItem { Name = "Gateway API", Port = 5000, IsHealthy = false });
        ServiceStatuses.Add(new ServiceItem { Name = "Auth API", Port = 5001, IsHealthy = false });
        ServiceStatuses.Add(new ServiceItem { Name = "Order API", Port = 5002, IsHealthy = false });
        ServiceStatuses.Add(new ServiceItem { Name = "Catalog API", Port = 5003, IsHealthy = false });
        ServiceStatuses.Add(new ServiceItem { Name = "Payment API", Port = 5004, IsHealthy = false });
    }

    private void LoadPlaceholderActivities()
    {
        RecentActivities.Add("Aucune activité récente disponible");
        RecentActivities.Add("Connectez-vous aux services pour voir l'activité en temps réel");
    }

    [RelayCommand]
    public async Task RefreshServicesAsync()
    {
        try
        {
            _logger.LogInformation("Checking real service health status...");
            
            // Vérification réelle de l'état des services
            foreach (var service in ServiceStatuses)
            {
                try
                {
                    var baseUrl = $"https://localhost:{service.Port}";
                    var health = await _apiService.CheckServiceHealthAsync(service.Name, baseUrl);
                    service.IsHealthy = health.Status == Models.ServiceStatus.Healthy;
                    service.LastCheck = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Health check failed for {ServiceName}", service.Name);
                    service.IsHealthy = false;
                    service.LastCheck = DateTime.Now;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during services health check");
        }
    }

    [RelayCommand]
    public async Task StartAllServicesAsync()
    {
        if (IsServiceOperationInProgress) return;
        
        try
        {
            IsServiceOperationInProgress = true;
            ServiceOperationStatus = "Démarrage des services en cours...";
            _logger.LogInformation("Starting all NiesPro services in background...");
            
            var scriptPath = @"c:\Users\HP\Documents\projets\NiesPro\start-all-services-background.ps1";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -Silent",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                _logger.LogInformation("Services startup completed in background");
                
                ServiceOperationStatus = "Vérification de l'état des services...";
                // Attendre un peu plus pour que les services soient prêts
                await Task.Delay(8000);
                await RefreshServicesAsync();
                ServiceOperationStatus = "Services démarrés avec succès";
                await Task.Delay(2000); // Afficher le message de succès
            }
        }
        catch (Exception ex)
        {
            ServiceOperationStatus = "Erreur lors du démarrage des services";
            _logger.LogError(ex, "Error starting services in background");
            await Task.Delay(3000);
        }
        finally
        {
            IsServiceOperationInProgress = false;
            ServiceOperationStatus = "";
        }
    }

    [RelayCommand]
    public async Task StopAllServicesAsync()
    {
        if (IsServiceOperationInProgress) return;
        
        try
        {
            IsServiceOperationInProgress = true;
            ServiceOperationStatus = "Arrêt des services en cours...";
            _logger.LogInformation("Stopping all NiesPro services...");
            
            var scriptPath = @"c:\Users\HP\Documents\projets\NiesPro\stop-all-services.ps1";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                _logger.LogInformation("All services stopped");
                
                ServiceOperationStatus = "Vérification de l'arrêt...";
                await Task.Delay(2000);
                await RefreshServicesAsync();
                ServiceOperationStatus = "Services arrêtés avec succès";
                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            ServiceOperationStatus = "Erreur lors de l'arrêt des services";
            _logger.LogError(ex, "Error stopping services");
            await Task.Delay(3000);
        }
        finally
        {
            IsServiceOperationInProgress = false;
            ServiceOperationStatus = "";
        }
    }

    [RelayCommand]
    public async Task RestartAllServicesAsync()
    {
        if (IsServiceOperationInProgress) return;
        
        try
        {
            IsServiceOperationInProgress = true;
            ServiceOperationStatus = "Redémarrage des services en cours...";
            _logger.LogInformation("Restarting all NiesPro services...");
            
            var scriptPath = @"c:\Users\HP\Documents\projets\NiesPro\restart-all-services.ps1";
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -Silent",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                _logger.LogInformation("All services restarted");
                
                ServiceOperationStatus = "Vérification du redémarrage...";
                await Task.Delay(10000);
                await RefreshServicesAsync();
                ServiceOperationStatus = "Services redémarrés avec succès";
                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            ServiceOperationStatus = "Erreur lors du redémarrage des services";
            _logger.LogError(ex, "Error restarting services");
            await Task.Delay(3000);
        }
        finally
        {
            IsServiceOperationInProgress = false;
            ServiceOperationStatus = "";
        }
    }
}

// DTOs pour les réponses réelles des APIs
public class UserCountResponse
{
    public int Count { get; set; }
}

public class OrderCountResponse  
{
    public int Count { get; set; }
}

public class ProductCountResponse
{
    public int Count { get; set; }
}

public class RevenueResponse
{
    public decimal Amount { get; set; }
}

public class ActivityResponse
{
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
}

public partial class ServiceItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int port;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText), nameof(StatusColor), nameof(StatusBackground))]
    private bool isHealthy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText), nameof(StatusColor), nameof(StatusBackground))]
    private DateTime? lastCheck;

    [ObservableProperty]
    private TimeSpan? responseTime;

    public string StatusText => LastCheck.HasValue 
        ? (IsHealthy ? "En ligne" : "Hors ligne") 
        : "Non vérifié";
    
    public string StatusColor => LastCheck.HasValue 
        ? (IsHealthy ? "#4CAF50" : "#F44336") 
        : "#9E9E9E";
    
    public string StatusBackground => LastCheck.HasValue 
        ? (IsHealthy ? "#E8F5E8" : "#FFEBEE") 
        : "#F5F5F5";
}