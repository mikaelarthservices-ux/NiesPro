using Auth.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using FluentValidation;

namespace Auth.API.Extensions
{
    /// <summary>
    /// Dependency injection extensions for Auth.API
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Auth API services to the DI container
        /// </summary>
        public static IServiceCollection AddAuthApiServices(this IServiceCollection services)
        {
            // Add Application layer services
            services.AddAuthApplication();

            // Configure API behavior options
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // Let FluentValidation handle validation
            });

            // Add API versioning
            services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("X-Version"),
                    new MediaTypeApiVersionReader("ver")
                );
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:3000", "https://localhost:3000") // React/Angular dev servers
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Add health checks
            // Health Checks
            services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());
                // .AddDbContextCheck<Auth.Infrastructure.Data.AuthDbContext>("database"); // Commented until Infrastructure is ready

            // Add Authentication & Authorization
            services.AddAuthentication()
                .AddJwtBearer();
            services.AddAuthorization();

            // Add Swagger documentation
            services.AddSwaggerDocumentation();

            return services;
        }

        /// <summary>
        /// Add Swagger/OpenAPI documentation
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "NiesPro Auth API",
                    Version = "v1",
                    Description = "Authentication and Authorization microservice for NiesPro ERP system",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "NiesPro Development Team",
                        Email = "dev@niespro.com"
                    }
                });

                // Include XML comments
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
        /// Configure tous les services de l'API (alias pour AddAuthApiServices)
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddAuthApiServices();
        }
    }
}