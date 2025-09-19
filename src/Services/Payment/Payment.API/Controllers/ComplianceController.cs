using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands.Compliance;
using Payment.Application.Queries.Compliance;
using Payment.Application.DTOs.Compliance;
using Payment.Application.DTOs;
using MediatR;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion de la conformité PCI-DSS et des audits
/// </summary>
[ApiController]
[Route("api/v1/compliance")]
[Authorize]
public class ComplianceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(IMediator mediator, ILogger<ComplianceController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Génère un rapport d'audit de conformité PCI-DSS
    /// </summary>
    /// <param name="request">Paramètres du rapport</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Rapport d'audit de conformité</returns>
    [HttpPost("audit-report")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(typeof(ComplianceAuditReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ComplianceAuditReportDto>> GenerateAuditReport(
        [FromBody] GenerateAuditReportCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating compliance audit report for period {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.RequestedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Compliance audit report generated successfully with {ViolationCount} violations", 
                result.ViolationCount);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid audit report request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance audit report");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error generating audit report" });
        }
    }

    /// <summary>
    /// Obtient l'état actuel de la conformité PCI-DSS
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>État de la conformité</returns>
    [HttpGet("status")]
    [Authorize(Roles = "Admin,ComplianceOfficer,Merchant")]
    [ProducesResponseType(typeof(ComplianceStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ComplianceStatusDto>> GetComplianceStatus(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving current compliance status");

            var query = new GetComplianceStatusQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance status");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving compliance status" });
        }
    }

    /// <summary>
    /// Valide la conformité d'une transaction spécifique
    /// </summary>
    /// <param name="transactionId">ID de la transaction</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de la validation de conformité</returns>
    [HttpPost("validate-transaction/{transactionId:guid}")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(typeof(TransactionComplianceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionComplianceDto>> ValidateTransactionCompliance(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating compliance for transaction {TransactionId}", transactionId);

            var command = new ValidateTransactionComplianceCommand 
            { 
                TransactionId = transactionId,
                ValidatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = "Transaction not found" });
            }

            _logger.LogInformation("Transaction compliance validation completed for {TransactionId} - Compliant: {IsCompliant}", 
                transactionId, result.IsCompliant);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid transaction compliance validation request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transaction compliance for {TransactionId}", transactionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error validating transaction compliance" });
        }
    }

    /// <summary>
    /// Obtient l'historique des audits de conformité
    /// </summary>
    /// <param name="pageNumber">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="startDate">Date de début (optionnel)</param>
    /// <param name="endDate">Date de fin (optionnel)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des audits</returns>
    [HttpGet("audit-history")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(typeof(PagedResult<ComplianceAuditSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ComplianceAuditSummaryDto>>> GetAuditHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Invalid pagination parameters" });
            }

            _logger.LogInformation("Retrieving audit history - Page: {PageNumber}, Size: {PageSize}", 
                pageNumber, pageSize);

            var query = new GetAuditHistoryQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit history");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving audit history" });
        }
    }

    /// <summary>
    /// Lance une vérification de conformité en temps réel
    /// </summary>
    /// <param name="request">Paramètres de la vérification</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de la vérification</returns>
    [HttpPost("real-time-check")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(typeof(RealTimeComplianceCheckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RealTimeComplianceCheckDto>> PerformRealTimeComplianceCheck(
        [FromBody] RealTimeComplianceCheckCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing real-time compliance check");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.InitiatedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Real-time compliance check completed - Overall Score: {ComplianceScore}", 
                result.ComplianceScore);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid real-time compliance check request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing real-time compliance check");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error performing compliance check" });
        }
    }

    /// <summary>
    /// Exporte les données de conformité pour audit externe
    /// </summary>
    /// <param name="request">Paramètres d'export</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Fichier d'export sécurisé</returns>
    [HttpPost("export")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportComplianceData(
        [FromBody] ExportComplianceDataCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting compliance data in format {Format} for period {StartDate} to {EndDate}", 
                request.Format, request.StartDate, request.EndDate);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.RequestedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            var contentType = request.Format.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "application/octet-stream"
            };

            var fileName = $"compliance_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToLowerInvariant()}";

            _logger.LogInformation("Compliance data exported successfully - File size: {FileSize} bytes", 
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
            _logger.LogError(ex, "Error exporting compliance data");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error exporting compliance data" });
        }
    }

    /// <summary>
    /// Obtient les métriques de conformité pour le tableau de bord
    /// </summary>
    /// <param name="period">Période d'analyse (Last7Days, Last30Days, Last90Days)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Métriques de conformité</returns>
    [HttpGet("metrics")]
    [Authorize(Roles = "Admin,ComplianceOfficer,Merchant")]
    [ProducesResponseType(typeof(ComplianceMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ComplianceMetricsDto>> GetComplianceMetrics(
        [FromQuery] string period = "Last30Days",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validPeriods = new[] { "Last7Days", "Last30Days", "Last90Days" };
            if (!validPeriods.Contains(period))
            {
                return BadRequest(new { error = "Invalid period. Valid values: " + string.Join(", ", validPeriods) });
            }

            _logger.LogInformation("Retrieving compliance metrics for period {Period}", period);

            var query = new GetComplianceMetricsQuery { Period = period };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving compliance metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving compliance metrics" });
        }
    }

    /// <summary>
    /// Configure les alertes de conformité
    /// </summary>
    /// <param name="request">Configuration des alertes</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de configuration</returns>
    [HttpPost("configure-alerts")]
    [Authorize(Roles = "Admin,ComplianceOfficer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ConfigureComplianceAlerts(
        [FromBody] ConfigureComplianceAlertsCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Configuring compliance alerts");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.ConfiguredBy = userId;

            await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Compliance alerts configured successfully");

            return Ok(new { message = "Compliance alerts configured successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid alert configuration: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring compliance alerts");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error configuring alerts" });
        }
    }
}