using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Payment.Infrastructure.Data;
using Payment.Application;
using Payment.Infrastructure;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Payment.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/payment-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration des services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configuration de la base de données
builder.Services.AddDbContext<PaymentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// Configuration de l'autorisation
builder.Services.AddAuthorization(options =>
{
    // Politiques de base
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MerchantAccess", policy => policy.RequireRole("Admin", "Merchant"));
    options.AddPolicy("FraudAnalystAccess", policy => policy.RequireRole("Admin", "FraudAnalyst"));
    options.AddPolicy("ComplianceOfficerAccess", policy => policy.RequireRole("Admin", "ComplianceOfficer"));
    
    // Politiques personnalisées
    options.AddPolicy("PaymentManagement", policy =>
        policy.RequireRole("Admin", "Merchant", "PaymentManager"));
    
    options.AddPolicy("TransactionAccess", policy =>
        policy.RequireRole("Admin", "Merchant", "Customer", "Analyst"));
    
    options.AddPolicy("ReportsAccess", policy =>
        policy.RequireRole("Admin", "Analyst", "FinanceManager", "Merchant"));

    // Politique par défaut
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Enregistrement des services métier
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PaymentApiCorsPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? 
                new[] { "https://localhost:3000", "https://localhost:5001" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(5));
    });
});

// Configuration de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "NiesPro Payment API",
        Version = "v1",
        Description = "API de paiement enterprise pour la plateforme NiesPro e-commerce",
        Contact = new OpenApiContact
        {
            Name = "NiesPro Support",
            Email = "support@niespro.com",
            Url = new Uri("https://niespro.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "Propriétaire",
            Url = new Uri("https://niespro.com/license")
        }
    });

    // Configuration JWT dans Swagger
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
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configuration des schémas
    c.SchemaFilter<EnumSchemaFilter>();
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});

// Configuration des Health Checks
builder.Services.AddHealthChecks()
    .AddDbContext<PaymentDbContext>()
    .AddCheck("Payment Processors", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("Fraud Detection Service", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddCheck("External APIs", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Configuration de la compression des réponses
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Configuration du cache
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configuration du pipeline de requêtes

// Gestion des erreurs globales
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Sécurité
app.UseHsts();
app.UseHttpsRedirection();

// Compression des réponses
app.UseResponseCompression();

// CORS
app.UseCors("PaymentApiCorsPolicy");

// Documentation Swagger (uniquement en développement et staging)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NiesPro Payment API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
    });
}

// Authentification et autorisation
app.UseAuthentication();
app.UseAuthorization();

// Cache de réponse
app.UseResponseCaching();

// Middleware de sécurité personnalisé
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Routes des contrôleurs
app.MapControllers();

// Health checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

// Endpoint de base
app.MapGet("/", () => new
{
    Service = "NiesPro Payment API",
    Version = "v1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Status = "Running"
});

// Endpoint d'information de version
app.MapGet("/version", () => new
{
    Version = "1.0.0",
    BuildDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    Environment = app.Environment.EnvironmentName,
    Framework = ".NET 8.0",
    Features = new[]
    {
        "PCI-DSS Compliance",
        "Multi-Provider Support",
        "Advanced Fraud Detection",
        "Real-time Analytics",
        "Event Sourcing",
        "Webhook Management"
    }
});

// Migration automatique de la base de données en développement
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    
    try
    {
        await context.Database.MigrateAsync();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during database migration");
    }
}

// Gestion gracieuse de l'arrêt
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Payment API is shutting down...");
});

lifetime.ApplicationStopped.Register(() =>
{
    Log.Information("Payment API has stopped");
    Log.CloseAndFlush();
});

try
{
    Log.Information("Starting NiesPro Payment API...");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Server URLs: {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? new[] { "Not configured" }));
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
