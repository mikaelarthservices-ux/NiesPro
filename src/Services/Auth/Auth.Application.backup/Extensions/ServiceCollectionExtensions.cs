using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Auth.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Enregistrement de MediatR avec les handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Validation avec FluentValidation (si nécessaire)
        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}