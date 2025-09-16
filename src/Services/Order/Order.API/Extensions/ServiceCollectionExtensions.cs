using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Data;
using Order.Infrastructure.EventStore;
using Order.Infrastructure.Repositories;
using Order.Domain.Repositories;
using Order.Application.Mappings;
using FluentValidation;
using System.Reflection;

namespace Order.API.Extensions;

/// <summary>
/// Extensions pour la configuration des services de l'application
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configurer Entity Framework Core avec SQL Server
    /// </summary>
    public static IServiceCollection AddOrderDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderDb") 
            ?? "Server=localhost;Database=NiesPro_OrderDb;Trusted_Connection=true;TrustServerCertificate=true;";

        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)),
                mysqlOptions =>
                {
                    mysqlOptions.CommandTimeout(30);
                    mysqlOptions.MigrationsAssembly("Order.Infrastructure");
                });

            // Configuration de performance pour la production
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching();
            options.EnableDetailedErrors(false);
        });

        return services;
    }

    /// <summary>
    /// Configurer MediatR avec tous les handlers
    /// </summary>
    public static IServiceCollection AddOrderMediatR(this IServiceCollection services)
    {
        // Enregistrer MediatR avec les assemblies des handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Order.Application.Commands.CreateOrderCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Order.Application.EventHandlers.OrderCreatedEventHandler).Assembly);
        });

        return services;
    }

    /// <summary>
    /// Configurer AutoMapper avec tous les profils
    /// </summary>
    public static IServiceCollection AddOrderAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(OrderMappingProfile));
        return services;
    }

    /// <summary>
    /// Configurer FluentValidation
    /// </summary>
    public static IServiceCollection AddOrderValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Order.Application.Validators.CreateOrderCommandValidator).Assembly);
        return services;
    }

    /// <summary>
    /// Configurer les repositories
    /// </summary>
    public static IServiceCollection AddOrderRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IEventStore, SqlEventStore>();

        return services;
    }

    /// <summary>
    /// Configurer Swagger avec documentation complète
    /// </summary>
    public static IServiceCollection AddOrderSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "NiesPro Order API",
                Version = "v1",
                Description = "API de gestion des commandes avec patterns CQRS et Event Sourcing",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "NiesPro Team",
                    Email = "support@niespro.com"
                }
            });

            // Inclure les commentaires XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configuration JWT pour Swagger
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configurer CORS
    /// </summary>
    public static IServiceCollection AddOrderCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "https://localhost:3001" };

        services.AddCors(options =>
        {
            options.AddPolicy("OrderCorsPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configurer l'authentification JWT
    /// </summary>
    public static IServiceCollection AddOrderAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-for-jwt-tokens-in-production-should-be-much-longer";

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = jwtSettings["Authority"] ?? "https://localhost:7001";
                options.RequireHttpsMetadata = false; // Pour le développement uniquement
                options.Audience = jwtSettings["Audience"] ?? "order-api";
                
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireOrderAccess", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "order-api");
            });
        });

        return services;
    }

    /// <summary>
    /// Configurer les services de santé (Health Checks)
    /// </summary>
    public static IServiceCollection AddOrderHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderDb") 
            ?? "Server=localhost;Database=NiesPro_OrderDb;Trusted_Connection=true;TrustServerCertificate=true;";

        services.AddHealthChecks()
            .AddCheck("order-api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Order API is healthy"));

        return services;
    }
}