# 🎯 TEMPLATE STANDARDISÉ - INTÉGRATION LOGS OBLIGATOIRE

*Template à appliquer OBLIGATOIREMENT à tous les microservices NiesPro*

---

## 📋 **CHECKLIST D'INTÉGRATION OBLIGATOIRE**

### ✅ **1. Ajout du Package NuGet**

**Modifier le .csproj du service :**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  
  <ItemGroup>
    <!-- PACKAGE OBLIGATOIRE -->
    <PackageReference Include="NiesPro.Logging.Client" Version="1.0.0" />
    
    <!-- Serilog OBLIGATOIRE -->
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.Http" Version="8.0.0" />
    <PackageReference Include="Serilog.Formatting.Json" Version="1.0.0" />
  </ItemGroup>

</Project>
```

### ✅ **2. Configuration appsettings.json**

**Ajouter OBLIGATOIREMENT dans appsettings.json :**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Http",
        "Args": {
          "requestUri": "https://localhost:5018/api/logs",
          "queueLimitBytes": null,
          "textFormatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "LogsService": {
    "BaseUrl": "https://localhost:5018",
    "ApiKey": "your-api-key-here",
    "ServiceName": "{{SERVICE_NAME}}", // <- Remplacer par Auth.API, Catalog.API, etc.
    "RetryAttempts": 3,
    "TimeoutSeconds": 30,
    "EnableHealthChecks": true
  }
}
```

### ✅ **3. Modification Program.cs OBLIGATOIRE**

**Remplacer INTÉGRALEMENT Program.cs :**
```csharp
using Serilog;
using NiesPro.Logging.Client;
using NiesPro.Logging.Client.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// SERILOG OBLIGATOIRE - Configuration centralisée
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// NIESPRO LOGGING OBLIGATOIRE - Clients centralisés
builder.Services.AddNiesProLogging(builder.Configuration);

// JWT Authentication (si nécessaire pour le service)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException()))
        };
    });

// Services spécifiques du microservice
// TODO: Ajouter ici vos propres services (DbContext, Repositories, etc.)

var app = builder.Build();

// MIDDLEWARE OBLIGATOIRES NIESPRO - Dans cet ordre exact !
app.UseNiesProCompleteLogging(); // <- OBLIGATOIRE EN PREMIER

// Middleware standards
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health checks OBLIGATOIRES
app.MapHealthChecks("/health");

app.MapControllers();

// LOG OBLIGATOIRE - Démarrage
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Service {{SERVICE_NAME}} démarré sur les ports configurés");

app.Run();
```

### ✅ **4. Modification des Controllers**

**Template OBLIGATOIRE pour tous les controllers :**
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiesPro.Logging.Client;

namespace {{SERVICE_NAMESPACE}}.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Si authentification requise
public class {{CONTROLLER_NAME}}Controller : ControllerBase
{
    private readonly ILogger<{{CONTROLLER_NAME}}Controller> _logger;
    private readonly ILogsServiceClient _logsClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly IMetricsServiceClient _metricsClient;
    private readonly IAlertServiceClient _alertClient;
    
    // Vos services métier
    private readonly I{{SERVICE_NAME}}Service _service;

