using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.MenuItems.Commands.CreateMenuItem;
using Restaurant.Application.Features.MenuItems.Commands.UpdateMenuItem;
using Restaurant.Application.Features.MenuItems.Commands.DeleteMenuItem;
using Restaurant.Application.Features.MenuItems.Commands.ActivateMenuItem;
using Restaurant.Application.Features.MenuItems.Commands.DeactivateMenuItem;
using Restaurant.Application.Features.MenuItems.Queries.GetMenuItem;
using Restaurant.Application.Features.MenuItems.Queries.GetMenuItems;
using Restaurant.Application.Features.MenuItems.Queries.GetAvailableMenuItems;
using Restaurant.Application.Features.MenuItems.Queries.GetMenuItemsByCategory;
using Restaurant.Application.Features.MenuItems.Queries.SearchMenuItems;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des items de menu
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Menu Items")]
public class MenuItemsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<MenuItemsController> _logger;

    public MenuItemsController(
        IMediator mediator, 
        ILogger<MenuItemsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer un nouvel item de menu
    /// </summary>
    /// <param name="command">Données de création de l'item</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Item de menu créé</returns>
    [HttpPost]
    [Authorize(Roles = "Manager,Admin,Chef")]
    [ProducesResponseType(typeof(ApiResponse<CreateMenuItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateMenuItemResponse>>> CreateMenuItem(
        [FromBody] CreateMenuItemCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating menu item with name {ItemName}", command.Name);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu item created successfully with ID {ItemId}", response.Id);
            
            return CreatedAtAction(
                nameof(GetMenuItem),
                new { id = response.Id },
                ApiResponse<CreateMenuItemResponse>.Success(response, "Menu item created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item with name {ItemName}", command.Name);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir un item de menu par son ID
    /// </summary>
    /// <param name="id">Identifiant de l'item</param>
    /// <param name="includeVariations">Inclure les variations</param>
    /// <param name="includePromotions">Inclure les promotions</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de l'item de menu</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemResponse>>> GetMenuItem(
        [FromRoute] Guid id,
        [FromQuery] bool includeVariations = false,
        [FromQuery] bool includePromotions = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetMenuItemQuery(id, includeVariations, includePromotions);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuItemResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu item with ID {ItemId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir tous les items de menu avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des items de menu</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemsResponse>>> GetMenuItems(
        [FromQuery] GetMenuItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetMenuItemsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les items de menu disponibles
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items de menu actuellement disponibles</returns>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<GetAvailableMenuItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetAvailableMenuItemsResponse>>> GetAvailableMenuItems(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAvailableMenuItemsQuery();
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetAvailableMenuItemsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available menu items");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les items de menu par catégorie
    /// </summary>
    /// <param name="category">Catégorie des items</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items de la catégorie spécifiée</returns>
    [HttpGet("by-category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemsByCategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemsByCategoryResponse>>> GetMenuItemsByCategory(
        [FromRoute] string category,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.MenuCategory>(category, true, out var categoryEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid category value"));
            }

            var query = new GetMenuItemsByCategoryQuery(categoryEnum);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuItemsByCategoryResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items for category {Category}", category);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les items de menu d'un menu spécifique
    /// </summary>
    /// <param name="menuId">Identifiant du menu</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items du menu spécifié</returns>
    [HttpGet("by-menu/{menuId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemsResponse>>> GetMenuItemsByMenu(
        [FromRoute] Guid menuId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetMenuItemsByMenuQuery(menuId);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuItemsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items for menu {MenuId}", menuId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Rechercher des items de menu par critères
    /// </summary>
    /// <param name="query">Critères de recherche</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items correspondant aux critères</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchMenuItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SearchMenuItemsResponse>>> SearchMenuItems(
        [FromQuery] SearchMenuItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<SearchMenuItemsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching menu items");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les items en promotion
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items actuellement en promotion</returns>
    [HttpGet("on-promotion")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemsOnPromotionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemsOnPromotionResponse>>> GetMenuItemsOnPromotion(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetMenuItemsOnPromotionQuery();
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuItemsOnPromotionResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items on promotion");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Filtrer les items par allergènes
    /// </summary>
    /// <param name="excludedAllergens">Allergènes à exclure (séparés par des virgules)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Items sans les allergènes spécifiés</returns>
    [HttpGet("filter-by-allergens")]
    [ProducesResponseType(typeof(ApiResponse<GetMenuItemsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMenuItemsResponse>>> FilterByAllergens(
        [FromQuery] string excludedAllergens,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(excludedAllergens))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Excluded allergens must be specified"));
            }

            var allergensList = excludedAllergens
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();

            var query = new FilterMenuItemsByAllergensQuery(allergensList);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetMenuItemsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering menu items by allergens");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour un item de menu
    /// </summary>
    /// <param name="id">Identifiant de l'item</param>
    /// <param name="command">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Item de menu mis à jour</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Manager,Admin,Chef")]
    [ProducesResponseType(typeof(ApiResponse<UpdateMenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateMenuItemResponse>>> UpdateMenuItem(
        [FromRoute] Guid id,
        [FromBody] UpdateMenuItemCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating menu item with ID {ItemId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu item updated successfully with ID {ItemId}", id);
            
            return Ok(ApiResponse<UpdateMenuItemResponse>.Success(response, "Menu item updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu item with ID {ItemId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Activer un item de menu
    /// </summary>
    /// <param name="id">Identifiant de l'item</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'activation</returns>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Manager,Admin,Chef")]
    [ProducesResponseType(typeof(ApiResponse<ActivateMenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateMenuItemResponse>>> ActivateMenuItem(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating menu item with ID {ItemId}", id);
            
            var command = new ActivateMenuItemCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu item activated successfully with ID {ItemId}", id);
            
            return Ok(ApiResponse<ActivateMenuItemResponse>.Success(response, "Menu item activated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating menu item with ID {ItemId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Désactiver un item de menu
    /// </summary>
    /// <param name="id">Identifiant de l'item</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de désactivation</returns>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Manager,Admin,Chef")]
    [ProducesResponseType(typeof(ApiResponse<DeactivateMenuItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DeactivateMenuItemResponse>>> DeactivateMenuItem(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating menu item with ID {ItemId}", id);
            
            var command = new DeactivateMenuItemCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu item deactivated successfully with ID {ItemId}", id);
            
            return Ok(ApiResponse<DeactivateMenuItemResponse>.Success(response, "Menu item deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating menu item with ID {ItemId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Supprimer un item de menu
    /// </summary>
    /// <param name="id">Identifiant de l'item</param>
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
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMenuItem(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting menu item with ID {ItemId}", id);
            
            var command = new DeleteMenuItemCommand(id);
            await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Menu item deleted successfully with ID {ItemId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Menu item deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting menu item with ID {ItemId}", id);
            return HandleError(ex);
        }
    }
}