using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Order.API.Configuration;

/// <summary>
/// Configuration Swagger pour l'API Order
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configurer Swagger avec l'authentification JWT
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Configuration de base
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order API",
                Version = "v1",
                Description = "API de gestion des commandes avec Event Sourcing et CQRS",
                Contact = new OpenApiContact
                {
                    Name = "NiesPro Development Team",
                    Email = "dev@niespro.com",
                    Url = new Uri("https://niespro.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Configuration JWT Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // Documentation XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configuration des schémas
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            
            // Filtres personnalisés
            options.SchemaFilter<EnumSchemaFilter>();
            options.OperationFilter<DefaultResponseOperationFilter>();
        });

        return services;
    }

    /// <summary>
    /// Configurer l'interface Swagger
    /// </summary>
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment() || env.IsStaging())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
                options.RoutePrefix = "swagger";
                
                // Configuration de l'interface
                options.DocumentTitle = "Order API Documentation";
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
                options.EnableValidator();
                
                // Thème
                options.DefaultModelExpandDepth(2);
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                
                // Configuration OAuth (si nécessaire)
                options.OAuthClientId("order-api");
                options.OAuthAppName("Order API");
                options.OAuthUsePkce();
            });
        }

        return app;
    }
}

/// <summary>
/// Filtre pour améliorer la documentation des enums
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);
            
            for (int i = 0; i < enumNames.Length; i++)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString($"{enumValues.GetValue(i)} - {enumNames[i]}"));
            }
        }
    }
}

/// <summary>
/// Filtre pour ajouter des réponses par défaut
/// </summary>
public class DefaultResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ajouter les réponses communes si elles n'existent pas
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Validation error or invalid input"
            });
        }

        if (!operation.Responses.ContainsKey("401"))
        {
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized - Valid authentication required"
            });
        }

        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error"
            });
        }
    }
}