using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NiesPro.AdminPanel.Services;
using NiesPro.AdminPanel.Models;
using System.Collections.ObjectModel;

namespace NiesPro.AdminPanel.ViewModels;

/// <summary>
/// ViewModel principal de l'application avec navigation et monitoring
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IMonitoringService _monitoringService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string title = "NiesPro Administration Panel";

    [ObservableProperty]
    private bool isAuthenticated;

    [ObservableProperty]
    private UserInfo? currentUser;

    [ObservableProperty]
    private string selectedView = "Dashboard";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = "Prêt";

    public ObservableCollection<ServiceHealth> Services => _monitoringService.Services;
    public ObservableCollection<NotificationItem> Notifications => _notificationService.Notifications;

    // ViewModels des vues
    public DashboardViewModel DashboardViewModel { get; }
    public UsersViewModel UsersViewModel { get; }
    public OrdersViewModel OrdersViewModel { get; }
    public ProductsViewModel ProductsViewModel { get; }
    public PaymentsViewModel PaymentsViewModel { get; }

    public MainWindowViewModel(
        IAuthenticationService authService,
        IMonitoringService monitoringService,
        INotificationService notificationService,
        ILogger<MainWindowViewModel> logger,
        DashboardViewModel dashboardViewModel,
        UsersViewModel usersViewModel,
        OrdersViewModel ordersViewModel,
        ProductsViewModel productsViewModel,
        PaymentsViewModel paymentsViewModel)
    {
        _authService = authService;
        _monitoringService = monitoringService;
        _notificationService = notificationService;
        _logger = logger;

        DashboardViewModel = dashboardViewModel;
        UsersViewModel = usersViewModel;
        OrdersViewModel = ordersViewModel;
        ProductsViewModel = productsViewModel;
        PaymentsViewModel = paymentsViewModel;

        // S'abonner aux événements
        _authService.AuthenticationChanged += OnAuthenticationChanged;
        _monitoringService.ServiceStatusChanged += OnServiceStatusChanged;

        // Initialiser l'état
        UpdateAuthenticationState();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Initialisation...";

            _logger.LogInformation("Initializing main window");

            // Démarrer le monitoring des services
            await _monitoringService.StartMonitoringAsync();

            StatusMessage = "Prêt";
            
            _notificationService.AddInfo("Application", "NiesPro Admin Panel démarré avec succès");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initialization");
            _notificationService.AddError("Erreur", $"Erreur lors de l'initialisation: {ex.Message}");
            StatusMessage = "Erreur d'initialisation";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateTo(string viewName)
    {
        if (!string.IsNullOrEmpty(viewName) && viewName != SelectedView)
        {
            SelectedView = viewName;
            _logger.LogInformation("Navigated to view: {ViewName}", viewName);
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Déconnexion...";

            await _authService.LogoutAsync();
            await _monitoringService.StopMonitoringAsync();
            
            _notificationService.AddInfo("Authentification", "Déconnexion réussie");
            
            // Revenir au dashboard après déconnexion
            SelectedView = "Dashboard";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            _notificationService.AddError("Erreur", $"Erreur lors de la déconnexion: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Prêt";
        }
    }

    [RelayCommand]
    private void ClearNotifications()
    {
        _notificationService.ClearAll();
    }

    [RelayCommand]
    private void RemoveNotification(NotificationItem notification)
    {
        _notificationService.Remove(notification);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Actualisation en cours...";
            
            // Redémarrer le monitoring pour actualiser les services
            await _monitoringService.StopMonitoringAsync();
            await _monitoringService.StartMonitoringAsync();
            
            // Actualiser le dashboard si c'est la vue active
            if (SelectedView == "Dashboard")
            {
                await DashboardViewModel.LoadDataCommand.ExecuteAsync(null);
            }
            
            StatusMessage = "Actualisation terminée";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refresh");
            StatusMessage = "Erreur lors de l'actualisation";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshServicesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Actualisation des services...";

            await _monitoringService.StopMonitoringAsync();
            await _monitoringService.StartMonitoringAsync();

            _notificationService.AddSuccess("Monitoring", "Services actualisés");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing services");
            _notificationService.AddError("Erreur", $"Erreur lors de l'actualisation: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Prêt";
        }
    }

    private void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
        UpdateAuthenticationState();
        
        if (isAuthenticated)
        {
            _notificationService.AddSuccess("Authentification", "Connexion réussie");
            _ = Task.Run(async () => await _monitoringService.StartMonitoringAsync());
        }
        else
        {
            _notificationService.AddInfo("Authentification", "Session fermée");
            _ = Task.Run(async () => await _monitoringService.StopMonitoringAsync());
        }
    }

    private void OnServiceStatusChanged(object? sender, ServiceHealth serviceHealth)
    {
        var message = serviceHealth.Status switch
        {
            ServiceStatus.Healthy => $"Service {serviceHealth.Name} est en ligne",
            ServiceStatus.Unhealthy => $"Service {serviceHealth.Name} est hors ligne: {serviceHealth.ErrorMessage}",
            ServiceStatus.Degraded => $"Service {serviceHealth.Name} fonctionne avec des problèmes",
            _ => $"Service {serviceHealth.Name} - statut inconnu"
        };

        var notificationType = serviceHealth.Status switch
        {
            ServiceStatus.Healthy => NotificationType.Success,
            ServiceStatus.Unhealthy => NotificationType.Error,
            ServiceStatus.Degraded => NotificationType.Warning,
            _ => NotificationType.Info
        };

        // Ajouter notification selon le type
        switch (notificationType)
        {
            case NotificationType.Success:
                _notificationService.AddSuccess("Service Status", message);
                break;
            case NotificationType.Error:
                _notificationService.AddError("Service Status", message);
                break;
            case NotificationType.Warning:
                _notificationService.AddWarning("Service Status", message);
                break;
            default:
                _notificationService.AddInfo("Service Status", message);
                break;
        }
    }

    private void UpdateAuthenticationState()
    {
        IsAuthenticated = _authService.IsAuthenticated;
        CurrentUser = _authService.CurrentUser;
        
        if (IsAuthenticated && CurrentUser != null)
        {
            Title = $"NiesPro Admin Panel - {CurrentUser.FirstName} {CurrentUser.LastName}";
        }
        else
        {
            Title = "NiesPro Administration Panel";
        }
    }

    public async Task CleanupAsync()
    {
        try
        {
            await _monitoringService.StopMonitoringAsync();
            _authService.AuthenticationChanged -= OnAuthenticationChanged;
            _monitoringService.ServiceStatusChanged -= OnServiceStatusChanged;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }
    }
}