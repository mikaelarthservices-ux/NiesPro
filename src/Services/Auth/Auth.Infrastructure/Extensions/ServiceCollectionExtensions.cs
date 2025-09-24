using Auth.Infrastructure.Data;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using Auth.Application.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configuration de la base de données
        services.AddDbContext<AuthDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)))
                   .EnableSensitiveDataLogging(false) // Désactiver en production
                   .EnableDetailedErrors(false); // Désactiver en production
        });





        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Security Services - Use Auth-specific adapters
        services.AddScoped<IPasswordService, PasswordServiceAdapter>();
        services.AddScoped<IJwtService, JwtServiceAdapter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Validation Services
        services.AddScoped<IValidationService, ValidationService>();

        return services;
    }
}
