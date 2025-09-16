using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace Gateway.API.Controllers;

/// <summary>
/// Contrôleur pour la supervision et les informations du Gateway API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GatewayController : ControllerBase
{
    private readonly ILogger<GatewayController> _logger;
    private readonly HealthCheckService _healthCheckService;

    public GatewayController(ILogger<GatewayController> logger, HealthCheckService healthCheckService)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Obtient les informations sur le Gateway et les microservices
    /// </summary>
    /// <returns>Informations du Gateway</returns>
    [HttpGet("info")]
    public IActionResult GetGatewayInfo()
    {
        var gatewayInfo = new
        {
            Name = "NiesPro Gateway API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            StartTime = Process.GetCurrentProcess().StartTime,
            Microservices = new
            {
                AuthAPI = new { Port = 5001, BaseUrl = "https://localhost:5001" },
                OrderAPI = new { Port = 5002, BaseUrl = "https://localhost:5002" },
                CatalogAPI = new { Port = 5003, BaseUrl = "https://localhost:5003" }
            },
            Features = new[]
            {
                "JWT Authentication",
                "Rate Limiting", 
                "CORS Support",
                "Health Checks",
                "Request Logging",
                "Swagger Documentation"
            }
        };

        _logger.LogInformation("Gateway info requested");
        return Ok(gatewayInfo);
    }

    /// <summary>
    /// Obtient le statut détaillé de tous les microservices
    /// </summary>
    /// <returns>Statut de santé complet</returns>
    [HttpGet("health/detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var result = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Description = entry.Value.Description,
                    Exception = entry.Value.Exception?.Message
                }).ToArray()
            };

            _logger.LogInformation("Detailed health check performed: {Status}", healthReport.Status);
            
            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(result) 
                : StatusCode(503, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing detailed health check");
            return StatusCode(500, new { Error = "Health check failed", Message = ex.Message });
        }
    }

    /// <summary>
    /// Obtient les métriques de performance du Gateway
    /// </summary>
    /// <returns>Métriques de performance</returns>
    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        var process = Process.GetCurrentProcess();
        
        var metrics = new
        {
            System = new
            {
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                OSVersion = Environment.OSVersion.ToString(),
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem
            },
            Process = new
            {
                Id = process.Id,
                ProcessName = process.ProcessName,
                StartTime = process.StartTime,
                WorkingSet = process.WorkingSet64,
                PrivateMemory = process.PrivateMemorySize64,
                TotalProcessorTime = process.TotalProcessorTime,
                ThreadCount = process.Threads.Count
            },
            Memory = new
            {
                WorkingSetMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                PrivateMemoryMB = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2)
            }
        };

        _logger.LogInformation("Gateway metrics requested");
        return Ok(metrics);
    }

    /// <summary>
    /// Obtient la configuration des routes Ocelot
    /// </summary>
    /// <returns>Configuration des routes</returns>
    [HttpGet("routes")]
    public IActionResult GetRoutes()
    {
        var routes = new[]
        {
            new { Path = "/api/auth/*", Target = "Auth.API (Port 5001)", Description = "Authentication and user management" },
            new { Path = "/api/orders/*", Target = "Order.API (Port 5002)", Description = "Order processing and management" },
            new { Path = "/api/products/*", Target = "Catalog.API (Port 5003)", Description = "Product catalog management" },
            new { Path = "/api/categories/*", Target = "Catalog.API (Port 5003)", Description = "Category management" },
            new { Path = "/api/brands/*", Target = "Catalog.API (Port 5003)", Description = "Brand management" }
        };

        _logger.LogInformation("Gateway routes configuration requested");
        return Ok(new { Routes = routes, TotalRoutes = routes.Length });
    }
}