using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Windows;
using Serilog;
using NiesPro.AdminPanel.Services;
using NiesPro.AdminPanel.ViewModels;
using NiesPro.AdminPanel.Views;

namespace NiesPro.AdminPanel;

/// <summary>
/// Application principale avec configuration DI professionnelle
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Configuration Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/adminpanel-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // Configuration du host avec DI
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Chemin absolu vers le fichier de configuration
                    var appSettingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                    config.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .Build();

            // Démarrer les services
            _host.Start();

            // Afficher la fenêtre principale
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Erreur lors du démarrage: {ex.Message}", "Erreur", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configuration HTTP clients
        services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "NiesPro-AdminPanel/1.0");
        });

        // Services métier
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IMonitoringService, MonitoringService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AuthenticationViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<PaymentsViewModel>();

        // Views
        services.AddSingleton<MainWindow>();

        // Configuration
        services.Configure<Models.AppSettings>(configuration);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            // Arrêter explicitement le monitoring service avant la fermeture
            var monitoringService = _host?.Services.GetService<IMonitoringService>();
            if (monitoringService != null)
            {
                try
                {
                    monitoringService.StopMonitoringAsync().Wait(TimeSpan.FromSeconds(2));
                    monitoringService.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error stopping monitoring service");
                }
            }

            _host?.StopAsync(TimeSpan.FromSeconds(3)).Wait();
            _host?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error during application shutdown");
        }
        finally
        {
            Log.CloseAndFlush();
            
            // Force l'arrêt de tous les threads en arrière-plan
            Environment.Exit(e.ApplicationExitCode);
        }
    }
}