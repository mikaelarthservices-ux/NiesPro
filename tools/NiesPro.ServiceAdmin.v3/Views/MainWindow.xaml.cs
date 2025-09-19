using System.Windows;using System.Windows;



namespace NiesPro.ServiceAdmin.Views;namespace NiesPro.ServiceAdmin.Views;



public partial class MainWindow : Windowpublic partial class MainWindow : Window

{{

    public MainWindow()    public MainWindow()

    {    {

        InitializeComponent();        InitializeComponent();

        Title = "NiesPro Service Administration Platform v3.0";        Title = "NiesPro Service Administration Platform v3.0";

    }    }

}}

            // Setup UI
            ServicesItemsControl.ItemsSource = _services;
            LogServiceComboBox.ItemsSource = _services;
            LogServiceComboBox.DisplayMemberPath = "DisplayName";
            LogServiceComboBox.SelectedValuePath = "Name";

            // Setup timer for updates
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            UpdateUI();
        }

        private string FindNiesProBasePath()
        {
            // Try to find NiesPro root directory
            var currentDir = Directory.GetCurrentDirectory();
            
            // Look for the projects folder structure
            var searchPaths = new[]
            {
                @"C:\Users\HP\Documents\projets\NiesPro",
                Path.Combine(currentDir, "..", "..", ".."),
                Path.Combine(currentDir, "..", "..")
            };

            foreach (var path in searchPaths)
            {
                if (Directory.Exists(path) && Directory.Exists(Path.Combine(path, "src")))
                {
                    return Path.GetFullPath(path);
                }
            }

            // Default fallback
            return @"C:\Users\HP\Documents\projets\NiesPro";
        }

        private async void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            await UpdateServiceStatuses();
            UpdateUI();
        }

        private async Task UpdateServiceStatuses()
        {
            foreach (var service in _services)
            {
                // Check if process is still running
                if (service.ProcessId.HasValue)
                {
                    try
                    {
                        var process = Process.GetProcessById(service.ProcessId.Value);
                        if (process.HasExited)
                        {
                            service.Status = ServiceStatus.Stopped;
                            service.ProcessId = null;
                            service.IsHealthy = false;
                        }
                        else if (service.Status == ServiceStatus.Starting)
                        {
                            // Check if it's been starting for more than 30 seconds
                            if (service.LastStarted.HasValue && 
                                DateTime.Now - service.LastStarted.Value > TimeSpan.FromSeconds(30))
                            {
                                // Try health check to confirm it's really running
                                var healthCheck = await _serviceController.CheckHealthAsync(service);
                                if (healthCheck)
                                {
                                    service.Status = ServiceStatus.Running;
                                }
                                else
                                {
                                    service.Status = ServiceStatus.Error;
                                }
                            }
                        }
                    }
                    catch
                    {
                        service.Status = ServiceStatus.Stopped;
                        service.ProcessId = null;
                        service.IsHealthy = false;
                    }
                }

                // Health check for running services
                if (service.Status == ServiceStatus.Running)
                {
                    await _serviceController.CheckHealthAsync(service);
                }
                else
                {
                    service.IsHealthy = false;
                }
            }
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                // Update overall status
                var runningServices = _services.Count(s => s.Status == ServiceStatus.Running);
                ActiveServicesText.Text = $"{runningServices}/{_services.Count}";
                LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");

                if (runningServices == _services.Count)
                {
                    OverallStatusText.Text = "Tous services actifs";
                    OverallStatusText.Foreground = Brushes.Green;
                }
                else if (runningServices > 0)
                {
                    OverallStatusText.Text = "Partiellement actif";
                    OverallStatusText.Foreground = Brushes.Orange;
                }
                else
                {
                    OverallStatusText.Text = "Tous services arrêtés";
                    OverallStatusText.Foreground = Brushes.Red;
                }

                // Update logs if a service is selected
                if (_selectedLogService != null)
                {
                    var currentService = _services.FirstOrDefault(s => s.Name == _selectedLogService.Name);
                    if (currentService != null)
                    {
                        // Always update logs, even if empty to show "no logs yet"
                        if (string.IsNullOrEmpty(currentService.LogOutput))
                        {
                            LogTextBox.Text = $"En attente de logs pour {currentService.DisplayName}...\nSélectionnez un service et cliquez sur Start pour voir les logs.";
                        }
                        else
                        {
                            LogTextBox.Text = currentService.LogOutput;
                            // Auto-scroll to bottom
                            LogScrollViewer.ScrollToEnd();
                        }
                    }
                }

                // Force refresh of items control to update status colors
                ServicesItemsControl.Items.Refresh();
            });
        }

        private async void StartService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serviceName)
            {
                var service = _services.FirstOrDefault(s => s.Name == serviceName);
                if (service != null)
                {
                    button.IsEnabled = false;
                    button.Content = "⏳ Démarrage...";
                    
                    var success = await _serviceController.StartServiceAsync(service);
                    
                    button.IsEnabled = true;
                    button.Content = "▶️ Start";
                    
                    if (!success)
                    {
                        MessageBox.Show($"Échec du démarrage de {service.DisplayName}", 
                                      "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void StopService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string serviceName)
            {
                var service = _services.FirstOrDefault(s => s.Name == serviceName);
                if (service != null)
                {
                    button.IsEnabled = false;
                    button.Content = "⏳ Arrêt...";
                    
                    await _serviceController.StopServiceAsync(service);
                    
                    button.IsEnabled = true;
                    button.Content = "⏹️ Stop";
                }
            }
        }

        private async void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            StartAllButton.IsEnabled = false;
            StartAllButton.Content = "⏳ Démarrage...";

            var tasks = _services.Where(s => s.Status == ServiceStatus.Stopped)
                               .Select(s => _serviceController.StartServiceAsync(s));
            
            await Task.WhenAll(tasks);

            StartAllButton.IsEnabled = true;
            StartAllButton.Content = "🚀 Démarrer Tout";
        }

        private async void StopAllButton_Click(object sender, RoutedEventArgs e)
        {
            StopAllButton.IsEnabled = false;
            StopAllButton.Content = "⏳ Arrêt...";

            await _serviceController.StopAllServicesAsync(_services);

            StopAllButton.IsEnabled = true;
            StopAllButton.Content = "🛑 Arrêter Tout";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private async void CleanupButton_Click(object sender, RoutedEventArgs e)
        {
            CleanupButton.IsEnabled = false;
            CleanupButton.Content = "⏳ Nettoyage...";

            // Kill all dotnet processes
            try
            {
                var processes = Process.GetProcessesByName("dotnet");
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                    catch { }
                }

                // Reset all service states
                foreach (var service in _services)
                {
                    service.Status = ServiceStatus.Stopped;
                    service.ProcessId = null;
                    service.IsHealthy = false;
                    service.LogOutput += $"[{DateTime.Now:HH:mm:ss}] Service reset by cleanup\n";
                }

                await Task.Delay(2000);
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du nettoyage: {ex.Message}", 
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CleanupButton.IsEnabled = true;
            CleanupButton.Content = "🧹 Nettoyer";
        }

        private void OpenHealth_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir l'URL: {ex.Message}", 
                                  "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenSwagger_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir Swagger: {ex.Message}", 
                                  "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogServiceComboBox.SelectedItem is MicroserviceInfo selectedService)
            {
                _selectedLogService = selectedService;
                
                // Initialize logs display
                if (string.IsNullOrEmpty(selectedService.LogOutput))
                {
                    LogTextBox.Text = $"=== {selectedService.DisplayName} ===\nEn attente de démarrage...\nCliquez sur 'Start' pour lancer le service.";
                }
                else
                {
                    LogTextBox.Text = selectedService.LogOutput;
                }
                LogScrollViewer.ScrollToEnd();
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLogService != null)
            {
                _selectedLogService.LogOutput = string.Empty;
                LogTextBox.Text = "Logs effacés...";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            _serviceController?.Dispose();
            base.OnClosed(e);
        }
    }
}