using Order.API.Configuration;
using Order.API.Extensions;
using Order.API.Middleware;
using Order.Application.Extensions;
using Order.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ApplicationName", "Order.API")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Order API application");

    // Configuration des services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configuration des couches
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    // Configuration Swagger
    builder.Services.AddSwaggerConfiguration();

    // Configuration Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    var app = builder.Build();

    // Configuration du pipeline middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandling();
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // Middleware personnalisés
    app.UseRequestLogging();
    app.UseSerilogRequestLogging();

    // Configuration Swagger
    app.UseSwaggerConfiguration(app.Environment);

    // Configuration CORS
    app.UseCors("OrderApiPolicy");

    // Configuration de l'authentification
    app.UseAuthentication();
    app.UseAuthorization();

    // Health checks
    app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(x => new
                {
                    name = x.Key,
                    status = x.Value.Status.ToString(),
                    exception = x.Value.Exception?.Message,
                    duration = x.Value.Duration.ToString()
                })
            };
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    });

    // Mappage des contrôleurs
    app.MapControllers();

    // Point de départ simple
    app.MapGet("/", () => new
    {
        Service = "Order API",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Documentation = "/swagger"
    }).WithTags("Info").WithOpenApi();

    // Migration automatique en développement
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Order.Infrastructure.Data.OrderDbContext>();
        
        Log.Information("Applying database migrations...");
        await context.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }

    Log.Information("Order API configured successfully");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Order API application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}