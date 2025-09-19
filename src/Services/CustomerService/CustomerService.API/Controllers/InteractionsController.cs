using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CustomerService.Application.Commands.Interactions;
using CustomerService.Application.Queries.Interactions;
using CustomerService.Application.DTOs.Interactions;
using Serilog;

namespace CustomerService.API.Controllers
{
    /// <summary>
    /// API Controller pour la gestion des interactions clients - NiesPro Customer Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InteractionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InteractionsController> _logger;

        public InteractionsController(IMediator mediator, ILogger<InteractionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ================================
        // GESTION DES INTERACTIONS
        // ================================

        /// <summary>
        /// Créer une nouvelle interaction
        /// </summary>
        /// <param name="command">Données de l'interaction</param>
        /// <returns>Interaction créée</returns>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerInteractionDto>> CreateInteraction([FromBody] CreateCustomerInteractionCommand command)
        {
            _logger.LogInformation("Creating new customer interaction - Customer: {CustomerId}, Type: {Type}", 
                command.CustomerId, command.Type);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer interaction created successfully: {InteractionId}", result.Id);
            
            return CreatedAtAction(nameof(GetInteractionById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Récupérer une interaction par ID
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="includeAttachments">Inclure les pièces jointes</param>
        /// <param name="includeFollowUps">Inclure les suivis</param>
        /// <returns>Détails de l'interaction</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerInteractionDto>> GetInteractionById(
            Guid id,
            [FromQuery] bool includeAttachments = true,
            [FromQuery] bool includeFollowUps = true)
        {
            var query = new GetCustomerInteractionByIdQuery
            {
                Id = id,
                IncludeAttachments = includeAttachments,
                IncludeFollowUps = includeFollowUps
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Customer interaction with ID {id} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour une interaction
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="command">Données de mise à jour</param>
        /// <returns>Interaction mise à jour</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerInteractionDto>> UpdateInteraction(Guid id, [FromBody] UpdateCustomerInteractionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating customer interaction: {InteractionId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer interaction updated successfully: {InteractionId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Supprimer une interaction (soft delete)
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="reason">Raison de la suppression</param>
        /// <returns>Confirmation de suppression</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteInteraction(Guid id, [FromQuery] string? reason = null)
        {
            _logger.LogInformation("Deleting customer interaction: {InteractionId}, Reason: {Reason}", id, reason);
            
            var command = new DeleteCustomerInteractionCommand 
            { 
                Id = id,
                Reason = reason ?? "Deleted by administrator"
            };
            
            await _mediator.Send(command);
            
            _logger.LogInformation("Customer interaction deleted successfully: {InteractionId}", id);
            
            return NoContent();
        }

        // ================================
        // RECHERCHE D'INTERACTIONS
        // ================================

        /// <summary>
        /// Rechercher des interactions
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="type">Type d'interaction</param>
        /// <param name="channel">Canal d'interaction</param>
        /// <param name="status">Statut de l'interaction</param>
        /// <param name="assignedToUserId">ID de l'utilisateur assigné</param>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="searchTerm">Terme de recherche</param>
        /// <param name="priority">Niveau de priorité</param>
        /// <param name="hasFollowUp">A un suivi</param>
        /// <param name="sortBy">Tri par champ</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste paginée des interactions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CustomerInteractionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerInteractionDto>>> SearchInteractions(
            [FromQuery] Guid? customerId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? channel = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? assignedToUserId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? priority = null,
            [FromQuery] bool? hasFollowUp = null,
            [FromQuery] string sortBy = "InteractionDate",
            [FromQuery] bool sortDescending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new SearchCustomerInteractionsQuery
            {
                CustomerId = customerId,
                Type = type,
                Channel = channel,
                Status = status,
                AssignedToUserId = assignedToUserId,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = searchTerm,
                Priority = priority,
                HasFollowUp = hasFollowUp,
                SortBy = sortBy,
                SortDescending = sortDescending,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer les interactions d'un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="type">Type d'interaction</param>
        /// <param name="includeResolved">Inclure les interactions résolues</param>
        /// <param name="limit">Nombre maximum d'interactions</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <returns>Liste des interactions du client</returns>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(PagedResult<CustomerInteractionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<CustomerInteractionDto>>> GetCustomerInteractions(
            Guid customerId,
            [FromQuery] string? type = null,
            [FromQuery] bool includeResolved = true,
            [FromQuery] int limit = 50,
            [FromQuery] bool sortDescending = true)
        {
            var query = new GetCustomerInteractionsQuery
            {
                CustomerId = customerId,
                Type = type,
                IncludeResolved = includeResolved,
                PageNumber = 1,
                PageSize = Math.Min(limit, 100),
                SortBy = "InteractionDate",
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // GESTION DES SUIVIS
        // ================================

        /// <summary>
        /// Ajouter un suivi à une interaction
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="command">Données du suivi</param>
        /// <returns>Suivi créé</returns>
        [HttpPost("{id:guid}/follow-ups")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(InteractionFollowUpDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InteractionFollowUpDto>> AddFollowUp(Guid id, [FromBody] AddInteractionFollowUpCommand command)
        {
            if (id != command.InteractionId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Adding follow-up to interaction: {InteractionId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Follow-up added successfully to interaction: {InteractionId}", id);
            
            return CreatedAtAction(nameof(GetInteractionById), new { id }, result);
        }

        /// <summary>
        /// Récupérer les suivis d'une interaction
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <returns>Liste des suivis</returns>
        [HttpGet("{id:guid}/follow-ups")]
        [ProducesResponseType(typeof(List<InteractionFollowUpDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<InteractionFollowUpDto>>> GetInteractionFollowUps(Guid id, [FromQuery] bool sortDescending = true)
        {
            var query = new GetInteractionFollowUpsQuery
            {
                InteractionId = id,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // ASSIGNATION ET WORKFLOW
        // ================================

        /// <summary>
        /// Assigner une interaction à un utilisateur
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="command">Commande d'assignation</param>
        /// <returns>Interaction mise à jour</returns>
        [HttpPost("{id:guid}/assign")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerInteractionDto>> AssignInteraction(Guid id, [FromBody] AssignInteractionCommand command)
        {
            if (id != command.InteractionId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Assigning interaction {InteractionId} to user {UserId}", id, command.AssignedToUserId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Interaction assigned successfully: {InteractionId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Changer le statut d'une interaction
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="command">Commande de changement de statut</param>
        /// <returns>Interaction mise à jour</returns>
        [HttpPost("{id:guid}/status")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerInteractionDto>> ChangeInteractionStatus(Guid id, [FromBody] ChangeInteractionStatusCommand command)
        {
            if (id != command.InteractionId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Changing status of interaction {InteractionId} to {Status}", id, command.NewStatus);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Interaction status changed successfully: {InteractionId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Résoudre une interaction
        /// </summary>
        /// <param name="id">ID de l'interaction</param>
        /// <param name="command">Commande de résolution</param>
        /// <returns>Interaction résolue</returns>
        [HttpPost("{id:guid}/resolve")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerInteractionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerInteractionDto>> ResolveInteraction(Guid id, [FromBody] ResolveInteractionCommand command)
        {
            if (id != command.InteractionId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Resolving interaction: {InteractionId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Interaction resolved successfully: {InteractionId}", id);
            
            return Ok(result);
        }

        // ================================
        // STATISTIQUES ET ANALYTICS
        // ================================

        /// <summary>
        /// Récupérer les statistiques des interactions
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="groupBy">Grouper par (day, week, month)</param>
        /// <param name="filterBy">Filtrer par type/canal/statut</param>
        /// <param name="includeResolution">Inclure les temps de résolution</param>
        /// <param name="includeSatisfaction">Inclure les données de satisfaction</param>
        /// <returns>Statistiques des interactions</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(InteractionStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<InteractionStatisticsDto>> GetInteractionStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string groupBy = "day",
            [FromQuery] string? filterBy = null,
            [FromQuery] bool includeResolution = true,
            [FromQuery] bool includeSatisfaction = true)
        {
            var query = new GetInteractionStatisticsQuery
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow,
                GroupBy = groupBy,
                FilterBy = filterBy,
                IncludeResolutionMetrics = includeResolution,
                IncludeSatisfactionMetrics = includeSatisfaction
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Tableau de bord des interactions
        /// </summary>
        /// <param name="period">Période (today, week, month)</param>
        /// <param name="userId">ID de l'utilisateur (pour filtrer)</param>
        /// <returns>Dashboard des interactions</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetInteractionsDashboard(
            [FromQuery] string period = "week",
            [FromQuery] Guid? userId = null)
        {
            var (startDate, endDate) = period.ToLower() switch
            {
                "today" => (DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1)),
                "week" => (DateTime.UtcNow.Date.AddDays(-7), DateTime.UtcNow),
                "month" => (DateTime.UtcNow.Date.AddMonths(-1), DateTime.UtcNow),
                _ => (DateTime.UtcNow.Date.AddDays(-7), DateTime.UtcNow)
            };

            // Combinaison de plusieurs requêtes pour créer un dashboard complet
            var statsQuery = new GetInteractionStatisticsQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                GroupBy = "day",
                IncludeResolutionMetrics = true,
                IncludeSatisfactionMetrics = true
            };

            var stats = await _mediator.Send(statsQuery);

            // Récupération des interactions récentes
            var recentQuery = new SearchCustomerInteractionsQuery
            {
                AssignedToUserId = userId,
                StartDate = startDate,
                PageNumber = 1,
                PageSize = 10,
                SortBy = "InteractionDate",
                SortDescending = true
            };

            var recentInteractions = await _mediator.Send(recentQuery);

            var dashboard = new
            {
                Period = new { Start = startDate, End = endDate },
                Statistics = stats,
                RecentInteractions = recentInteractions.Items,
                UserId = userId,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(dashboard);
        }

        // ================================
        // RAPPORTS ET EXPORTS
        // ================================

        /// <summary>
        /// Export des interactions
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="customerId">ID du client (optionnel)</param>
        /// <param name="includeFollowUps">Inclure les suivis</param>
        /// <param name="format">Format d'export (csv, excel)</param>
        /// <returns>Fichier d'export</returns>
        [HttpGet("export")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportInteractions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] bool includeFollowUps = false,
            [FromQuery] string format = "csv")
        {
            if (!new[] { "csv", "excel" }.Contains(format.ToLower()))
            {
                return BadRequest("Format must be 'csv' or 'excel'");
            }

            _logger.LogInformation("Exporting interactions - Period: {StartDate} to {EndDate}, Format: {Format}", 
                startDate, endDate, format);

            // Dans une implémentation réelle, on utiliserait un service d'export
            var fileName = $"interactions_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Placeholder pour l'export réel
            var content = "Export functionality to be implemented";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, contentType, fileName);
        }
    }
}