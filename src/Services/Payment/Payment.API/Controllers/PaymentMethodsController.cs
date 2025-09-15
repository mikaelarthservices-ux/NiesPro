using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payment.Application.Commands;
using Payment.Application.Queries;
using Payment.Application.DTOs;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur API pour la gestion des moyens de paiement avec sécurité PCI-DSS
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class PaymentMethodsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentMethodsController> _logger;

    public PaymentMethodsController(IMediator mediator, ILogger<PaymentMethodsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Créer un nouveau moyen de paiement (carte)
    /// </summary>
    /// <param name="command">Données du moyen de paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Moyen de paiement créé (sans données sensibles)</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier que l'utilisateur peut créer des moyens de paiement pour ce client
            if (!await CanManageCustomerPaymentMethods(command.CustomerId))
                return Forbid();

            _logger.LogInformation("Creating payment method for customer {CustomerId}", command.CustomerId);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment method created successfully with ID {PaymentMethodId}", result.Id);

            return CreatedAtAction(
                nameof(GetPaymentMethod),
                new { id = result.Id },
                result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating payment method: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return UnprocessableEntity(new ValidationErrorResponse
            {
                Errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method for customer {CustomerId}", command.CustomerId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Une erreur interne s'est produite" });
        }
    }

    /// <summary>
    /// Obtenir les détails d'un moyen de paiement
    /// </summary>
    /// <param name="id">Identifiant du moyen de paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du moyen de paiement (sans données sensibles)</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPaymentMethodByIdQuery { PaymentMethodId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(new ApiErrorResponse { Message = "Moyen de paiement non trouvé" });

            // Vérifier l'autorisation d'accès
            if (!await CanAccessPaymentMethod(result))
                return Forbid();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment method {PaymentMethodId}", id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération du moyen de paiement" });
        }
    }

    /// <summary>
    /// Obtenir les moyens de paiement d'un client
    /// </summary>
    /// <param name="customerId">Identifiant du client</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des moyens de paiement du client</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetPaymentMethodsByCustomer(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier l'autorisation d'accès aux données du client
            if (!await CanAccessCustomerData(customerId))
                return Forbid();

            var query = new GetPaymentMethodsByCustomerQuery { CustomerId = customerId };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods for customer {CustomerId}", customerId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des moyens de paiement" });
        }
    }

    /// <summary>
    /// Définir un moyen de paiement comme défaut
    /// </summary>
    /// <param name="id">Identifiant du moyen de paiement</param>
    /// <param name="command">Commande de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Moyen de paiement mis à jour</returns>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(typeof(PaymentMethodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentMethodDto>> SetDefaultPaymentMethod(
        Guid id,
        [FromBody] SetDefaultPaymentMethodCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            command.PaymentMethodId = id;

            // Récupérer le moyen de paiement pour vérifier l'autorisation
            var getQuery = new GetPaymentMethodByIdQuery { PaymentMethodId = id };
            var paymentMethod = await _mediator.Send(getQuery, cancellationToken);

            if (paymentMethod == null)
                return NotFound(new ApiErrorResponse { Message = "Moyen de paiement non trouvé" });

            if (!await CanManageCustomerPaymentMethods(paymentMethod.CustomerId))
                return Forbid();

            _logger.LogInformation("Setting payment method {PaymentMethodId} as default", id);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} set as default successfully", id);

            return Ok(result);
        }
        catch (PaymentMethodNotFoundException)
        {
            return NotFound(new ApiErrorResponse { Message = "Moyen de paiement non trouvé" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting payment method {PaymentMethodId} as default", id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la mise à jour du moyen de paiement" });
        }
    }

    /// <summary>
    /// Supprimer un moyen de paiement
    /// </summary>
    /// <param name="id">Identifiant du moyen de paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de suppression</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePaymentMethod(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Récupérer le moyen de paiement pour vérifier l'autorisation
            var getQuery = new GetPaymentMethodByIdQuery { PaymentMethodId = id };
            var paymentMethod = await _mediator.Send(getQuery, cancellationToken);

            if (paymentMethod == null)
                return NotFound(new ApiErrorResponse { Message = "Moyen de paiement non trouvé" });

            if (!await CanManageCustomerPaymentMethods(paymentMethod.CustomerId))
                return Forbid();

            var command = new DeletePaymentMethodCommand { PaymentMethodId = id };

            _logger.LogInformation("Deleting payment method {PaymentMethodId}", id);

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment method {PaymentMethodId} deleted successfully", id);

            return NoContent();
        }
        catch (PaymentMethodNotFoundException)
        {
            return NotFound(new ApiErrorResponse { Message = "Moyen de paiement non trouvé" });
        }
        catch (PaymentMethodInUseException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method {PaymentMethodId}", id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la suppression du moyen de paiement" });
        }
    }

    /// <summary>
    /// Obtenir les statistiques des moyens de paiement pour un client
    /// </summary>
    /// <param name="customerId">Identifiant du client</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Statistiques des moyens de paiement</returns>
    [HttpGet("customer/{customerId}/stats")]
    [ProducesResponseType(typeof(PaymentMethodStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaymentMethodStatsDto>> GetPaymentMethodStats(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier l'autorisation d'accès aux données du client
            if (!await CanAccessCustomerData(customerId))
                return Forbid();

            var query = new GetPaymentMethodStatsQuery { CustomerId = customerId };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment method stats for customer {CustomerId}", customerId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des statistiques" });
        }
    }

    // Méthodes utilitaires privées
    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private async Task<bool> CanAccessPaymentMethod(PaymentMethodDto paymentMethod)
    {
        // Admin peut accéder à tous les moyens de paiement
        if (User.IsInRole("Admin"))
            return true;

        // Client peut accéder à ses propres moyens de paiement
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return paymentMethod.CustomerId.ToString() == userId;
        }

        return false;
    }

    private async Task<bool> CanAccessCustomerData(Guid customerId)
    {
        // Admin peut accéder à toutes les données
        if (User.IsInRole("Admin"))
            return true;

        // Client peut accéder à ses propres données
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return customerId.ToString() == userId;
        }

        return false;
    }

    private async Task<bool> CanManageCustomerPaymentMethods(Guid customerId)
    {
        // Admin peut gérer tous les moyens de paiement
        if (User.IsInRole("Admin"))
            return true;

        // Client peut gérer ses propres moyens de paiement
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return customerId.ToString() == userId;
        }

        return false;
    }
}

// Exceptions spécifiques aux moyens de paiement
public class PaymentMethodNotFoundException : Exception
{
    public PaymentMethodNotFoundException(Guid paymentMethodId)
        : base($"Payment method with ID {paymentMethodId} was not found")
    {
    }
}

public class PaymentMethodInUseException : Exception
{
    public PaymentMethodInUseException(string message) : base(message)
    {
    }
}