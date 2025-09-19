using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CustomerService.Application.Commands.Segments;
using CustomerService.Application.Queries.Segments;
using CustomerService.Application.DTOs.Segments;
using Serilog;

namespace CustomerService.API.Controllers
{
    /// <summary>
    /// API Controller pour la gestion des segments clients - NiesPro Customer Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SegmentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SegmentsController> _logger;

        public SegmentsController(IMediator mediator, ILogger<SegmentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ================================
        // GESTION DES SEGMENTS
        // ================================

        /// <summary>
        /// Récupérer tous les segments
        /// </summary>
        /// <param name="activeOnly">Segments actifs seulement</param>
        /// <param name="includeStatistics">Inclure les statistiques</param>
        /// <param name="sortBy">Tri par champ</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste paginée des segments</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CustomerSegmentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerSegmentDto>>> GetCustomerSegments(
            [FromQuery] bool activeOnly = true,
            [FromQuery] bool includeStatistics = false,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetCustomerSegmentsQuery
            {
                ActiveOnly = activeOnly,
                IncludeStatistics = includeStatistics,
                SortBy = sortBy,
                SortDescending = sortDescending,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer un segment par ID
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="includeCustomers">Inclure les clients du segment</param>
        /// <param name="includeStatistics">Inclure les statistiques</param>
        /// <returns>Détails du segment</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerSegmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerSegmentDto>> GetCustomerSegmentById(
            Guid id,
            [FromQuery] bool includeCustomers = false,
            [FromQuery] bool includeStatistics = true)
        {
            var query = new GetCustomerSegmentByIdQuery
            {
                Id = id,
                IncludeCustomers = includeCustomers,
                IncludeStatistics = includeStatistics
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Customer segment with ID {id} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Créer un nouveau segment
        /// </summary>
        /// <param name="command">Données du segment</param>
        /// <returns>Segment créé</returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerSegmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerSegmentDto>> CreateCustomerSegment([FromBody] CreateCustomerSegmentCommand command)
        {
            _logger.LogInformation("Creating new customer segment: {Name}", command.Name);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer segment created successfully: {SegmentId}", result.Id);
            
            return CreatedAtAction(nameof(GetCustomerSegmentById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Mettre à jour un segment
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="command">Données de mise à jour</param>
        /// <returns>Segment mis à jour</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerSegmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerSegmentDto>> UpdateCustomerSegment(Guid id, [FromBody] UpdateCustomerSegmentCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating customer segment: {SegmentId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer segment updated successfully: {SegmentId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Supprimer un segment (désactivation)
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <returns>Confirmation de suppression</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCustomerSegment(Guid id)
        {
            _logger.LogInformation("Deleting customer segment: {SegmentId}", id);
            
            var command = new DeleteCustomerSegmentCommand { Id = id };
            await _mediator.Send(command);
            
            _logger.LogInformation("Customer segment deleted successfully: {SegmentId}", id);
            
            return NoContent();
        }

        // ================================
        // SEGMENTATION AUTOMATIQUE
        // ================================

        /// <summary>
        /// Calculer la segmentation pour tous les clients
        /// </summary>
        /// <param name="command">Paramètres de calcul</param>
        /// <returns>Résultat de la segmentation</returns>
        [HttpPost("calculate")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(SegmentationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SegmentationResultDto>> CalculateCustomerSegmentation([FromBody] CalculateSegmentationCommand command)
        {
            _logger.LogInformation("Starting customer segmentation calculation with algorithm: {Algorithm}", command.Algorithm);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer segmentation completed - {CustomerCount} customers processed, {SegmentCount} segments created",
                result.CustomersProcessed, result.SegmentsCreated);
            
            return Ok(result);
        }

        /// <summary>
        /// Assigner des clients à un segment
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="command">Commande d'assignation</param>
        /// <returns>Résultat de l'assignation</returns>
        [HttpPost("{id:guid}/assign")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerAssignmentResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerAssignmentResultDto>> AssignCustomersToSegment(Guid id, [FromBody] AssignCustomersToSegmentCommand command)
        {
            if (id != command.SegmentId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Assigning {CustomerCount} customers to segment: {SegmentId}", 
                command.CustomerIds?.Count ?? 0, id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer assignment completed - {AssignedCount} customers assigned to segment: {SegmentId}",
                result.AssignedCount, id);
            
            return Ok(result);
        }

        /// <summary>
        /// Retirer des clients d'un segment
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="command">Commande de retrait</param>
        /// <returns>Résultat du retrait</returns>
        [HttpPost("{id:guid}/unassign")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerAssignmentResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerAssignmentResultDto>> UnassignCustomersFromSegment(Guid id, [FromBody] UnassignCustomersFromSegmentCommand command)
        {
            if (id != command.SegmentId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Unassigning {CustomerCount} customers from segment: {SegmentId}", 
                command.CustomerIds?.Count ?? 0, id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer unassignment completed - {UnassignedCount} customers removed from segment: {SegmentId}",
                result.UnassignedCount, id);
            
            return Ok(result);
        }

        // ================================
        // CLIENTS D'UN SEGMENT
        // ================================

        /// <summary>
        /// Récupérer les clients d'un segment
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="includeInactive">Inclure les clients inactifs</param>
        /// <param name="sortBy">Tri par champ</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <param name="searchTerm">Terme de recherche</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste paginée des clients du segment</returns>
        [HttpGet("{id:guid}/customers")]
        [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<object>>> GetSegmentCustomers(
            Guid id,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string sortBy = "LastName",
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetSegmentCustomersQuery
            {
                SegmentId = id,
                IncludeInactive = includeInactive,
                SortBy = sortBy,
                SortDescending = sortDescending,
                SearchTerm = searchTerm,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer les statistiques d'un segment
        /// </summary>
        /// <param name="id">ID du segment</param>
        /// <param name="startDate">Date de début pour les statistiques</param>
        /// <param name="endDate">Date de fin pour les statistiques</param>
        /// <param name="includeTrends">Inclure les tendances</param>
        /// <param name="includePredictions">Inclure les prédictions</param>
        /// <returns>Statistiques détaillées du segment</returns>
        [HttpGet("{id:guid}/statistics")]
        [ProducesResponseType(typeof(CustomerSegmentStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerSegmentStatsDto>> GetSegmentStatistics(
            Guid id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeTrends = true,
            [FromQuery] bool includePredictions = false)
        {
            var query = new GetSegmentStatisticsQuery
            {
                SegmentId = id,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-3),
                EndDate = endDate ?? DateTime.UtcNow,
                IncludeTrends = includeTrends,
                IncludePredictions = includePredictions
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Customer segment with ID {id} not found");
            }

            return Ok(result);
        }

        // ================================
        // ANALYSE ET PRÉDICTIONS
        // ================================

        /// <summary>
        /// Analyser l'évolution des segments
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="includeChurnAnalysis">Inclure l'analyse de churn</param>
        /// <param name="includeValueAnalysis">Inclure l'analyse de valeur</param>
        /// <returns>Analyse d'évolution des segments</returns>
        [HttpGet("analytics/evolution")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(SegmentEvolutionAnalysisDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SegmentEvolutionAnalysisDto>> GetSegmentEvolutionAnalysis(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeChurnAnalysis = true,
            [FromQuery] bool includeValueAnalysis = true)
        {
            var query = new GetSegmentEvolutionAnalysisQuery
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-6),
                EndDate = endDate ?? DateTime.UtcNow,
                IncludeChurnAnalysis = includeChurnAnalysis,
                IncludeValueAnalysis = includeValueAnalysis
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Recommander des segments pour un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="maxRecommendations">Nombre maximum de recommandations</param>
        /// <param name="includeExplanations">Inclure les explications</param>
        /// <returns>Recommandations de segments</returns>
        [HttpGet("recommendations/{customerId:guid}")]
        [ProducesResponseType(typeof(List<SegmentRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<SegmentRecommendationDto>>> GetSegmentRecommendations(
            Guid customerId,
            [FromQuery] int maxRecommendations = 5,
            [FromQuery] bool includeExplanations = true)
        {
            var query = new GetSegmentRecommendationsQuery
            {
                CustomerId = customerId,
                MaxRecommendations = Math.Min(maxRecommendations, 10),
                IncludeExplanations = includeExplanations
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // RAPPORTS ET EXPORTS
        // ================================

        /// <summary>
        /// Tableau de bord des segments
        /// </summary>
        /// <param name="includeInactive">Inclure les segments inactifs</param>
        /// <param name="startDate">Date de début pour les métriques</param>
        /// <param name="endDate">Date de fin pour les métriques</param>
        /// <returns>Dashboard des segments</returns>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetSegmentsDashboard(
            [FromQuery] bool includeInactive = false,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var period = new { 
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1), 
                EndDate = endDate ?? DateTime.UtcNow 
            };

            // Combinaison de plusieurs requêtes pour créer un dashboard complet
            var segmentsQuery = new GetCustomerSegmentsQuery 
            { 
                ActiveOnly = !includeInactive, 
                IncludeStatistics = true, 
                PageNumber = 1, 
                PageSize = 50 
            };

            var segments = await _mediator.Send(segmentsQuery);

            var dashboard = new
            {
                Period = period,
                TotalSegments = segments.TotalCount,
                ActiveSegments = segments.Items.Count(s => s.IsActive),
                TopSegments = segments.Items.OrderByDescending(s => s.CustomerCount).Take(5),
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Export des données de segmentation
        /// </summary>
        /// <param name="segmentIds">IDs des segments à exporter</param>
        /// <param name="includeCustomers">Inclure les détails clients</param>
        /// <param name="format">Format d'export (csv, excel)</param>
        /// <returns>Fichier d'export</returns>
        [HttpPost("export")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportSegmentData(
            [FromBody] List<Guid>? segmentIds = null,
            [FromQuery] bool includeCustomers = false,
            [FromQuery] string format = "csv")
        {
            if (!new[] { "csv", "excel" }.Contains(format.ToLower()))
            {
                return BadRequest("Format must be 'csv' or 'excel'");
            }

            _logger.LogInformation("Exporting segment data - Segments: {SegmentCount}, Format: {Format}", 
                segmentIds?.Count ?? 0, format);

            // Dans une implémentation réelle, on utiliserait un service d'export
            var fileName = $"segments_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Placeholder pour l'export réel
            var content = "Export functionality to be implemented";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, contentType, fileName);
        }
    }
}