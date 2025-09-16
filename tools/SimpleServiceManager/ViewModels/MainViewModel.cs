using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using NiesPro.ServiceAdmin.Models;
using NiesPro.ServiceAdmin.Services;

namespace NiesPro.ServiceAdmin.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IMicroserviceManager _serviceManager;
        private readonly ILogger<MainViewModel> _logger;
        private DispatcherTimer? _updateTimer;
        private DispatcherTimer? _healthCheckTimer;
        private CancellationTokenSource? _cancellationTokenSource;
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private MicroserviceInfo? _selectedService;

        public ObservableCollection<MicroserviceInfo> Services { get; }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        public bool IsNotBusy => !IsBusy;

        public MicroserviceInfo? SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged();
            }
        }

        public string WindowTitle { get; }
        public string ApplicationVersion { get; }

        // Commands
        public ICommand StartAllCommand { get; }
        public ICommand StopAllCommand { get; }
        public ICommand RestartAllCommand { get; }
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }
        public ICommand RestartServiceCommand { get; }
        public ICommand OpenHealthCommand { get; }
        public ICommand OpenSwaggerCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(IMicroserviceManager serviceManager, ILogger<MainViewModel> logger)
        {
            _serviceManager = serviceManager;
            _logger = logger;

            WindowTitle = "NiesPro - Professional Service Administration";
            ApplicationVersion = "v1.0.0";

            // Initialize services
            Services = new ObservableCollection<MicroserviceInfo>
            {
                new("Gateway.API", "API Gateway", "Central API Gateway and routing", 5000, 
                    GetProjectPath("Gateway", "Gateway.API")),
                new("Auth.API", "Authentication Service", "User authentication and authorization", 5001, 
                    GetProjectPath("Services", "Auth", "Auth.API")),
                new("Order.API", "Order Management", "Order processing and management", 5002, 
                    GetProjectPath("Services", "Order", "Order.API")),
                new("Catalog.API", "Product Catalog", "Product and inventory management", 5003, 
                    GetProjectPath("Services", "Catalog", "Catalog.API"))
            };

            // Initialize commands
            StartAllCommand = new AsyncRelayCommand(StartAllServicesAsync, () => IsNotBusy);
            StopAllCommand = new AsyncRelayCommand(StopAllServicesAsync, () => IsNotBusy);
            RestartAllCommand = new AsyncRelayCommand(RestartAllServicesAsync, () => IsNotBusy);
            StartServiceCommand = new AsyncRelayCommand<MicroserviceInfo>(StartServiceAsync, CanExecuteServiceCommand);
            StopServiceCommand = new AsyncRelayCommand<MicroserviceInfo>(StopServiceAsync, CanExecuteServiceCommand);
            RestartServiceCommand = new AsyncRelayCommand<MicroserviceInfo>(RestartServiceAsync, CanExecuteServiceCommand);
            OpenHealthCommand = new RelayCommand<MicroserviceInfo>(OpenHealthEndpoint);
            OpenSwaggerCommand = new RelayCommand<MicroserviceInfo>(OpenSwaggerEndpoint);
            RefreshCommand = new AsyncRelayCommand(RefreshServicesAsync, () => IsNotBusy);

            // Setup timers
            _cancellationTokenSource = new CancellationTokenSource();
            
            _updateTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += async (s, e) => await UpdateServicesStatusAsync();
            _updateTimer.Start();

            _healthCheckTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _healthCheckTimer.Tick += async (s, e) => await CheckAllServicesHealthAsync();
            _healthCheckTimer.Start();

            _logger.LogInformation("Service Administration Tool initialized");
        }

        private async Task StartAllServicesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Starting all services...";
                _logger.LogInformation("Starting all services");

                var tasks = Services.Select(async service =>
                {
                    try
                    {
                        StatusMessage = $"Starting {service.DisplayName}...";
                        await _serviceManager.StartServiceAsync(service);
                        await Task.Delay(1000); // Stagger starts
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to start service {ServiceName}", service.Name);
                    }
                });

                await Task.WhenAll(tasks);
                StatusMessage = "All services started";
                _logger.LogInformation("All services start operations completed");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error starting services";
                _logger.LogError(ex, "Error during start all services operation");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StopAllServicesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Stopping all services...";
                _logger.LogInformation("Stopping all services");

                var tasks = Services.Select(async service =>
                {
                    try
                    {
                        StatusMessage = $"Stopping {service.DisplayName}...";
                        await _serviceManager.StopServiceAsync(service);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to stop service {ServiceName}", service.Name);
                    }
                });

                await Task.WhenAll(tasks);
                StatusMessage = "All services stopped";
                _logger.LogInformation("All services stop operations completed");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error stopping services";
                _logger.LogError(ex, "Error during stop all services operation");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RestartAllServicesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Restarting all services...";
                _logger.LogInformation("Restarting all services");

                await StopAllServicesAsync();
                await Task.Delay(3000); // Wait between stop and start
                await StartAllServicesAsync();

                StatusMessage = "All services restarted";
                _logger.LogInformation("All services restart operations completed");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error restarting services";
                _logger.LogError(ex, "Error during restart all services operation");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StartServiceAsync(MicroserviceInfo? service)
        {
            if (service == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"Starting {service.DisplayName}...";
                
                var result = await _serviceManager.StartServiceAsync(service);
                StatusMessage = result 
                    ? $"{service.DisplayName} started successfully" 
                    : $"Failed to start {service.DisplayName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error starting {service.DisplayName}";
                _logger.LogError(ex, "Error starting service {ServiceName}", service.Name);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task StopServiceAsync(MicroserviceInfo? service)
        {
            if (service == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"Stopping {service.DisplayName}...";
                
                var result = await _serviceManager.StopServiceAsync(service);
                StatusMessage = result 
                    ? $"{service.DisplayName} stopped successfully" 
                    : $"Failed to stop {service.DisplayName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error stopping {service.DisplayName}";
                _logger.LogError(ex, "Error stopping service {ServiceName}", service.Name);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RestartServiceAsync(MicroserviceInfo? service)
        {
            if (service == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = $"Restarting {service.DisplayName}...";
                
                var result = await _serviceManager.RestartServiceAsync(service);
                StatusMessage = result 
                    ? $"{service.DisplayName} restarted successfully" 
                    : $"Failed to restart {service.DisplayName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error restarting {service.DisplayName}";
                _logger.LogError(ex, "Error restarting service {ServiceName}", service.Name);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenHealthEndpoint(MicroserviceInfo? service)
        {
            if (service == null) return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = service.HealthEndpoint,
                    UseShellExecute = true
                });
                _logger.LogInformation("Opened health endpoint for {ServiceName}", service.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open health endpoint for {ServiceName}", service.Name);
            }
        }

        private void OpenSwaggerEndpoint(MicroserviceInfo? service)
        {
            if (service == null) return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = service.SwaggerEndpoint,
                    UseShellExecute = true
                });
                _logger.LogInformation("Opened Swagger endpoint for {ServiceName}", service.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open Swagger endpoint for {ServiceName}", service.Name);
            }
        }

        private async Task RefreshServicesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Refreshing services...";
                
                await UpdateServicesStatusAsync();
                await CheckAllServicesHealthAsync();
                
                StatusMessage = "Services refreshed";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error refreshing services";
                _logger.LogError(ex, "Error refreshing services");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateServicesStatusAsync()
        {
            try
            {
                var tasks = Services.Select(service => _serviceManager.UpdateServiceStatusAsync(service));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating services status");
            }
        }

        private async Task CheckAllServicesHealthAsync()
        {
            try
            {
                var tasks = Services
                    .Where(s => s.Status == ServiceStatus.Running)
                    .Select(service => _serviceManager.CheckServiceHealthAsync(service));
                
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking services health");
            }
        }

        private bool CanExecuteServiceCommand(MicroserviceInfo? service)
        {
            return IsNotBusy && service != null;
        }

        private static string GetProjectPath(params string[] pathParts)
        {
            // Utiliser un chemin absolu direct vers le répertoire src de NiesPro
            var basePath = @"C:\Users\HP\Documents\projets\NiesPro\src";
                
            return Path.Combine(basePath, Path.Combine(pathParts));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Stops all update timers and disposes resources
        /// </summary>
        public void StopUpdates()
        {
            try
            {
                _logger.LogInformation("Stopping all update timers");
                
                _updateTimer?.Stop();
                _updateTimer = null;
                
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                
                _logger.LogInformation("Update timers stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping update timers");
            }
        }

        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            StopUpdates();
            _updateTimer?.Stop();
            _healthCheckTimer?.Stop();
            
            _logger.LogInformation("Service Administration Tool disposed");
        }
    }

    // Command implementations for MVVM
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (_isExecuting) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (_isExecuting) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute((T?)parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}