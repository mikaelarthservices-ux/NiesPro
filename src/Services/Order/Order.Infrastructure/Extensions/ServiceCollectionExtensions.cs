using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;
using Order.Infrastructure.EventStore;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services de la couche Infrastructure
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajouter les services de la couche Infrastructure
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Base de données
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName));
            
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching();
        });

        // Event Store
        services.AddScoped<IEventStore, SqlEventStore>();

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}