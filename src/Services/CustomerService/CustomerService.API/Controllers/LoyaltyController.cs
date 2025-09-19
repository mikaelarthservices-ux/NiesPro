using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CustomerService.Application.Commands.Loyalty;
using CustomerService.Application.Queries.Loyalty;
using CustomerService.Application.DTOs.Loyalty;
using Serilog;

namespace CustomerService.API.Controllers
{
    /// <summary>
    /// API Controller pour la gestion de la fidélité - NiesPro Customer Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class LoyaltyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoyaltyController> _logger;

        public LoyaltyController(IMediator mediator, ILogger<LoyaltyController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ================================
        // GESTION DES PROGRAMMES DE FIDÉLITÉ
        // ================================

        /// <summary>
        /// Récupérer tous les programmes de fidélité
        /// </summary>
        /// <param name="activeOnly">Programmes actifs seulement</param>
        /// <param name="includeStatistics">Inclure les statistiques</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste paginée des programmes</returns>
        [HttpGet("programs")]
        [ProducesResponseType(typeof(PagedResult<LoyaltyProgramDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<LoyaltyProgramDto>>> GetLoyaltyPrograms(
            [FromQuery] bool activeOnly = true,
            [FromQuery] bool includeStatistics = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetLoyaltyProgramsQuery
            {
                ActiveOnly = activeOnly,
                IncludeStatistics = includeStatistics,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                SortBy = "Name",
                SortDescending = false
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Créer un nouveau programme de fidélité
        /// </summary>
        /// <param name="command">Données du programme</param>
        /// <returns>Programme créé</returns>
        [HttpPost("programs")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyProgramDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoyaltyProgramDto>> CreateLoyaltyProgram([FromBody] CreateLoyaltyProgramCommand command)
        {
            _logger.LogInformation("Creating new loyalty program: {Name}", command.Name);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Loyalty program created successfully: {ProgramId}", result.Id);
            
            return CreatedAtAction(nameof(GetLoyaltyPrograms), new { id = result.Id }, result);
        }

        /// <summary>
        /// Mettre à jour un programme de fidélité
        /// </summary>
        /// <param name="id">ID du programme</param>
        /// <param name="command">Données de mise à jour</param>
        /// <returns>Programme mis à jour</returns>
        [HttpPut("programs/{id:guid}")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyProgramDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoyaltyProgramDto>> UpdateLoyaltyProgram(Guid id, [FromBody] UpdateLoyaltyProgramCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating loyalty program: {ProgramId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Loyalty program updated successfully: {ProgramId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Récupérer les statistiques d'un programme de fidélité
        /// </summary>
        /// <param name="id">ID du programme</param>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="includeTrends">Inclure les tendances</param>
        /// <param name="includePredictions">Inclure les prédictions</param>
        /// <returns>Statistiques du programme</returns>
        [HttpGet("programs/{id:guid}/statistics")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyProgramStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoyaltyProgramStatsDto>> GetLoyaltyProgramStats(
            Guid id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeTrends = true,
            [FromQuery] bool includePredictions = false)
        {
            var query = new GetLoyaltyProgramStatsQuery
            {
                ProgramId = id,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow,
                IncludeTrends = includeTrends,
                IncludePredictions = includePredictions
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Loyalty program with ID {id} not found");
            }

            return Ok(result);
        }

        // ================================
        // GESTION DES POINTS DE FIDÉLITÉ
        // ================================

        /// <summary>
        /// Attribuer des points de fidélité à un client
        /// </summary>
        /// <param name="command">Données d'attribution de points</param>
        /// <returns>Transaction de fidélité créée</returns>
        [HttpPost("points/earn")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoyaltyTransactionDto>> EarnLoyaltyPoints([FromBody] EarnLoyaltyPointsCommand command)
        {
            _logger.LogInformation("Earning loyalty points for customer: {CustomerId}, Amount: {Amount}", 
                command.CustomerId, command.Amount);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Loyalty points earned successfully: {TransactionId}, Points: {Points}", 
                result.Id, result.PointsEarned);
            
            return CreatedAtAction(nameof(GetLoyaltyTransactionHistory), 
                new { customerId = command.CustomerId }, result);
        }

        /// <summary>
        /// Utiliser des points de fidélité
        /// </summary>
        /// <param name="command">Données d'utilisation de points</param>
        /// <returns>Transaction de fidélité créée</returns>
        [HttpPost("points/redeem")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoyaltyTransactionDto>> RedeemLoyaltyPoints([FromBody] RedeemLoyaltyPointsCommand command)
        {
            _logger.LogInformation("Redeeming loyalty points for customer: {CustomerId}, Reward: {RewardId}", 
                command.CustomerId, command.RewardId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Loyalty points redeemed successfully: {TransactionId}, Points: {Points}", 
                result.Id, result.PointsUsed);
            
            return CreatedAtAction(nameof(GetLoyaltyTransactionHistory), 
                new { customerId = command.CustomerId }, result);
        }

        /// <summary>
        /// Récupérer les statistiques de fidélité d'un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="includeTransactionHistory">Inclure l'historique des transactions</param>
        /// <param name="includePredictions">Inclure les prédictions</param>
        /// <returns>Statistiques de fidélité du client</returns>
        [HttpGet("customers/{customerId:guid}/stats")]
        [ProducesResponseType(typeof(CustomerLoyaltyStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerLoyaltyStatsDto>> GetCustomerLoyaltyStats(
            Guid customerId,
            [FromQuery] bool includeTransactionHistory = false,
            [FromQuery] bool includePredictions = false)
        {
            var query = new GetCustomerLoyaltyStatsQuery
            {
                CustomerId = customerId,
                IncludeTransactionHistory = includeTransactionHistory,
                IncludePredictions = includePredictions
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Customer with ID {customerId} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Récupérer l'historique des transactions de fidélité
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="transactionType">Type de transaction</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <param name="includeSummary">Inclure le résumé</param>
        /// <returns>Historique des transactions</returns>
        [HttpGet("customers/{customerId:guid}/transactions")]
        [ProducesResponseType(typeof(PagedResult<LoyaltyTransactionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<LoyaltyTransactionDto>>> GetLoyaltyTransactionHistory(
            Guid customerId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeSummary = true)
        {
            var query = new GetLoyaltyTransactionHistoryQuery
            {
                CustomerId = customerId,
                StartDate = startDate,
                EndDate = endDate,
                TransactionType = transactionType,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                SortBy = "TransactionDate",
                SortDescending = true,
                IncludeSummary = includeSummary
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // GESTION DES RÉCOMPENSES
        // ================================

        /// <summary>
        /// Créer une nouvelle récompense
        /// </summary>
        /// <param name="command">Données de la récompense</param>
        /// <returns>Récompense créée</returns>
        [HttpPost("rewards")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(LoyaltyRewardDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoyaltyRewardDto>> CreateLoyaltyReward([FromBody] CreateLoyaltyRewardCommand command)
        {
            _logger.LogInformation("Creating new loyalty reward: {Name}", command.Name);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Loyalty reward created successfully: {RewardId}", result.Id);
            
            return CreatedAtAction(nameof(GetAvailableRewards), new { id = result.Id }, result);
        }

        /// <summary>
        /// Récupérer les récompenses disponibles
        /// </summary>
        /// <param name="programId">ID du programme</param>
        /// <param name="customerId">ID du client (pour vérifier l'éligibilité)</param>
        /// <param name="rewardType">Type de récompense</param>
        /// <param name="sortByPersonalization">Tri par personnalisation</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste des récompenses disponibles</returns>
        [HttpGet("rewards")]
        [ProducesResponseType(typeof(PagedResult<LoyaltyRewardDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<LoyaltyRewardDto>>> GetAvailableRewards(
            [FromQuery] Guid? programId = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] string? rewardType = null,
            [FromQuery] bool sortByPersonalization = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetAvailableRewardsQuery
            {
                ProgramId = programId,
                CustomerId = customerId,
                RewardType = rewardType,
                SortByPersonalization = sortByPersonalization,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                SortBy = "Name",
                SortDescending = false
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Vérifier l'éligibilité à une récompense
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="rewardId">ID de la récompense</param>
        /// <returns>Détails d'éligibilité</returns>
        [HttpGet("rewards/{rewardId:guid}/eligibility/{customerId:guid}")]
        [ProducesResponseType(typeof(RewardEligibilityDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RewardEligibilityDto>> CheckRewardEligibility(Guid customerId, Guid rewardId)
        {
            var query = new CheckRewardEligibilityQuery
            {
                CustomerId = customerId,
                RewardId = rewardId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // RAPPORTS ET ANALYTICS
        // ================================

        /// <summary>
        /// Tableau de bord fidélité (résumé global)
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>Dashboard de fidélité</returns>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetLoyaltyDashboard(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var period = new { 
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1), 
                EndDate = endDate ?? DateTime.UtcNow 
            };

            // Combinaison de plusieurs requêtes pour créer un dashboard complet
            var programsQuery = new GetLoyaltyProgramsQuery 
            { 
                ActiveOnly = true, 
                IncludeStatistics = true, 
                PageNumber = 1, 
                PageSize = 10 
            };

            var programs = await _mediator.Send(programsQuery);

            var dashboard = new
            {
                Period = period,
                ActivePrograms = programs.TotalCount,
                Programs = programs.Items,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Export des données de fidélité
        /// </summary>
        /// <param name="programId">ID du programme</param>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="format">Format d'export (csv, excel)</param>
        /// <returns>Fichier d'export</returns>
        [HttpGet("export")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportLoyaltyData(
            [FromQuery] Guid? programId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string format = "csv")
        {
            if (!new[] { "csv", "excel" }.Contains(format.ToLower()))
            {
                return BadRequest("Format must be 'csv' or 'excel'");
            }

            _logger.LogInformation("Exporting loyalty data - Program: {ProgramId}, Format: {Format}", programId, format);

            // Dans une implémentation réelle, on utiliserait un service d'export
            var fileName = $"loyalty_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Placeholder pour l'export réel
            var content = "Export functionality to be implemented";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, contentType, fileName);
        }
    }
}