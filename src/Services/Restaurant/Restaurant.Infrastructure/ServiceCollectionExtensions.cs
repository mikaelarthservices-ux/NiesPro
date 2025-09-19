using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Infrastructure.Data;
using Restaurant.Domain.Repositories;
using Restaurant.Infrastructure.Data;
using Restaurant.Infrastructure.Repositories;

namespace Restaurant.Infrastructure;

/// <summary>
/// Configuration des services d'infrastructure pour le module Restaurant
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajouter les services d'infrastructure Restaurant
    /// </summary>
    public static IServiceCollection AddRestaurantInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration de la base de données
        services.AddDbContext<RestaurantDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("RestaurantDatabase");
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(RestaurantDbContext).Assembly.FullName);
                b.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            // Configuration pour le développement
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Enregistrement du Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RestaurantDbContext>());

        // Enregistrement des repositories - Tables
        services.AddScoped<ITableRepository, TableRepository>();
        services.AddScoped<ITableReservationRepository, TableReservationRepository>();

        // Repositories - Menu
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();

        // Repositories - Kitchen Orders
        services.AddScoped<IKitchenOrderRepository, KitchenOrderRepository>();

        // Repositories - Staff
        services.AddScoped<IStaffRepository, StaffRepository>();

        // Services d'infrastructure supplémentaires
        services.AddRestaurantInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Ajouter les services d'infrastructure supplémentaires
    /// </summary>
    private static IServiceCollection AddRestaurantInfrastructureServices(
        this IServiceCollection services)
    {
        // Services de notification
        // services.AddScoped<INotificationService, NotificationService>();

        // Services de cache
        // services.AddScoped<ICacheService, CacheService>();

        // Services d'audit
        // services.AddScoped<IAuditService, AuditService>();

        // Services de reporting
        // services.AddScoped<IReportingService, ReportingService>();

        // Services d'intégration externe
        // services.AddScoped<IPaymentService, PaymentService>();
        // services.AddScoped<IInventoryIntegrationService, InventoryIntegrationService>();

        return services;
    }

    /// <summary>
    /// Ajouter les services de cache pour Restaurant
    /// </summary>
    public static IServiceCollection AddRestaurantCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration Redis si disponible
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "RestaurantService";
            });
        }
        else
        {
            // Fallback vers le cache mémoire
            services.AddMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Configurer les intercepteurs de base de données
    /// </summary>
    public static IServiceCollection AddRestaurantInterceptors(
        this IServiceCollection services)
    {
        // Intercepteur d'audit
        // services.AddScoped<AuditInterceptor>();

        // Intercepteur de performance
        // services.AddScoped<PerformanceInterceptor>();

        // Intercepteur de sécurité
        // services.AddScoped<SecurityInterceptor>();

        return services;
    }

    /// <summary>
    /// Ajouter les services de monitoring et logging
    /// </summary>
    public static IServiceCollection AddRestaurantMonitoring(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration Application Insights si disponible
        var appInsightsKey = configuration["ApplicationInsights:InstrumentationKey"];
        if (!string.IsNullOrWhiteSpace(appInsightsKey))
        {
            services.AddApplicationInsightsTelemetry(appInsightsKey);
        }

        // Métriques personnalisées
        // services.AddScoped<IMetricsService, MetricsService>();

        // Health checks
        services.AddHealthChecks()
            .AddDbContextCheck<RestaurantDbContext>("restaurant-database");

        return services;
    }
}

/// <summary>
/// Extensions pour la migration et l'initialisation de la base de données
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Migrer et initialiser la base de données Restaurant
    /// </summary>
    public static async Task MigrateRestaurantDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

        try
        {
            // Appliquer les migrations
            await context.Database.MigrateAsync(cancellationToken);

            // Initialiser les données de base si nécessaire
            await SeedBaseDataAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log l'erreur
            var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<RestaurantDbContext>>();
            logger?.LogError(ex, "An error occurred while migrating the Restaurant database");
            throw;
        }
    }

    /// <summary>
    /// Initialiser les données de base
    /// </summary>
    private static async Task SeedBaseDataAsync(
        RestaurantDbContext context,
        CancellationToken cancellationToken = default)
    {
        // Vérifier si des données existent déjà
        if (await context.Tables.AnyAsync(cancellationToken))
        {
            return; // Les données existent déjà
        }

        // Ici, vous pouvez ajouter des données de seed
        // Par exemple, des sections de restaurant par défaut, des rôles, etc.

        /*
        // Exemple de données de seed
        var defaultSections = new[]
        {
            new Section { Name = "Main Dining", Description = "Main dining area" },
            new Section { Name = "Private Dining", Description = "Private dining rooms" },
            new Section { Name = "Terrace", Description = "Outdoor terrace" }
        };

        context.Sections.AddRange(defaultSections);
        await context.SaveChangesAsync(cancellationToken);
        */
    }

    /// <summary>
    /// Vérifier la santé de la base de données
    /// </summary>
    public static async Task<bool> CheckDatabaseHealthAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();

            // Test simple de connexion
            return await context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}