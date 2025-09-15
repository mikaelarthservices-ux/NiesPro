using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Application.Extensions;

/// <summary>
/// Extensions pour l'enregistrement des services de la couche Application
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Ajouter les services de la couche Application
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Validation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}