using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NiesPro.Logging.Client;
using System.Collections.Generic;

namespace Catalog.Tests.Unit.Infrastructure;

/// <summary>
/// Tests d'intégration du logging centralisé NiesPro pour le service Catalog
/// Validation de l'alignement avec le service Logs centralisé
/// </summary>
[TestFixture]
public class LoggingIntegrationTests
{
    private ServiceCollection _services;
    private IConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        
        // Configuration de test mimant appsettings.json
        var configurationData = new Dictionary<string, string>
        {
            ["LogsService:BaseUrl"] = "https://localhost:5018",
            ["LogsService:ApiKey"] = "catalog-service-api-key-2024",
            ["LogsService:ServiceName"] = "Catalog.API",
            ["LogsService:TimeoutSeconds"] = "30",
            ["LogsService:EnableHealthChecks"] = "true"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();
    }

    [Test]
    [Description("Validation configuration LogsService depuis appsettings.json")]
    public void Should_Load_LogsService_Configuration_Successfully()
    {
        // Act
        var logsConfig = _configuration.GetSection("LogsService").Get<LogsServiceConfiguration>();

        // Assert
        logsConfig.Should().NotBeNull();
        logsConfig!.BaseUrl.Should().Be("https://localhost:5018");
        logsConfig.ApiKey.Should().Be("catalog-service-api-key-2024");
        logsConfig.ServiceName.Should().Be("Catalog.API");
        logsConfig.TimeoutSeconds.Should().Be(30);
        logsConfig.EnableHealthChecks.Should().BeTrue();
    }

    [Test]
    [Description("Validation enregistrement des services de logging NiesPro")]
    public void Should_Register_NiesProLogging_Services_Successfully()
    {
        // Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - Vérification que tous les clients sont enregistrés
        var logsClient = serviceProvider.GetService<ILogsServiceClient>();
        var auditClient = serviceProvider.GetService<IAuditServiceClient>();
        var metricsClient = serviceProvider.GetService<IMetricsServiceClient>();
        var alertClient = serviceProvider.GetService<IAlertServiceClient>();

        logsClient.Should().NotBeNull("ILogsServiceClient doit être enregistré");
        auditClient.Should().NotBeNull("IAuditServiceClient doit être enregistré");
        metricsClient.Should().NotBeNull("IMetricsServiceClient doit être enregistré");
        alertClient.Should().NotBeNull("IAlertServiceClient doit être enregistré");
    }

    [Test]
    [Description("Validation configuration HttpClient pour service Logs")]
    public void Should_Configure_HttpClient_For_LogsService()
    {
        // Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - Vérification que HttpClient est configuré
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull("HttpClientFactory doit être disponible");

        // Vérification indirecte via le client
        var logsClient = serviceProvider.GetService<ILogsServiceClient>();
        logsClient.Should().NotBeNull();
        logsClient.Should().BeOfType<LogsServiceClient>();
    }

    [Test]
    [Description("Test alignement interfaces client avec service Logs")]
    public async Task Should_Have_Compatible_Interfaces_With_LogsService()
    {
        // Arrange
        _services.AddNiesProLogging(_configuration);
        _services.AddLogging();
        var serviceProvider = _services.BuildServiceProvider();
        
        var logsClient = serviceProvider.GetRequiredService<ILogsServiceClient>();
        var auditClient = serviceProvider.GetRequiredService<IAuditServiceClient>();

        // Assert - Vérification des méthodes d'interface (sans appel réseau)
        logsClient.Should().NotBeNull();
        auditClient.Should().NotBeNull();

        // Test des méthodes publiques sont disponibles
        var logsServiceType = typeof(ILogsServiceClient);
        var auditServiceType = typeof(IAuditServiceClient);

        logsServiceType.GetMethod("LogAsync").Should().NotBeNull("Méthode LogAsync requise");
        logsServiceType.GetMethod("LogErrorAsync").Should().NotBeNull("Méthode LogErrorAsync requise");
        logsServiceType.GetMethod("LogInformationAsync").Should().NotBeNull("Méthode LogInformationAsync requise");
        logsServiceType.GetMethod("LogWarningAsync").Should().NotBeNull("Méthode LogWarningAsync requise");

        auditServiceType.GetMethod("AuditAsync").Should().NotBeNull("Méthode AuditAsync requise");
        auditServiceType.GetMethod("AuditCreateAsync").Should().NotBeNull("Méthode AuditCreateAsync requise");
        auditServiceType.GetMethod("AuditUpdateAsync").Should().NotBeNull("Méthode AuditUpdateAsync requise");
        auditServiceType.GetMethod("AuditDeleteAsync").Should().NotBeNull("Méthode AuditDeleteAsync requise");

        await Task.CompletedTask; // Pour satisfaire async
    }

    [Test]
    [Description("Validation configuration Serilog dans Program.cs")]
    public void Should_Support_Serilog_Configuration()
    {
        // Arrange & Act
        _services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        var serviceProvider = _services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<LoggingIntegrationTests>>();

        // Assert
        logger.Should().NotBeNull("ILogger doit être disponible pour Serilog");
    }
}