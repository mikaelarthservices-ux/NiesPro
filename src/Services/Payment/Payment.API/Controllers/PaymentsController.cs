using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payment.Application.Commands;
using Payment.Application.Queries;
using Payment.Application.DTOs;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur API pour la gestion des paiements avec conformité PCI-DSS
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Créer un nouveau paiement
    /// </summary>
    /// <param name="command">Données du paiement à créer</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du paiement créé</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PaymentDto>> CreatePayment(
        [FromBody] CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating payment for customer {CustomerId}, amount {Amount}", 
                command.CustomerId, command.Amount);

            // Enrichir la commande avec les données de contexte
            command.IpAddress = GetClientIpAddress();
            command.UserAgent = Request.Headers["User-Agent"].ToString();
            command.SessionId = GetSessionId();

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment created successfully with ID {PaymentId}", result.Id);

            return CreatedAtAction(
                nameof(GetPayment),
                new { id = result.Id },
                result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating payment: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            
            return UnprocessableEntity(new ValidationErrorResponse
            {
                Errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for customer {CustomerId}", command.CustomerId);
            
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiErrorResponse { Message = "Une erreur interne s'est produite" });
        }
    }

    /// <summary>
    /// Traiter un paiement (capture ou annulation)
    /// </summary>
    /// <param name="id">Identifiant du paiement</param>
    /// <param name="command">Commande de traitement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultat du traitement</returns>
    [HttpPost("{id}/process")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> ProcessPayment(
        Guid id,
        [FromBody] ProcessPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            command.PaymentId = id;
            command.IpAddress = GetClientIpAddress();
            command.UserAgent = Request.Headers["User-Agent"].ToString();

            _logger.LogInformation("Processing payment {PaymentId}", id);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} processed successfully", id);

            return Ok(result);
        }
        catch (PaymentNotFoundException)
        {
            return NotFound(new ApiErrorResponse { Message = "Paiement non trouvé" });
        }
        catch (InvalidPaymentStateException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors du traitement du paiement" });
        }
    }

    /// <summary>
    /// Obtenir les détails d'un paiement
    /// </summary>
    /// <param name="id">Identifiant du paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du paiement</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPayment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetPaymentByIdQuery { PaymentId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(new ApiErrorResponse { Message = "Paiement non trouvé" });

            // Vérifier l'autorisation d'accès
            if (!await CanAccessPayment(result))
                return Forbid();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération du paiement" });
        }
    }

    /// <summary>
    /// Obtenir les paiements d'un client
    /// </summary>
    /// <param name="customerId">Identifiant du client</param>
    /// <param name="page">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des paiements</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPaymentsByCustomer(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier l'autorisation d'accès aux données du client
            if (!await CanAccessCustomerData(customerId))
                return Forbid();

            var query = new GetPaymentsByCustomerQuery
            {
                CustomerId = customerId,
                Page = page,
                PageSize = Math.Min(pageSize, 100) // Limiter la taille de page
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for customer {CustomerId}", customerId);
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des paiements" });
        }
    }

    /// <summary>
    /// Rechercher des paiements avec filtres
    /// </summary>
    /// <param name="query">Critères de recherche</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultats de recherche paginés</returns>
    [HttpGet("search")]
    [Authorize(Roles = "Admin,Merchant")]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaymentDto>>> SearchPayments(
        [FromQuery] SearchPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Limiter la recherche au marchand si l'utilisateur n'est pas admin
            if (!User.IsInRole("Admin"))
            {
                query.MerchantId = GetMerchantId();
            }

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching payments");
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la recherche" });
        }
    }

    /// <summary>
    /// Obtenir les statistiques de paiements pour un marchand
    /// </summary>
    /// <param name="merchantId">Identifiant du marchand</param>
    /// <param name="from">Date de début</param>
    /// <param name="to">Date de fin</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Statistiques de paiements</returns>
    [HttpGet("merchant/{merchantId}/stats")]
    [Authorize(Roles = "Admin,Merchant")]
    [ProducesResponseType(typeof(MerchantPaymentStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MerchantPaymentStatsDto>> GetMerchantStats(
        Guid merchantId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Vérifier l'autorisation d'accès aux données du marchand
            if (!User.IsInRole("Admin") && GetMerchantId() != merchantId)
                return Forbid();

            var query = new GetMerchantPaymentStatsQuery
            {
                MerchantId = merchantId,
                FromDate = from ?? DateTime.UtcNow.AddDays(-30),
                ToDate = to ?? DateTime.UtcNow
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchant stats for {MerchantId}", merchantId);
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des statistiques" });
        }
    }

    /// <summary>
    /// Annuler un paiement
    /// </summary>
    /// <param name="id">Identifiant du paiement</param>
    /// <param name="command">Détails de l'annulation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Paiement mis à jour</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> CancelPayment(
        Guid id,
        [FromBody] CancelPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            command.PaymentId = id;
            command.UserId = GetUserId();

            _logger.LogInformation("Cancelling payment {PaymentId}", id);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Payment {PaymentId} cancelled successfully", id);

            return Ok(result);
        }
        catch (PaymentNotFoundException)
        {
            return NotFound(new ApiErrorResponse { Message = "Paiement non trouvé" });
        }
        catch (InvalidPaymentStateException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment {PaymentId}", id);
            
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de l'annulation du paiement" });
        }
    }

    // Méthodes utilitaires privées
    private string GetClientIpAddress()
    {
        return Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string? GetSessionId()
    {
        return HttpContext.Session?.Id;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private Guid? GetMerchantId()
    {
        var merchantIdClaim = User.FindFirst("MerchantId")?.Value;
        return Guid.TryParse(merchantIdClaim, out var merchantId) ? merchantId : null;
    }

    private async Task<bool> CanAccessPayment(PaymentDto payment)
    {
        // Admin peut accéder à tous les paiements
        if (User.IsInRole("Admin"))
            return true;

        // Marchand peut accéder à ses paiements
        if (User.IsInRole("Merchant"))
        {
            var merchantId = GetMerchantId();
            return merchantId.HasValue && payment.MerchantId == merchantId.Value;
        }

        // Client peut accéder à ses propres paiements
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return payment.CustomerId.ToString() == userId;
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

        // Marchand peut accéder aux données de ses clients (à implémenter selon les besoins)
        return false;
    }
}

// DTOs pour les réponses d'erreur
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ValidationErrorResponse
{
    public Dictionary<string, string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Exceptions personnalisées
public class PaymentNotFoundException : Exception
{
    public PaymentNotFoundException(Guid paymentId) 
        : base($"Payment with ID {paymentId} was not found")
    {
    }
}

public class InvalidPaymentStateException : Exception
{
    public InvalidPaymentStateException(string message) : base(message)
    {
    }
}

public class ValidationException : Exception
{
    public List<ValidationError> Errors { get; }

    public ValidationException(List<ValidationError> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}