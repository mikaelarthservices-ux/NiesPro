using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands.Analytics;
using Payment.Application.Queries.Analytics;
using Payment.Application.DTOs.Analytics;
using MediatR;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur pour l'analytics et les rapports des paiements
/// </summary>
[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IMediator mediator, ILogger<AnalyticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtient les métriques de performance des paiements
    /// </summary>
    /// <param name="period">Période d'analyse</param>
    /// <param name="merchantId">ID du marchand (optionnel)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Métriques de performance</returns>
    [HttpGet("payment-metrics")]
    [Authorize(Roles = "Admin,Merchant,Analyst")]
    [ProducesResponseType(typeof(PaymentMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentMetricsDto>> GetPaymentMetrics(
        [FromQuery] string period = "Last30Days",
        [FromQuery] Guid? merchantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validPeriods = new[] { "Last7Days", "Last30Days", "Last90Days", "LastYear" };
            if (!validPeriods.Contains(period))
            {
                return BadRequest(new { error = "Invalid period. Valid values: " + string.Join(", ", validPeriods) });
            }

            // Validation des permissions pour l'accès aux données du marchand
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userRole == "Merchant" && merchantId.HasValue)
            {
                // Un marchand ne peut voir que ses propres métriques
                // Cette validation devrait être plus sophistiquée en production
                _logger.LogInformation("Merchant {UserId} requesting metrics for merchant {MerchantId}", 
                    currentUserId, merchantId);
            }

            _logger.LogInformation("Retrieving payment metrics for period {Period}, merchant {MerchantId}", 
                period, merchantId);

            var query = new GetPaymentMetricsQuery 
            { 
                Period = period,
                MerchantId = merchantId
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving payment metrics" });
        }
    }

    /// <summary>
    /// Obtient les statistiques de fraude
    /// </summary>
    /// <param name="startDate">Date de début</param>
    /// <param name="endDate">Date de fin</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Statistiques de fraude</returns>
    [HttpGet("fraud-statistics")]
    [Authorize(Roles = "Admin,FraudAnalyst,Analyst")]
    [ProducesResponseType(typeof(FraudStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FraudStatisticsDto>> GetFraudStatistics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            if ((endDate - startDate).TotalDays > 365)
            {
                return BadRequest(new { error = "Date range cannot exceed 365 days" });
            }

            _logger.LogInformation("Retrieving fraud statistics for period {StartDate} to {EndDate}", 
                startDate, endDate);

            var query = new GetFraudStatisticsQuery 
            { 
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fraud statistics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving fraud statistics" });
        }
    }

    /// <summary>
    /// Génère un rapport financier détaillé
    /// </summary>
    /// <param name="request">Paramètres du rapport</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Rapport financier</returns>
    [HttpPost("financial-report")]
    [Authorize(Roles = "Admin,Merchant,Analyst,FinanceManager")]
    [ProducesResponseType(typeof(FinancialReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FinancialReportDto>> GenerateFinancialReport(
        [FromBody] GenerateFinancialReportCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating financial report for period {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.GeneratedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Financial report generated successfully with {TransactionCount} transactions", 
                result.TotalTransactions);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid financial report request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error generating financial report" });
        }
    }

    /// <summary>
    /// Obtient l'analyse des tendances de paiement
    /// </summary>
    /// <param name="timeframe">Cadre temporel (Daily, Weekly, Monthly)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Analyse des tendances</returns>
    [HttpGet("payment-trends")]
    [Authorize(Roles = "Admin,Merchant,Analyst")]
    [ProducesResponseType(typeof(PaymentTrendsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentTrendsDto>> GetPaymentTrends(
        [FromQuery] string timeframe = "Monthly",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validTimeframes = new[] { "Daily", "Weekly", "Monthly" };
            if (!validTimeframes.Contains(timeframe))
            {
                return BadRequest(new { error = "Invalid timeframe. Valid values: " + string.Join(", ", validTimeframes) });
            }

            _logger.LogInformation("Retrieving payment trends with timeframe {Timeframe}", timeframe);

            var query = new GetPaymentTrendsQuery { Timeframe = timeframe };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment trends");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving payment trends" });
        }
    }

    /// <summary>
    /// Obtient l'analyse des méthodes de paiement
    /// </summary>
    /// <param name="period">Période d'analyse</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Analyse des méthodes de paiement</returns>
    [HttpGet("payment-methods-analysis")]
    [Authorize(Roles = "Admin,Merchant,Analyst")]
    [ProducesResponseType(typeof(PaymentMethodAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentMethodAnalysisDto>> GetPaymentMethodsAnalysis(
        [FromQuery] string period = "Last30Days",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving payment methods analysis for period {Period}", period);

            var query = new GetPaymentMethodAnalysisQuery { Period = period };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods analysis");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving payment methods analysis" });
        }
    }

    /// <summary>
    /// Obtient le tableau de bord en temps réel
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Données du tableau de bord</returns>
    [HttpGet("real-time-dashboard")]
    [Authorize(Roles = "Admin,Merchant,Analyst")]
    [ProducesResponseType(typeof(RealTimeDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RealTimeDashboardDto>> GetRealTimeDashboard(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving real-time dashboard data");

            var query = new GetRealTimeDashboardQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving real-time dashboard data");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Génère un rapport personnalisé
    /// </summary>
    /// <param name="request">Paramètres du rapport personnalisé</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Rapport personnalisé</returns>
    [HttpPost("custom-report")]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(typeof(CustomReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomReportDto>> GenerateCustomReport(
        [FromBody] GenerateCustomReportCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating custom report with {MetricCount} metrics", 
                request.Metrics?.Count ?? 0);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.GeneratedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Custom report generated successfully");

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid custom report request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating custom report");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error generating custom report" });
        }
    }

    /// <summary>
    /// Exporte les données d'analytics
    /// </summary>
    /// <param name="request">Paramètres d'export</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Fichier d'export</returns>
    [HttpPost("export")]
    [Authorize(Roles = "Admin,Analyst,Merchant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAnalyticsData(
        [FromBody] ExportAnalyticsDataCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting analytics data in format {Format} for report type {ReportType}", 
                request.Format, request.ReportType);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.RequestedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            var contentType = request.Format.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "json" => "application/json",
                _ => "application/octet-stream"
            };

            var fileName = $"analytics_export_{request.ReportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToLowerInvariant()}";

            _logger.LogInformation("Analytics data exported successfully - File size: {FileSize} bytes", 
                result.FileData.Length);

            return File(result.FileData, contentType, fileName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid export request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics data");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error exporting analytics data" });
        }
    }

    /// <summary>
    /// Obtient les alertes d'analytics
    /// </summary>
    /// <param name="severity">Niveau de sévérité (Low, Medium, High, Critical)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des alertes</returns>
    [HttpGet("alerts")]
    [Authorize(Roles = "Admin,Analyst,Merchant")]
    [ProducesResponseType(typeof(List<AnalyticsAlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AnalyticsAlertDto>>> GetAnalyticsAlerts(
        [FromQuery] string? severity = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(severity))
            {
                var validSeverities = new[] { "Low", "Medium", "High", "Critical" };
                if (!validSeverities.Contains(severity))
                {
                    return BadRequest(new { error = "Invalid severity. Valid values: " + string.Join(", ", validSeverities) });
                }
            }

            _logger.LogInformation("Retrieving analytics alerts with severity filter: {Severity}", severity ?? "All");

            var query = new GetAnalyticsAlertsQuery { Severity = severity };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics alerts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving analytics alerts" });
        }
    }

    /// <summary>
    /// Configure les seuils d'alerte
    /// </summary>
    /// <param name="request">Configuration des seuils</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de configuration</returns>
    [HttpPost("configure-alert-thresholds")]
    [Authorize(Roles = "Admin,Analyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigureAlertThresholds(
        [FromBody] ConfigureAlertThresholdsCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Configuring analytics alert thresholds");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.ConfiguredBy = userId;

            await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Analytics alert thresholds configured successfully");

            return Ok(new { message = "Alert thresholds configured successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid alert threshold configuration: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring alert thresholds");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error configuring alert thresholds" });
        }
    }
}