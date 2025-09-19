using Microsoft.OpenApi.Models;
using System.Reflection;
using Restaurant.Application;
using Restaurant.Infrastructure;
using BuildingBlocks.API.Middleware;
using BuildingBlocks.API.Extensions;
using BuildingBlocks.Security.JWT;
using Serilog;

namespace Restaurant.API;

/// <summary>
/// Point d'entrée et configuration de l'API Restaurant
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting Restaurant API");
            
            var builder = WebApplication.CreateBuilder(args);

            // Configuration de Serilog
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            // Configuration des services
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configuration du pipeline
            await ConfigurePipeline(app);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Restaurant API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Couches applicatives
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        // Configuration API
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });

        // Configuration CORS
        services.AddCors(options =>
        {
            options.AddPolicy("RestaurantPolicy", policy =>
            {
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Configuration Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Restaurant Management API",
                Version = "v1",
                Description = "API complète pour la gestion des opérations du restaurant dans le système ERP NiesPro",
                Contact = new OpenApiContact
                {
                    Name = "NiesPro Development Team",
                    Email = "dev@niespro.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License"
                }
            });

            // Configuration de l'authentification JWT dans Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Inclusion des commentaires XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Tags personnalisés pour grouper les endpoints
            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var controllerName = api.ActionDescriptor.RouteValues["controller"];
                return new[] { controllerName ?? "Default" };
            });
        });

        // Configuration de l'authentification JWT
        services.AddJwtAuthentication(configuration);

        // Configuration de l'autorisation
        services.AddAuthorization(options =>
        {
            // Politiques d'autorisation pour le restaurant
            options.AddPolicy("RestaurantStaff", policy =>
                policy.RequireRole("Waiter", "Cook", "Chef", "Manager", "Admin"));

            options.AddPolicy("KitchenStaff", policy =>
                policy.RequireRole("Cook", "Chef", "Manager", "Admin"));

            options.AddPolicy("Management", policy =>
                policy.RequireRole("Manager", "Admin"));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("HROperations", policy =>
                policy.RequireRole("HR", "Manager", "Admin"));
        });

        // Configuration de la validation des modèles
        services.AddFluentValidationConfiguration();

        // Configuration du cache
        services.AddMemoryCache();
        
        // Configuration Redis si disponible
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }

        // Configuration de la compression des réponses
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Configuration du rate limiting
        services.AddRateLimiting(configuration);

        // Configuration de Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<Restaurant.Infrastructure.Persistence.RestaurantDbContext>()
            .AddCheck("restaurant-api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Restaurant API is running"));

        // Configuration des métriques et observabilité
        services.AddApplicationInsightsTelemetry(configuration);
    }

    private static async Task ConfigurePipeline(WebApplication app)
    {
        // Middleware de gestion des erreurs
        app.UseGlobalExceptionHandler();

        // Middleware de sécurité
        if (app.Environment.IsProduction())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Configuration Swagger en développement
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API V1");
                c.RoutePrefix = "swagger";
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
            });
        }

        // Middleware de compression
        app.UseResponseCompression();

        // Middleware de journalisation des requêtes
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                    diagnosticContext.Set("UserRole", httpContext.User.FindFirst("role")?.Value);
                }
            };
        });

        // Middleware CORS
        app.UseCors("RestaurantPolicy");

        // Middleware de rate limiting
        app.UseRateLimiter();

        // Middleware d'authentification et autorisation
        app.UseAuthentication();
        app.UseAuthorization();

        // Middleware de cache
        app.UseResponseCaching();

        // Configuration des routes
        app.MapControllers();

        // Configuration Health Checks
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration.TotalMilliseconds
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                });
                await context.Response.WriteAsync(result);
            }
        });

        // Endpoint d'informations sur l'API
        app.MapGet("/", () => new
        {
            service = "Restaurant Management API",
            version = "1.0.0",
            environment = app.Environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            endpoints = new
            {
                swagger = "/swagger",
                health = "/health",
                tables = "/api/v1/tables",
                reservations = "/api/v1/reservations",
                menus = "/api/v1/menus",
                menuItems = "/api/v1/menuItems",
                kitchenOrders = "/api/v1/kitchenOrders",
                staff = "/api/v1/staff"
            }
        });

        // Migration automatique de la base de données en développement
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Restaurant.Infrastructure.Persistence.RestaurantDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        Log.Information("Restaurant API pipeline configured successfully");
    }
}

/// <summary>
/// Extensions pour la configuration des services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configuration de FluentValidation
    /// </summary>
    public static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
    {
        services.AddFluentValidation(config =>
        {
            config.RegisterValidatorsFromAssembly(typeof(Restaurant.Application.AssemblyReference).Assembly);
            config.DisableDataAnnotationsValidation = true;
            config.ImplicitlyValidateChildProperties = true;
        });

        return services;
    }

    /// <summary>
    /// Configuration du rate limiting
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Politique générale
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Politique pour les opérations critiques
            options.AddPolicy("CriticalOperations", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
            };
        });

        return services;
    }
}