    public {{CONTROLLER_NAME}}Controller(
        ILogger<{{CONTROLLER_NAME}}Controller> logger,
        ILogsServiceClient logsClient,
        IAuditServiceClient auditClient,
        IMetricsServiceClient metricsClient,
        IAlertServiceClient alertClient,
        I{{SERVICE_NAME}}Service service)
    {
        _logger = logger;
        _logsClient = logsClient;
        _auditClient = auditClient;
        _metricsClient = metricsClient;
        _alertClient = alertClient;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<{{ENTITY_NAME}}>>> GetAll()
    {
        using var timer = _metricsClient.StartTimer("get_all_{{entities}}");
        
        try
        {
            // LOG OBLIGATOIRE - Début opération
            await _logsClient.LogInformationAsync("Récupération de tous les {{entities}}", new Dictionary<string, object>
            {
                { "UserId", GetCurrentUserId() },
                { "Action", "GET_ALL_{{ENTITIES}}" }
            });

            var entities = await _service.GetAllAsync();
            
            // MÉTRIQUE OBLIGATOIRE
            await _metricsClient.RecordMetricAsync("{{entities}}_retrieved", entities.Count());
            
            return Ok(entities);
        }
        catch (Exception ex)
        {
            // LOG ERREUR OBLIGATOIRE
            await _logsClient.LogErrorAsync(ex, "Erreur récupération {{entities}}", new Dictionary<string, object>
            {
                { "UserId", GetCurrentUserId() }
            });
            
            // ALERTE CRITIQUE si nécessaire
            await _alertClient.CreateWarningAlertAsync(
                "Erreur récupération {{entities}}", 
                ex.Message,
                new Dictionary<string, object> { { "UserId", GetCurrentUserId() } });
            
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpPost]
    public async Task<ActionResult<{{ENTITY_NAME}}>> Create([FromBody] Create{{ENTITY_NAME}}Dto dto)
    {
        using var timer = _metricsClient.StartTimer("create_{{entity}}");
        
        try
        {
            var entity = await _service.CreateAsync(dto);
            
            // AUDIT OBLIGATOIRE - Création
            await _auditClient.AuditCreateAsync(
                GetCurrentUserId(),
                GetCurrentUserName(), 
                "{{ENTITY_NAME}}",
                entity.Id.ToString(),
                new Dictionary<string, object>
                {
                    { "EntityData", dto }
                });

            // LOG SUCCÈS OBLIGATOIRE
            await _logsClient.LogInformationAsync($"{{ENTITY_NAME}} créé avec succès: {entity.Id}", new Dictionary<string, object>
            {
                { "EntityId", entity.Id },
                { "UserId", GetCurrentUserId() }
            });

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
        catch (Exception ex)
        {
            // LOG ERREUR + ALERTE OBLIGATOIRES
            await _logsClient.LogErrorAsync(ex, "Erreur création {{entity}}", new Dictionary<string, object>
            {
                { "UserId", GetCurrentUserId() },
                { "EntityData", dto }
            });
            
            return StatusCode(500, "Erreur lors de la création");
        }
    }

    // Méthodes utilitaires OBLIGATOIRES
    private string GetCurrentUserId() => User?.Identity?.Name ?? "Anonymous";
    private string GetCurrentUserName() => User?.Identity?.Name ?? "Anonymous";
}
```

---

## 🚀 **SCRIPT D'AUTOMATISATION**

**PowerShell pour appliquer le template :**
```powershell
# Paramètres à définir pour chaque service
param(
    [string]$ServiceName = "Auth.API",  # Auth.API, Catalog.API, etc.
    [string]$ServicePath = "./src/Services/Auth/Auth.API"
)

# 1. Ajouter le package NuGet
dotnet add $ServicePath package NiesPro.Logging.Client
dotnet add $ServicePath package Serilog.Extensions.Hosting
dotnet add $ServicePath package Serilog.Sinks.Console  
dotnet add $ServicePath package Serilog.Sinks.Http
dotnet add $ServicePath package Serilog.Formatting.Json

# 2. Modifier appsettings.json
$appsettingsPath = "$ServicePath/appsettings.json"
# TODO: Script pour injecter la configuration LogsService

# 3. Modifier Program.cs
$programPath = "$ServicePath/Program.cs"
# TODO: Script pour remplacer Program.cs avec template

Write-Host "✅ Service $ServiceName configuré pour intégration Logs obligatoire"
```

---

## ⚠️ **RÈGLES DE COMPLIANCE OBLIGATOIRES**

### 🚫 **INTERDICTIONS FORMELLES**

1. **AUCUN service ne peut démarrer** sans client Logs configuré
2. **AUCUNE action CUD** sans audit automatique
3. **AUCUNE exception** sans log centralisé
4. **AUCUN endpoint critique** sans métriques
5. **AUCUN build** sans validation intégration Logs

### ✅ **VALIDATIONS AUTOMATIQUES**

1. **Pipeline CI/CD** vérifie présence NiesPro.Logging.Client
2. **SonarQube** détecte controllers sans logging
3. **Health checks** valident connectivité Logs service
4. **Tests d'intégration** testent flux complet

---

## 📊 **MONITORING DE COMPLIANCE**

**Dashboard obligatoire pour chaque service :**
- ✅ Logs reçus dans les 24h dernières
- ✅ Audits générés pour toutes actions CUD
- ✅ Métriques de performance collectées
- ✅ Alertes configurées et fonctionnelles

---

**🎯 RÉSULTAT : Écosystème NiesPro 100% traceable, monitoré et auditable !**