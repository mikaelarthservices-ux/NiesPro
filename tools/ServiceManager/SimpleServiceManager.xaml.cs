using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NiesPro.ServiceManager
{
    public partial class SimpleServiceManager : Window
    {
        private readonly List<ServiceInfo> _services;
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;
        private ServiceInfo? _selectedService;

        public SimpleServiceManager()
        {
            InitializeComponent();
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            
            _services = new List<ServiceInfo>
            {
                new ServiceInfo { Name = "Gateway", Port = 5000, Path = @"C:\Users\HP\Documents\projets\NiesPro\src\Gateway\Gateway.API" },
                new ServiceInfo { Name = "Auth", Port = 5001, Path = @"C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.API" },
                new ServiceInfo { Name = "Order", Port = 5002, Path = @"C:\Users\HP\Documents\projets\NiesPro\src\Services\Order\Order.API" },
                new ServiceInfo { Name = "Catalog", Port = 5003, Path = @"C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog\Catalog.API" }
            };

            ServicesListBox.ItemsSource = _services;
            LogServiceCombo.ItemsSource = _services;
            
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            UpdateUI();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var service in _services)
            {
                await CheckServiceStatus(service);
            }
            UpdateUI();
        }

        private async Task CheckServiceStatus(ServiceInfo service)
        {
            try
            {
                // Check if process is running
                if (service.ProcessId.HasValue)
                {
                    try
                    {
                        var process = Process.GetProcessById(service.ProcessId.Value);
                        if (process.HasExited)
                        {
                            service.IsRunning = false;
                            service.ProcessId = null;
                            service.AddLog($"Process {service.ProcessId} exited");
                        }
                    }
                    catch
                    {
                        service.IsRunning = false;
                        service.ProcessId = null;
                        service.AddLog("Process not found");
                    }
                }

                // Health check if supposed to be running
                if (service.IsRunning)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync($"https://localhost:{service.Port}/health");
                        service.IsHealthy = response.IsSuccessStatusCode;
                    }
                    catch
                    {
                        service.IsHealthy = false;
                    }
                }
                else
                {
                    service.IsHealthy = false;
                }
            }
            catch (Exception ex)
            {
                service.AddLog($"Status check error: {ex.Message}");
            }
        }

        private async void StartService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceInfo service)
            {
                await StartServiceAsync(service);
            }
        }

        private async Task StartServiceAsync(ServiceInfo service)
        {
            try
            {
                if (service.IsRunning)
                {
                    service.AddLog("Service is already running");
                    return;
                }

                if (!Directory.Exists(service.Path))
                {
                    service.AddLog($"ERROR: Path not found: {service.Path}");
                    return;
                }

                service.AddLog($"Starting {service.Name} on port {service.Port}...");
                service.AddLog($"Working directory: {service.Path}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --urls https://localhost:{service.Port}",
                    WorkingDirectory = service.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    service.ProcessId = process.Id;
                    service.IsRunning = true;
                    service.AddLog($"Process started with PID: {process.Id}");

                    // Redirect output
                    _ = Task.Run(async () =>
                    {
                        while (!process.StandardOutput.EndOfStream)
                        {
                            var line = await process.StandardOutput.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line))
                            {
                                service.AddLog($"OUT: {line}");
                            }
                        }
                    });

                    _ = Task.Run(async () =>
                    {
                        while (!process.StandardError.EndOfStream)
                        {
                            var line = await process.StandardError.ReadLineAsync();
                            if (!string.IsNullOrEmpty(line))
                            {
                                service.AddLog($"ERR: {line}");
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                service.AddLog($"Failed to start: {ex.Message}");
            }
        }

        private void StopService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceInfo service)
            {
                StopService(service);
            }
        }

        private void StopService(ServiceInfo service)
        {
            try
            {
                if (service.ProcessId.HasValue)
                {
                    try
                    {
                        var process = Process.GetProcessById(service.ProcessId.Value);
                        process.Kill();
                        service.AddLog($"Process {service.ProcessId} killed");
                    }
                    catch (Exception ex)
                    {
                        service.AddLog($"Error killing process: {ex.Message}");
                    }
                }

                service.IsRunning = false;
                service.ProcessId = null;
                service.IsHealthy = false;
            }
            catch (Exception ex)
            {
                service.AddLog($"Stop error: {ex.Message}");
            }
        }

        private async void StartAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var service in _services)
            {
                if (!service.IsRunning)
                {
                    await StartServiceAsync(service);
                    await Task.Delay(2000); // Stagger starts
                }
            }
        }

        private void StopAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var service in _services)
            {
                if (service.IsRunning)
                {
                    StopService(service);
                }
            }
        }

        private void Cleanup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kill all dotnet processes
                var processes = Process.GetProcessesByName("dotnet");
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }

                // Reset all services
                foreach (var service in _services)
                {
                    service.IsRunning = false;
                    service.ProcessId = null;
                    service.IsHealthy = false;
                    service.AddLog("Service reset by cleanup");
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cleanup error: {ex.Message}");
            }
        }

        private void LogServiceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogServiceCombo.SelectedItem is ServiceInfo service)
            {
                _selectedService = service;
                LogTextBox.Text = service.GetLogs();
                LogScrollViewer.ScrollToEnd();
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedService != null)
            {
                _selectedService._logs.Clear();
                _selectedService.AddLog("Logs cleared");
                LogTextBox.Text = _selectedService.GetLogs();
            }
        }

        private void OpenHealth_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceInfo service)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://localhost:{service.Port}/health",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening health endpoint: {ex.Message}");
                }
            }
        }

        private void OpenSwagger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServiceInfo service)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://localhost:{service.Port}/swagger",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening Swagger: {ex.Message}");
                }
            }
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                var runningCount = 0;
                var healthyCount = 0;

                foreach (var service in _services)
                {
                    if (service.IsRunning) runningCount++;
                    if (service.IsHealthy) healthyCount++;
                }

                StatusText.Text = $"Running: {runningCount}/4 | Healthy: {healthyCount}/4 | {DateTime.Now:HH:mm:ss}";

                // Update logs if service selected
                if (_selectedService != null)
                {
                    LogTextBox.Text = _selectedService.GetLogs();
                    LogScrollViewer.ScrollToEnd();
                }

                // Refresh the list
                ServicesListBox.Items.Refresh();
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _httpClient?.Dispose();
            base.OnClosed(e);
        }
    }

    public class ServiceInfo
    {
        public string Name { get; set; } = "";
        public int Port { get; set; }
        public string Path { get; set; } = "";
        public bool IsRunning { get; set; }
        public bool IsHealthy { get; set; }
        public int? ProcessId { get; set; }
        
        private readonly List<string> _logs = new();

        public void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logs.Add($"[{timestamp}] {message}");
            
            // Keep only last 100 lines
            if (_logs.Count > 100)
            {
                _logs.RemoveAt(0);
            }
        }

        public string GetLogs() => string.Join("\n", _logs);

        public string StatusText => IsRunning ? (IsHealthy ? "🟢 Running" : "🟡 Starting") : "⚫ Stopped";
        public string DisplayName => $"{Name} API (:{Port})";
    }
}