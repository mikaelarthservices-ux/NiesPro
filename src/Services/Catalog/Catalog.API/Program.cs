using Catalog.Application.Extensions;
using Catalog.Infrastructure.Extensions;
using Catalog.API.Middleware;
using NiesPro.Logging.Client;
using NiesPro.Logging.Client.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Catalog.API")
        .CreateLogger();

    Log.Information("Démarrage de l'application Catalog.API");

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddApplicationServices();

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configuration du service de logging centralisé NiesPro OBLIGATOIRE
builder.Services.AddNiesProLogging(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Ajout du middleware de logging NiesPro OBLIGATOIRE (en premier pour capturer toutes les requêtes)
app.UseNiesProLogging();

// Global exception handling (must be first)
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Map Health Checks
app.MapHealthChecks("/health");

    Log.Information("Application Catalog.API démarrée avec succès");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erreur fatale lors du démarrage de l'application Catalog.API");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }

/// <summary>
/// Program class for testing accessibility
/// </summary>
public partial class Program { }