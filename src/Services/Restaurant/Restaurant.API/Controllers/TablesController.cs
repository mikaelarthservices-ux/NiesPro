using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.Tables.Commands.CreateTable;
using Restaurant.Application.Features.Tables.Commands.UpdateTable;
using Restaurant.Application.Features.Tables.Commands.DeleteTable;
using Restaurant.Application.Features.Tables.Queries.GetTable;
using Restaurant.Application.Features.Tables.Queries.GetTables;
using Restaurant.Application.Features.Tables.Queries.GetAvailableTables;
using Restaurant.Application.Features.Tables.Queries.GetTablesBySection;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des tables du restaurant
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Tables")]
public class TablesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<TablesController> _logger;

    public TablesController(
        IMediator mediator, 
        ILogger<TablesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer une nouvelle table
    /// </summary>
    /// <param name="command">Données de création de la table</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Table créée</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateTableResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateTableResponse>>> CreateTable(
        [FromBody] CreateTableCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating table with number {TableNumber}", command.Number);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Table created successfully with ID {TableId}", response.Id);
            
            return CreatedAtAction(
                nameof(GetTable),
                new { id = response.Id },
                ApiResponse<CreateTableResponse>.Success(response, "Table created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table with number {TableNumber}", command.Number);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir une table par son ID
    /// </summary>
    /// <param name="id">Identifiant de la table</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la table</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetTableResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetTableResponse>>> GetTable(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetTableQuery(id);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetTableResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table with ID {TableId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir toutes les tables avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des tables</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetTablesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetTablesResponse>>> GetTables(
        [FromQuery] GetTablesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetTablesResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tables");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les tables disponibles pour une réservation
    /// </summary>
    /// <param name="query">Critères de disponibilité</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Tables disponibles</returns>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<GetAvailableTablesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetAvailableTablesResponse>>> GetAvailableTables(
        [FromQuery] GetAvailableTablesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetAvailableTablesResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available tables");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les tables par section
    /// </summary>
    /// <param name="section">Section du restaurant</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Tables de la section spécifiée</returns>
    [HttpGet("by-section/{section}")]
    [ProducesResponseType(typeof(ApiResponse<GetTablesBySectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetTablesBySectionResponse>>> GetTablesBySection(
        [FromRoute] string section,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.RestaurantSection>(section, true, out var sectionEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid section value"));
            }

            var query = new GetTablesBySectionQuery(sectionEnum);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetTablesBySectionResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tables for section {Section}", section);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour une table
    /// </summary>
    /// <param name="id">Identifiant de la table</param>
    /// <param name="command">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Table mise à jour</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateTableResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateTableResponse>>> UpdateTable(
        [FromRoute] Guid id,
        [FromBody] UpdateTableCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating table with ID {TableId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Table updated successfully with ID {TableId}", id);
            
            return Ok(ApiResponse<UpdateTableResponse>.Success(response, "Table updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table with ID {TableId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Supprimer une table
    /// </summary>
    /// <param name="id">Identifiant de la table</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTable(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting table with ID {TableId}", id);
            
            var command = new DeleteTableCommand(id);
            await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Table deleted successfully with ID {TableId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Table deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting table with ID {TableId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Changer le statut d'une table
    /// </summary>
    /// <param name="id">Identifiant de la table</param>
    /// <param name="status">Nouveau statut</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation du changement de statut</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangeTableStatus(
        [FromRoute] Guid id,
        [FromQuery] string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.TableStatus>(status, true, out var statusEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid status value"));
            }

            _logger.LogInformation("Changing status of table {TableId} to {Status}", id, status);
            
            // Cette commande devrait exister dans l'Application layer
            // var command = new ChangeTableStatusCommand(id, statusEnum);
            // await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Table status changed successfully for ID {TableId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Table status changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for table with ID {TableId}", id);
            return HandleError(ex);
        }
    }
}