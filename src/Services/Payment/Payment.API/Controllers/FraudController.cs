using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands.FraudDetection;
using Payment.Application.Queries.FraudDetection;
using Payment.Application.DTOs.FraudDetection;
using MediatR;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion et l'analyse de détection de fraude
/// </summary>
[ApiController]
[Route("api/v1/fraud")]
[Authorize]
public class FraudController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FraudController> _logger;

    public FraudController(IMediator mediator, ILogger<FraudController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Analyse une transaction pour détecter la fraude
    /// </summary>
    /// <param name="request">Données de la transaction à analyser</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat de l'analyse de fraude</returns>
    [HttpPost("analyze")]
    [Authorize(Roles = "Admin,FraudAnalyst,System")]
    [ProducesResponseType(typeof(FraudAnalysisResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FraudAnalysisResultDto>> AnalyzeTransaction(
        [FromBody] AnalyzeTransactionFraudCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing transaction {TransactionId} for fraud", request.TransactionId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.AnalyzedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Fraud analysis completed for transaction {TransactionId} - Risk Score: {RiskScore}, Decision: {Decision}", 
                request.TransactionId, result.RiskScore, result.Decision);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid fraud analysis request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing transaction {TransactionId} for fraud", request.TransactionId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error analyzing transaction for fraud" });
        }
    }

    /// <summary>
    /// Obtient les transactions suspectes
    /// </summary>
    /// <param name="pageNumber">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="minRiskScore">Score de risque minimum</param>
    /// <param name="status">Statut de la transaction</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des transactions suspectes</returns>
    [HttpGet("suspicious-transactions")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(PagedResult<SuspiciousTransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<SuspiciousTransactionDto>>> GetSuspiciousTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] decimal? minRiskScore = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Invalid pagination parameters" });
            }

            if (minRiskScore.HasValue && (minRiskScore < 0 || minRiskScore > 100))
            {
                return BadRequest(new { error = "Risk score must be between 0 and 100" });
            }

            _logger.LogInformation("Retrieving suspicious transactions - Page: {PageNumber}, Size: {PageSize}, MinRiskScore: {MinRiskScore}", 
                pageNumber, pageSize, minRiskScore);

            var query = new GetSuspiciousTransactionsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                MinRiskScore = minRiskScore,
                Status = status
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suspicious transactions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving suspicious transactions" });
        }
    }

    /// <summary>
    /// Met à jour le statut d'une alerte de fraude
    /// </summary>
    /// <param name="alertId">ID de l'alerte</param>
    /// <param name="request">Nouvelle information de statut</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de mise à jour</returns>
    [HttpPut("alerts/{alertId:guid}/status")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFraudAlertStatus(
        Guid alertId,
        [FromBody] UpdateFraudAlertStatusCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating fraud alert {AlertId} status to {Status}", alertId, request.Status);

            request.AlertId = alertId;
            request.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(request, cancellationToken);

            if (!result)
            {
                return NotFound(new { error = "Fraud alert not found" });
            }

            _logger.LogInformation("Fraud alert {AlertId} status updated successfully", alertId);

            return Ok(new { message = "Fraud alert status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid fraud alert status update: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fraud alert {AlertId} status", alertId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error updating fraud alert status" });
        }
    }

    /// <summary>
    /// Ajoute une transaction à la liste blanche
    /// </summary>
    /// <param name="request">Informations de la transaction à whitelister</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'ajout</returns>
    [HttpPost("whitelist")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddToWhitelist(
        [FromBody] AddToWhitelistCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding to whitelist - Type: {Type}, Value: {Value}", 
                request.Type, request.Value);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.AddedBy = userId;

            await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Successfully added to whitelist");

            return Ok(new { message = "Successfully added to whitelist" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid whitelist request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to whitelist");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error adding to whitelist" });
        }
    }

    /// <summary>
    /// Ajoute une transaction à la liste noire
    /// </summary>
    /// <param name="request">Informations de la transaction à blacklister</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'ajout</returns>
    [HttpPost("blacklist")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddToBlacklist(
        [FromBody] AddToBlacklistCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding to blacklist - Type: {Type}, Value: {Value}", 
                request.Type, request.Value);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.AddedBy = userId;

            await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Successfully added to blacklist");

            return Ok(new { message = "Successfully added to blacklist" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid blacklist request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to blacklist");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error adding to blacklist" });
        }
    }

    /// <summary>
    /// Obtient les métriques de fraude
    /// </summary>
    /// <param name="period">Période d'analyse</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Métriques de fraude</returns>
    [HttpGet("metrics")]
    [Authorize(Roles = "Admin,FraudAnalyst,Analyst")]
    [ProducesResponseType(typeof(FraudMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FraudMetricsDto>> GetFraudMetrics(
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

            _logger.LogInformation("Retrieving fraud metrics for period {Period}", period);

            var query = new GetFraudMetricsQuery { Period = period };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fraud metrics");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving fraud metrics" });
        }
    }

    /// <summary>
    /// Obtient les règles de fraude configurées
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des règles de fraude</returns>
    [HttpGet("rules")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(List<FraudRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<FraudRuleDto>>> GetFraudRules(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving fraud rules");

            var query = new GetFraudRulesQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fraud rules");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error retrieving fraud rules" });
        }
    }

    /// <summary>
    /// Crée ou met à jour une règle de fraude
    /// </summary>
    /// <param name="request">Informations de la règle</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de création/mise à jour</returns>
    [HttpPost("rules")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(FraudRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FraudRuleDto>> CreateFraudRule(
        [FromBody] CreateFraudRuleCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating fraud rule: {RuleName}", request.Name);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.CreatedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Fraud rule created successfully with ID {RuleId}", result.Id);

            return CreatedAtAction(nameof(GetFraudRules), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid fraud rule request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fraud rule");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error creating fraud rule" });
        }
    }

    /// <summary>
    /// Met à jour une règle de fraude existante
    /// </summary>
    /// <param name="ruleId">ID de la règle</param>
    /// <param name="request">Nouvelles informations de la règle</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Règle mise à jour</returns>
    [HttpPut("rules/{ruleId:guid}")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(FraudRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FraudRuleDto>> UpdateFraudRule(
        Guid ruleId,
        [FromBody] UpdateFraudRuleCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating fraud rule {RuleId}", ruleId);

            request.RuleId = ruleId;
            request.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var result = await _mediator.Send(request, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = "Fraud rule not found" });
            }

            _logger.LogInformation("Fraud rule {RuleId} updated successfully", ruleId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid fraud rule update: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fraud rule {RuleId}", ruleId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error updating fraud rule" });
        }
    }

    /// <summary>
    /// Supprime une règle de fraude
    /// </summary>
    /// <param name="ruleId">ID de la règle</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("rules/{ruleId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFraudRule(
        Guid ruleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting fraud rule {RuleId}", ruleId);

            var command = new DeleteFraudRuleCommand 
            { 
                RuleId = ruleId,
                DeletedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound(new { error = "Fraud rule not found" });
            }

            _logger.LogInformation("Fraud rule {RuleId} deleted successfully", ruleId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fraud rule {RuleId}", ruleId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error deleting fraud rule" });
        }
    }

    /// <summary>
    /// Lance un test de performance du moteur de fraude
    /// </summary>
    /// <param name="request">Paramètres du test</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultats du test</returns>
    [HttpPost("performance-test")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(FraudEnginePerformanceTestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FraudEnginePerformanceTestDto>> RunPerformanceTest(
        [FromBody] RunFraudEnginePerformanceTestCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Running fraud engine performance test with {TransactionCount} transactions", 
                request.TransactionCount);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            request.InitiatedBy = userId;

            var result = await _mediator.Send(request, cancellationToken);

            _logger.LogInformation("Fraud engine performance test completed - Average processing time: {AverageTime}ms", 
                result.AverageProcessingTimeMs);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid performance test request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running fraud engine performance test");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error running performance test" });
        }
    }
}