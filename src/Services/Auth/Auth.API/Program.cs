using Auth.API.Extensions;
using Auth.Application.Extensions;
using Auth.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Auth.API")
        .CreateLogger();

    Log.Information("Démarrage de l'application Auth.API");

    builder.Host.UseSerilog();

    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddAuthApplication();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    app.UseApiMiddleware();
    
    Log.Information("Application Auth.API démarrée avec succès");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erreur fatale lors du démarrage de l'application Auth.API");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
namespace Auth.API
{
    public partial class Program { }
}