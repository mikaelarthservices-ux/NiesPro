using NUnit.Framework;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NiesPro.Logging.Client;
using System.Reflection;

namespace Order.Tests.Unit.Infrastructure;

/// <summary>
/// Tests d'intégration du logging centralisé NiesPro pour le service Order
/// Validation de l'alignement avec le service Logs centralisé
/// </summary>
[TestFixture]
public class LoggingIntegrationTests
{
    private ServiceCollection _services = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
        
        // Configuration minimale pour les tests
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LogsService:BaseUrl"] = "https://localhost:5018",
                ["LogsService:TimeoutSeconds"] = "30",
                ["LogsService:RetryCount"] = "3",
                ["LogsService:ServiceName"] = "Order.API",
                ["LogsService:ApiKey"] = "order-api-key-2024"
            });
            
        _configuration = configurationBuilder.Build();
    }

    [Test]
    [Description("Validation enregistrement des services de logging NiesPro")]
    public void Should_Register_NiesProLogging_Services_Successfully()
    {
        // Arrange & Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert - Vérification des interfaces principales
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
    [Description("Validation configuration du service Logs centralisé")]
    public void Should_Configure_LogsService_Settings_Correctly()
    {
        // Arrange & Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();
        var logsClient = serviceProvider.GetService<ILogsServiceClient>();

        // Assert
        logsClient.Should().NotBeNull();
        
        // Vérification de la configuration via la réflexion
        var clientType = logsClient!.GetType();
        var configField = clientType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.Name.Contains("config") || f.Name.Contains("settings"));
        
        if (configField != null)
        {
            var config = configField.GetValue(logsClient);
            config.Should().NotBeNull("Configuration du client doit être initialisée");
        }
    }

    [Test]
    [Description("Validation des méthodes d'interface ILogsServiceClient")]
    public void Should_Implement_ILogsServiceClient_Methods()
    {
        // Arrange & Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();
        var logsClient = serviceProvider.GetService<ILogsServiceClient>();

        // Assert
        logsClient.Should().NotBeNull();
        
        var clientType = logsClient!.GetType();
        
        // Vérification des méthodes essentielles
        clientType.GetMethod("LogAsync").Should().NotBeNull("Méthode LogAsync doit exister");
        clientType.GetMethod("LogInformationAsync").Should().NotBeNull("Méthode LogInformationAsync doit exister");
        clientType.GetMethod("LogErrorAsync").Should().NotBeNull("Méthode LogErrorAsync doit exister");
        clientType.GetMethod("LogWarningAsync").Should().NotBeNull("Méthode LogWarningAsync doit exister");
    }

    [Test]
    [Description("Validation des méthodes d'interface IAuditServiceClient")]
    public void Should_Implement_IAuditServiceClient_Methods()
    {
        // Arrange & Act
        _services.AddNiesProLogging(_configuration);
        var serviceProvider = _services.BuildServiceProvider();
        var auditClient = serviceProvider.GetService<IAuditServiceClient>();

        // Assert
        auditClient.Should().NotBeNull();
        
        var clientType = auditClient!.GetType();
        
        // Vérification des méthodes d'audit
        clientType.GetMethod("AuditAsync").Should().NotBeNull("Méthode AuditAsync doit exister");
    }

    [Test]
    [Description("Validation intégration avec ILogger<T> standard")]
    public void Should_Integrate_With_Standard_ILogger()
    {
        // Arrange
        _services.AddLogging(builder => builder.AddConsole());
        _services.AddNiesProLogging(_configuration);
        
        var serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        var logger = serviceProvider.GetService<ILogger<LoggingIntegrationTests>>();
        var logsClient = serviceProvider.GetService<ILogsServiceClient>();

        logger.Should().NotBeNull("ILogger<T> doit être disponible");
        logsClient.Should().NotBeNull("ILogsServiceClient doit être disponible");
        
        // Les deux systèmes peuvent coexister
        logger.Should().NotBeSameAs(logsClient, "Les services doivent être distincts mais complémentaires");
    }

    [TearDown]
    public void TearDown()
    {
        _services.Clear();
    }
}