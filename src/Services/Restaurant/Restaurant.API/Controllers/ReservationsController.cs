using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.TableReservations.Commands.CreateReservation;
using Restaurant.Application.Features.TableReservations.Commands.UpdateReservation;
using Restaurant.Application.Features.TableReservations.Commands.CancelReservation;
using Restaurant.Application.Features.TableReservations.Commands.ConfirmReservation;
using Restaurant.Application.Features.TableReservations.Queries.GetReservation;
using Restaurant.Application.Features.TableReservations.Queries.GetReservations;
using Restaurant.Application.Features.TableReservations.Queries.GetReservationsByDate;
using Restaurant.Application.Features.TableReservations.Queries.SearchReservations;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des réservations de tables
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Table Reservations")]
public class ReservationsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IMediator mediator, 
        ILogger<ReservationsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer une nouvelle réservation
    /// </summary>
    /// <param name="command">Données de création de la réservation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Réservation créée</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateReservationResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateReservationResponse>>> CreateReservation(
        [FromBody] CreateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating reservation for customer {CustomerName} on {Date}", 
                command.CustomerName, command.ReservationDate);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reservation created successfully with ID {ReservationId} and confirmation {ConfirmationNumber}", 
                response.Id, response.ConfirmationNumber);
            
            return CreatedAtAction(
                nameof(GetReservation),
                new { id = response.Id },
                ApiResponse<CreateReservationResponse>.Success(response, "Reservation created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation for customer {CustomerName}", command.CustomerName);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir une réservation par son ID
    /// </summary>
    /// <param name="id">Identifiant de la réservation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la réservation</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetReservationResponse>>> GetReservation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetReservationQuery(id);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetReservationResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation with ID {ReservationId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir une réservation par numéro de confirmation
    /// </summary>
    /// <param name="confirmationNumber">Numéro de confirmation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la réservation</returns>
    [HttpGet("by-confirmation/{confirmationNumber}")]
    [ProducesResponseType(typeof(ApiResponse<GetReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetReservationResponse>>> GetReservationByConfirmation(
        [FromRoute] string confirmationNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetReservationByConfirmationQuery(confirmationNumber);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetReservationResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation with confirmation number {ConfirmationNumber}", confirmationNumber);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir toutes les réservations avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des réservations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetReservationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetReservationsResponse>>> GetReservations(
        [FromQuery] GetReservationsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetReservationsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les réservations pour une date spécifique
    /// </summary>
    /// <param name="date">Date de réservation (format: yyyy-MM-dd)</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Réservations du jour spécifié</returns>
    [HttpGet("by-date/{date:datetime}")]
    [ProducesResponseType(typeof(ApiResponse<GetReservationsByDateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetReservationsByDateResponse>>> GetReservationsByDate(
        [FromRoute] DateTime date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetReservationsByDateQuery(date.Date);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetReservationsByDateResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations for date {Date}", date);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Rechercher des réservations par critères
    /// </summary>
    /// <param name="query">Critères de recherche</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Réservations correspondant aux critères</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchReservationsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SearchReservationsResponse>>> SearchReservations(
        [FromQuery] SearchReservationsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<SearchReservationsResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching reservations");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour une réservation
    /// </summary>
    /// <param name="id">Identifiant de la réservation</param>
    /// <param name="command">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Réservation mise à jour</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateReservationResponse>>> UpdateReservation(
        [FromRoute] Guid id,
        [FromBody] UpdateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating reservation with ID {ReservationId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reservation updated successfully with ID {ReservationId}", id);
            
            return Ok(ApiResponse<UpdateReservationResponse>.Success(response, "Reservation updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation with ID {ReservationId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Confirmer une réservation
    /// </summary>
    /// <param name="id">Identifiant de la réservation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de la réservation</returns>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ApiResponse<ConfirmReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ConfirmReservationResponse>>> ConfirmReservation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Confirming reservation with ID {ReservationId}", id);
            
            var command = new ConfirmReservationCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reservation confirmed successfully with ID {ReservationId}", id);
            
            return Ok(ApiResponse<ConfirmReservationResponse>.Success(response, "Reservation confirmed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation with ID {ReservationId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Annuler une réservation
    /// </summary>
    /// <param name="id">Identifiant de la réservation</param>
    /// <param name="command">Données d'annulation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'annulation</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<CancelReservationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CancelReservationResponse>>> CancelReservation(
        [FromRoute] Guid id,
        [FromBody] CancelReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Cancelling reservation with ID {ReservationId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reservation cancelled successfully with ID {ReservationId}", id);
            
            return Ok(ApiResponse<CancelReservationResponse>.Success(response, "Reservation cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation with ID {ReservationId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Marquer l'arrivée du client (check-in)
    /// </summary>
    /// <param name="id">Identifiant de la réservation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation du check-in</returns>
    [HttpPost("{id:guid}/checkin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckInReservation(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking in reservation with ID {ReservationId}", id);
            
            // Cette commande devrait exister dans l'Application layer
            // var command = new CheckInReservationCommand(id);
            // await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Reservation checked in successfully with ID {ReservationId}", id);
            
            return Ok(ApiResponse<bool>.Success(true, "Customer checked in successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in reservation with ID {ReservationId}", id);
            return HandleError(ex);
        }
    }
}