using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Auth.Application.Common.Behaviors;

namespace Auth.Application.Extensions
{
    /// <summary>
    /// Dependency injection extensions for Auth.Application layer
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Auth.Application services to the DI container
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddAuthApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Add MediatR with all handlers
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
            });

            // Add FluentValidation validators manually (AddValidatorsFromAssembly needs FluentValidation.DependencyInjection)
            services.AddScoped<IValidator<Features.Authentication.Commands.Login.LoginCommand>, 
                Features.Authentication.Commands.Login.LoginCommandValidator>();
            services.AddScoped<IValidator<Features.Users.Commands.RegisterUser.RegisterUserCommand>, 
                Features.Users.Commands.RegisterUser.RegisterUserCommandValidator>();
            services.AddScoped<IValidator<Features.Users.Commands.ChangePassword.ChangePasswordCommand>, 
                Features.Users.Commands.ChangePassword.ChangePasswordCommandValidator>();
            services.AddScoped<IValidator<Features.Users.Queries.GetUserProfile.GetUserProfileQuery>, 
                Features.Users.Queries.GetUserProfile.GetUserProfileQueryValidator>();
            services.AddScoped<IValidator<Features.Users.Queries.GetAllUsers.GetAllUsersQuery>, 
                Features.Users.Queries.GetAllUsers.GetAllUsersQueryValidator>();

            // Configure validation options (removed ApiBehaviorOptions as it's Web API specific)
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

            // Add application-specific services
            services.AddApplicationServices();

            return services;
        }

        /// <summary>
        /// Add application-specific services
        /// </summary>
        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add mapping profiles if using AutoMapper
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Add application-specific services here
            // services.AddScoped<IEmailService, EmailService>();
            // services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
