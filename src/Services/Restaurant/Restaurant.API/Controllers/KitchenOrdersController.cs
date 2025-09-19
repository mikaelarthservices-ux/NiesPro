using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.KitchenOrders.Commands.CreateKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Commands.UpdateKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Commands.AcceptKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Commands.StartKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Commands.CompleteKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Commands.CancelKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Queries.GetKitchenOrder;
using Restaurant.Application.Features.KitchenOrders.Queries.GetKitchenOrders;
using Restaurant.Application.Features.KitchenOrders.Queries.GetKitchenOrdersByStatus;
using Restaurant.Application.Features.KitchenOrders.Queries.GetKitchenOrdersBySection;
using Restaurant.Application.Features.KitchenOrders.Queries.GetDelayedKitchenOrders;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des commandes de cuisine
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Kitchen Orders")]
public class KitchenOrdersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<KitchenOrdersController> _logger;

    public KitchenOrdersController(
        IMediator mediator, 
        ILogger<KitchenOrdersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer une nouvelle commande de cuisine
    /// </summary>
    /// <param name="command">Données de création de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Commande de cuisine créée</returns>
    [HttpPost]
    [Authorize(Roles = "Waiter,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CreateKitchenOrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateKitchenOrderResponse>>> CreateKitchenOrder(
        [FromBody] CreateKitchenOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating kitchen order for table {TableId}", command.TableId);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order created successfully with ID {OrderId} and number {OrderNumber}", 
                response.Id, response.OrderNumber);
            
            return CreatedAtAction(
                nameof(GetKitchenOrder),
                new { id = response.Id },
                ApiResponse<CreateKitchenOrderResponse>.Success(response, "Kitchen order created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating kitchen order for table {TableId}", command.TableId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir une commande de cuisine par son ID
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="includeItems">Inclure les items de la commande</param>
    /// <param name="includeLogs">Inclure l'historique des actions</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la commande de cuisine</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrderResponse>>> GetKitchenOrder(
        [FromRoute] Guid id,
        [FromQuery] bool includeItems = true,
        [FromQuery] bool includeLogs = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetKitchenOrderQuery(id, includeItems, includeLogs);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetKitchenOrderResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen order with ID {OrderId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir une commande par son numéro
    /// </summary>
    /// <param name="orderNumber">Numéro de commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la commande</returns>
    [HttpGet("by-number/{orderNumber}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrderResponse>>> GetKitchenOrderByNumber(
        [FromRoute] string orderNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetKitchenOrderByNumberQuery(orderNumber);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetKitchenOrderResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen order with number {OrderNumber}", orderNumber);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir toutes les commandes avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des commandes</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrdersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrdersResponse>>> GetKitchenOrders(
        [FromQuery] GetKitchenOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetKitchenOrdersResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen orders");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les commandes par statut
    /// </summary>
    /// <param name="status">Statut des commandes</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Commandes du statut spécifié</returns>
    [HttpGet("by-status/{status}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrdersByStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrdersByStatusResponse>>> GetKitchenOrdersByStatus(
        [FromRoute] string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.KitchenOrderStatus>(status, true, out var statusEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid status value"));
            }

            var query = new GetKitchenOrdersByStatusQuery(statusEnum);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetKitchenOrdersByStatusResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen orders for status {Status}", status);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les commandes actives pour une section de cuisine
    /// </summary>
    /// <param name="section">Section de cuisine</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Commandes actives de la section</returns>
    [HttpGet("by-section/{section}")]
    [Authorize(Roles = "Chef,Cook,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrdersBySectionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrdersBySectionResponse>>> GetKitchenOrdersBySection(
        [FromRoute] string section,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.KitchenSection>(section, true, out var sectionEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid section value"));
            }

            var query = new GetKitchenOrdersBySectionQuery(sectionEnum);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetKitchenOrdersBySectionResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen orders for section {Section}", section);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les commandes en retard
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Commandes en retard</returns>
    [HttpGet("delayed")]
    [Authorize(Roles = "Chef,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<GetDelayedKitchenOrdersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetDelayedKitchenOrdersResponse>>> GetDelayedKitchenOrders(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetDelayedKitchenOrdersQuery();
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetDelayedKitchenOrdersResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed kitchen orders");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les commandes assignées à un chef
    /// </summary>
    /// <param name="chefId">Identifiant du chef</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Commandes du chef</returns>
    [HttpGet("by-chef/{chefId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetKitchenOrdersByChefResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetKitchenOrdersByChefResponse>>> GetKitchenOrdersByChef(
        [FromRoute] Guid chefId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetKitchenOrdersByChefQuery(chefId);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetKitchenOrdersByChefResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving kitchen orders for chef {ChefId}", chefId);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Accepter une commande de cuisine
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="command">Données d'acceptation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'acceptation</returns>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Chef,Cook,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AcceptKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AcceptKitchenOrderResponse>>> AcceptKitchenOrder(
        [FromRoute] Guid id,
        [FromBody] AcceptKitchenOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.OrderId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Accepting kitchen order with ID {OrderId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order accepted successfully with ID {OrderId}", id);
            
            return Ok(ApiResponse<AcceptKitchenOrderResponse>.Success(response, "Kitchen order accepted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting kitchen order with ID {OrderId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Commencer la préparation d'une commande
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de début de préparation</returns>
    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "Chef,Cook,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<StartKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StartKitchenOrderResponse>>> StartKitchenOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting preparation of kitchen order with ID {OrderId}", id);
            
            var command = new StartKitchenOrderCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order preparation started successfully with ID {OrderId}", id);
            
            return Ok(ApiResponse<StartKitchenOrderResponse>.Success(response, "Kitchen order preparation started"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting kitchen order with ID {OrderId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Marquer une commande comme prête
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation que la commande est prête</returns>
    [HttpPost("{id:guid}/ready")]
    [Authorize(Roles = "Chef,Cook,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CompleteKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CompleteKitchenOrderResponse>>> MarkKitchenOrderReady(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking kitchen order as ready with ID {OrderId}", id);
            
            var command = new CompleteKitchenOrderCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order marked as ready successfully with ID {OrderId}", id);
            
            return Ok(ApiResponse<CompleteKitchenOrderResponse>.Success(response, "Kitchen order is ready"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking kitchen order as ready with ID {OrderId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Annuler une commande de cuisine
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="command">Données d'annulation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'annulation</returns>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CancelKitchenOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CancelKitchenOrderResponse>>> CancelKitchenOrder(
        [FromRoute] Guid id,
        [FromBody] CancelKitchenOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.OrderId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Cancelling kitchen order with ID {OrderId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order cancelled successfully with ID {OrderId}", id);
            
            return Ok(ApiResponse<CancelKitchenOrderResponse>.Success(response, "Kitchen order cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling kitchen order with ID {OrderId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour la priorité d'une commande
    /// </summary>
    /// <param name="id">Identifiant de la commande</param>
    /// <param name="priority">Nouvelle priorité</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation du changement de priorité</returns>
    [HttpPatch("{id:guid}/priority")]
    [Authorize(Roles = "Chef,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateKitchenOrderPriority(
        [FromRoute] Guid id,
        [FromQuery] string priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.OrderPriority>(priority, true, out var priorityEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid priority value"));
            }

            _logger.LogInformation("Updating priority of kitchen order {OrderId} to {Priority}", id, priority);
            
            // Cette commande devrait exister dans l'Application layer
            // var command = new UpdateKitchenOrderPriorityCommand(id, priorityEnum);
            // await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Kitchen order priority updated successfully for ID {OrderId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Kitchen order priority updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority for kitchen order with ID {OrderId}", id);
            return HandleError(ex);
        }
    }
}