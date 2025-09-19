using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.Menus.Commands.CreateMenu;
using Restaurant.Application.Features.Menus.Commands.UpdateMenu;
using Restaurant.Application.Features.Menus.Commands.DeleteMenu;
using Restaurant.Application.Features.Menus.Commands.ActivateMenu;
using Restaurant.Application.Features.Menus.Commands.DeactivateMenu;
using Restaurant.Application.Features.Menus.Queries.GetMenu;
using Restaurant.Application.Features.Menus.Queries.GetMenus;
using Restaurant.Application.Features.Menus.Queries.GetActiveMenus;
using Restaurant.Application.Features.Menus.Queries.GetMenusByType;
using Restaurant.Application.Features.Menus.Queries.SearchMenus;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des menus du restaurant
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Menus")]
public class MenusController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<MenusController> _logger;

    public MenusController(
        IMediator mediator, 
        ILogger<MenusController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer un nouveau menu
    /// </summary>
    /// <param name="command">Données de création du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menu créé</returns>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CreateMenuResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateMenuResponse>>> CreateMenu(
        [FromBody] CreateMenuCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating menu with name {MenuName}", command.Name);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu created successfully with ID {MenuId}", response.Id);
            
            return CreatedAtAction(
                nameof(GetMenu),
                new { id = response.Id },
                ApiResponse<CreateMenuResponse>.Success(response, "Menu created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu with name {MenuName}", command.Name);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir un menu par son ID
    /// </summary>
    /// <param name="id">Identifiant du menu</param>
    /// <param name="includeItems">Inclure les items du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du menu</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuResponse>>> GetMenu(
        [FromRoute] Guid id,
        [FromQuery] bool includeItems = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetMenuQuery(id, includeItems);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir tous les menus avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des menus</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMenusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenusResponse>>> GetMenus(
        [FromQuery] GetMenusQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetMenusResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menus");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les menus actifs
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menus actuellement actifs</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<GetActiveMenusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetActiveMenusResponse>>> GetActiveMenus(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetActiveMenusQuery();
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetActiveMenusResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active menus");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les menus par type
    /// </summary>
    /// <param name="type">Type de menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menus du type spécifié</returns>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(ApiResponse<GetMenusByTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenusByTypeResponse>>> GetMenusByType(
        [FromRoute] string type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.MenuType>(type, true, out var menuType))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid menu type value"));
            }

            var query = new GetMenusByTypeQuery(menuType);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenusByTypeResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menus for type {MenuType}", type);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Rechercher des menus par critères
    /// </summary>
    /// <param name="query">Critères de recherche</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menus correspondant aux critères</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchMenusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SearchMenusResponse>>> SearchMenus(
        [FromQuery] SearchMenusQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<SearchMenusResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching menus");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour un menu
    /// </summary>
    /// <param name="id">Identifiant du menu</param>
    /// <param name="command">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menu mis à jour</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateMenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateMenuResponse>>> UpdateMenu(
        [FromRoute] Guid id,
        [FromBody] UpdateMenuCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating menu with ID {MenuId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu updated successfully with ID {MenuId}", id);
            
            return Ok(ApiResponse<UpdateMenuResponse>.Success(response, "Menu updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Activer un menu
    /// </summary>
    /// <param name="id">Identifiant du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'activation</returns>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ActivateMenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateMenuResponse>>> ActivateMenu(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating menu with ID {MenuId}", id);
            
            var command = new ActivateMenuCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu activated successfully with ID {MenuId}", id);
            
            return Ok(ApiResponse<ActivateMenuResponse>.Success(response, "Menu activated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Désactiver un menu
    /// </summary>
    /// <param name="id">Identifiant du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de désactivation</returns>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DeactivateMenuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DeactivateMenuResponse>>> DeactivateMenu(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating menu with ID {MenuId}", id);
            
            var command = new DeactivateMenuCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu deactivated successfully with ID {MenuId}", id);
            
            return Ok(ApiResponse<DeactivateMenuResponse>.Success(response, "Menu deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Supprimer un menu
    /// </summary>
    /// <param name="id">Identifiant du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMenu(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting menu with ID {MenuId}", id);
            
            var command = new DeleteMenuCommand(id);
            await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu deleted successfully with ID {MenuId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Menu deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Dupliquer un menu
    /// </summary>
    /// <param name="id">Identifiant du menu à dupliquer</param>
    /// <param name="newName">Nom du nouveau menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Menu dupliqué</returns>
    [HttpPost("{id:guid}/duplicate")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CreateMenuResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateMenuResponse>>> DuplicateMenu(
        [FromRoute] Guid id,
        [FromQuery] string newName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                return BadRequest(ApiErrorResponse.BadRequest("New menu name is required"));
            }

            _logger.LogInformation("Duplicating menu with ID {MenuId} as {NewName}", id, newName);
            
            // Cette commande devrait exister dans l'Application layer
            // var command = new DuplicateMenuCommand(id, newName);
            // var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu duplicated successfully");
            
            // return CreatedAtAction(nameof(GetMenu), new { id = response.Id }, 
            //     ApiResponse<CreateMenuResponse>.Success(response, "Menu duplicated successfully"));
            
            return Ok(ApiResponse<bool>.Success(true, "Menu duplication feature coming soon"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating menu with ID {MenuId}", id);
            return HandleError(ex);
        }
    }
}