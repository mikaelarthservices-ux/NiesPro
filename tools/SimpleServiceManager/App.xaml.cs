using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NiesPro.ServiceAdmin.Models;
using NiesPro.ServiceAdmin.Services;
using NiesPro.ServiceAdmin.ViewModels;
using Serilog;
using System;
using System.IO;
using System.Windows;

namespace NiesPro.ServiceAdmin
{
    /// <summary>
    /// Professional WPF Application with dependency injection and structured logging
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private ILogger<App>? _logger;

        /// <summary>
        /// Application startup with professional logging and DI setup
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Setup Serilog configuration
                ConfigureSerilog();

                // Build the host with dependency injection
                _host = CreateHostBuilder().Build();

                // Get logger after host is built
                _logger = _host.Services.GetRequiredService<ILogger<App>>();
                _logger.LogInformation("NiesPro Service Admin application starting...");

                // Start the host
                await _host.StartAsync();

                // Get the main window from DI container
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                
                // Set as main window and show
                MainWindow = mainWindow;
                mainWindow.Show();

                _logger.LogInformation("Application started successfully");
            }
            catch (Exception ex)
            {
                // Log to file if possible, otherwise show error dialog
                var errorMessage = $"Critical error during application startup: {ex.Message}";
                
                try
                {
                    _logger?.LogCritical(ex, "Critical error during application startup");
                }
                catch
                {
                    // Fallback to file logging
                    var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                               "NiesPro", "ServiceAdmin", "error.log");
                    Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} CRITICAL: {errorMessage}\n{ex}\n\n");
                }

                MessageBox.Show(
                    errorMessage,
                    "Erreur Critique d'Initialisation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(1);
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// Configures Serilog for structured logging
        /// </summary>
        private void ConfigureSerilog()
        {
            var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                           "NiesPro", "ServiceAdmin", "logs");
            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "serviceadmin-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "errors-.log"),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
                .CreateLogger();
        }

        /// <summary>
        /// Creates the host builder with all required services
        /// </summary>
        private IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Register HttpClient
                    services.AddHttpClient<IMicroserviceManager, MicroserviceManager>(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        client.DefaultRequestHeaders.Add("User-Agent", "NiesPro-ServiceAdmin/1.0");
                    });

                    // Register core services
                    services.AddSingleton<IMicroserviceManager, MicroserviceManager>();
                    
                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();
                    
                    // Register Windows
                    services.AddSingleton<MainWindow>();

                    // Register logging
                    services.AddLogging(builder =>
                    {
                        builder.AddSerilog(dispose: true);
                    });
                });
        }

        /// <summary>
        /// Application exit with graceful shutdown
        /// </summary>
        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                _logger?.LogInformation("Application shutdown initiated");

                // Stop the host gracefully
                if (_host != null)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                    _host.Dispose();
                }

                _logger?.LogInformation("Application shutdown completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during application shutdown");
            }
            finally
            {
                // Ensure Serilog is flushed and closed
                Log.CloseAndFlush();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Global exception handler
        /// </summary>
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                _logger?.LogCritical(e.Exception, "Unhandled application exception");

                var result = MessageBox.Show(
                    $"Une erreur inattendue s'est produite:\n\n{e.Exception.Message}\n\nVoulez-vous continuer?",
                    "Erreur Critique",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                e.Handled = result == MessageBoxResult.Yes;

                if (result == MessageBoxResult.No)
                {
                    _logger?.LogInformation("User chose to exit after critical error");
                    Shutdown(1);
                }
            }
            catch (Exception logEx)
            {
                // Last resort - try to save error to file
                try
                {
                    var errorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                                                   "NiesPro_CriticalError.log");
                    File.AppendAllText(errorLogPath, 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} CRITICAL ERROR:\n{e.Exception}\n\nLOGGING ERROR:\n{logEx}\n\n");
                }
                catch
                {
                    // If even file logging fails, just mark as handled to prevent crash
                }
                
                e.Handled = true;
                Shutdown(1);
            }
        }

        /// <summary>
        /// Override to setup global exception handling
        /// </summary>
        public App()
        {
            // Subscribe to global exception handling
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Handles unhandled exceptions from non-UI threads
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                _logger?.LogCritical(exception, "Unhandled domain exception (IsTerminating: {IsTerminating})", e.IsTerminating);

                if (e.IsTerminating)
                {
                    MessageBox.Show(
                        $"Une erreur critique non récupérable s'est produite. L'application va se fermer.\n\nErreur: {exception?.Message}",
                        "Erreur Fatale",
                        MessageBoxButton.OK,
                        MessageBoxImage.Stop);
                }
            }
            catch
            {
                // Last resort - try to save to desktop
                try
                {
                    var errorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                                                   "NiesPro_FatalError.log");
                    File.AppendAllText(errorLogPath, 
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} FATAL ERROR:\n{e.ExceptionObject}\n\n");
                }
                catch { }
            }
        }
    }
}