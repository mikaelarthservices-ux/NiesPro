using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Catalog.Application.Common;

namespace Catalog.Application.Extensions
{
    /// <summary>
    /// Extension methods for configuring application services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Add FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Add validation behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            return services;
        }
    }
}