using Microsoft.EntityFrameworkCore;
using Customer.Infrastructure;
using Customer.Infrastructure.Repositories;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Common;
using MediatR;
using System.Reflection;
using Customer.Application.Handlers;

namespace Customer.API.Extensions
{
    /// <summary>
    /// Service collection extensions for Customer API
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Customer services to the DI container
        /// </summary>
        public static IServiceCollection AddCustomerServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            var connectionString = configuration.GetConnectionString("CustomerConnection");
            services.AddDbContext<CustomerContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 28)),
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Add repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
            services.AddScoped<ICustomerPreferenceRepository, CustomerPreferenceRepository>();

            // Add MediatR
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommandHandler).Assembly);
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // Add AutoMapper if needed
            // services.AddAutoMapper(typeof(CustomerMappingProfile));

            return services;
        }

        /// <summary>
        /// Add Customer infrastructure services
        /// </summary>
        public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add health checks
            services.AddHealthChecks();

            // Add logging
            services.AddLogging();

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CustomerPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }

        /// <summary>
        /// Configure Customer application
        /// </summary>
        public static WebApplication ConfigureCustomerApp(this WebApplication app)
        {
            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("CustomerPolicy");
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Add health check endpoint
            app.MapHealthChecks("/health");

            return app;
        }

        /// <summary>
        /// Ensure database is created and migrated
        /// </summary>
        public static async Task<WebApplication> EnsureDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CustomerContext>();
            
            try
            {
                await context.Database.EnsureCreatedAsync();
                
                // Optionally run migrations
                // await context.Database.MigrateAsync();
                
                app.Logger.LogInformation("Customer database ensured successfully");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred while ensuring the Customer database");
                throw;
            }

            return app;
        }
    }
}