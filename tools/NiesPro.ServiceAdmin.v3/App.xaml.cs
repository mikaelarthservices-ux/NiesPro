using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using NiesPro.ServiceAdmin.Core.Interfaces;
using NiesPro.ServiceAdmin.Services;
using NiesPro.ServiceAdmin.ViewModels;
using NiesPro.ServiceAdmin.Views;
using NiesPro.ServiceAdmin.v3.Services;

namespace NiesPro.ServiceAdmin;

/// <summary>
/// Professional WPF application for NiesPro Service Administration Platform v3.0
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Configure professional logging with Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/niespro-service-admin-.log", 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();

            // Build host with professional dependency injection
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            // Start main window
            // Create and show main window without DI for now
            var mainWindow = new MainWindow();
            mainWindow.Show();

            Log.Information("NiesPro Enterprise Service Administration Platform started successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "NiesPro Service Administration Platform failed to start");
            MessageBox.Show($"Critical startup error: {ex.Message}", 
                          "NiesPro Service Admin - Startup Error", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
            Environment.Exit(1);
        }

        base.OnStartup(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<INiesProServiceManager, NiesProServiceManager>();
        services.AddSingleton<INiesProHttpClient, NiesProHttpClient>();
        services.AddSingleton<INiesProProcessManager, NiesProProcessManager>();
        services.AddSingleton<ServiceLogCapture>();

        // HTTP client configuration
        services.AddHttpClient<NiesProHttpClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "NiesPro-ServiceAdmin/3.0");
        });

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views - MainWindow created manually for now
        // services.AddTransient<MainWindow>();

        // Logging
        services.AddLogging(builder => builder.AddSerilog());

        Log.Information("NiesPro Enterprise Administration Platform services configured with professional DI");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.Information("NiesPro Service Administration Platform v3.0 shutdown completed");
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error during NiesPro Service Administration Platform shutdown");
        }

        base.OnExit(e);
    }
}

