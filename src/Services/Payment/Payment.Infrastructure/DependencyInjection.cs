using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Repositories;
using Payment.Domain.Interfaces;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration de la base de données
        services.AddDbContext<PaymentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
        
        // Enregistrement des repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IThreeDSecureRepository, ThreeDSecureRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        
        return services;
    }
}