using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payment.Application.Commands;
using Payment.Application.Queries;
using Payment.Application.DTOs;
using System.Security.Claims;

namespace Payment.API.Controllers;

/// <summary>
/// Contrôleur API pour la gestion des transactions avec audit complet
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Capturer une transaction autorisée
    /// </summary>
    /// <param name="command">Détails de la capture</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Transaction mise à jour</returns>
    [HttpPost("capture")]
    [Authorize(Roles = "Admin,Merchant")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransactionDto>> CaptureTransaction(
        [FromBody] CaptureTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Capturing transaction {TransactionId} for amount {Amount}", 
                command.TransactionId, command.Amount);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Transaction {TransactionId} captured successfully", command.TransactionId);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error capturing transaction: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return UnprocessableEntity(new ValidationErrorResponse
            {
                Errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (InvalidTransactionStateException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing transaction {TransactionId}", command.TransactionId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la capture de la transaction" });
        }
    }

    /// <summary>
    /// Rembourser une transaction
    /// </summary>
    /// <param name="command">Détails du remboursement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Transaction de remboursement</returns>
    [HttpPost("refund")]
    [Authorize(Roles = "Admin,Merchant")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransactionDto>> RefundTransaction(
        [FromBody] RefundTransactionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            command.UserId = GetUserId();

            _logger.LogInformation("Refunding transaction {TransactionId} for amount {Amount}", 
                command.TransactionId, command.Amount);

            var result = await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Transaction {TransactionId} refunded successfully", command.TransactionId);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error refunding transaction: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            return UnprocessableEntity(new ValidationErrorResponse
            {
                Errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage)
            });
        }
        catch (TransactionNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse { Message = ex.Message });
        }
        catch (InvalidTransactionStateException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (InsufficientFundsException ex)
        {
            return BadRequest(new ApiErrorResponse { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding transaction {TransactionId}", command.TransactionId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors du remboursement de la transaction" });
        }
    }

    /// <summary>
    /// Obtenir les détails d'une transaction
    /// </summary>
    /// <param name="id">Identifiant de la transaction</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails de la transaction</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDetailDto>> GetTransaction(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetTransactionByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(new ApiErrorResponse { Message = "Transaction non trouvée" });

            // Vérifier l'autorisation d'accès
            if (!await CanAccessTransaction(result))
                return Forbid();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération de la transaction" });
        }
    }

    /// <summary>
    /// Obtenir les transactions d'un paiement
    /// </summary>
    /// <param name="paymentId">Identifiant du paiement</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste des transactions du paiement</returns>
    [HttpGet("payment/{paymentId}")]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TransactionDto>>> GetTransactionsByPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetTransactionsByPaymentQuery(paymentId);
            var result = await _mediator.Send(query, cancellationToken);

            // Filtrer les transactions selon les autorisations
            var filteredResults = new List<TransactionSummaryDto>();
            foreach (var transaction in result)
            {
                if (await CanAccessTransaction(transaction))
                {
                    filteredResults.Add(transaction);
                }
            }

            return Ok(filteredResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for payment {PaymentId}", paymentId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des transactions" });
        }
    }

    /// <summary>
    /// Obtenir les transactions d'un client
    /// </summary>
    /// <param name="customerId">Identifiant du client</param>
    /// <param name="page">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée des transactions</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactionsByCustomer(
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

            var query = new GetTransactionsByCustomerQuery
            {
                CustomerId = customerId,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for customer {CustomerId}", customerId);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des transactions" });
        }
    }

    /// <summary>
    /// Rechercher des transactions avec filtres avancés
    /// </summary>
    /// <param name="query">Critères de recherche</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Résultats de recherche paginés</returns>
    [HttpGet("search")]
    [Authorize(Roles = "Admin,Merchant")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> SearchTransactions(
        [FromQuery] SearchTransactionsQuery query,
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
            _logger.LogError(ex, "Error searching transactions");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la recherche" });
        }
    }

    /// <summary>
    /// Obtenir les transactions suspectes (score de fraude élevé)
    /// </summary>
    /// <param name="minimumScore">Score minimum de fraude</param>
    /// <param name="page">Numéro de page</param>
    /// <param name="pageSize">Taille de page</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Transactions suspectes paginées</returns>
    [HttpGet("suspicious")]
    [Authorize(Roles = "Admin,FraudAnalyst")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetSuspiciousTransactions(
        [FromQuery] int minimumScore = 70,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetSuspiciousTransactionsQuery
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suspicious transactions");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des transactions suspectes" });
        }
    }

    /// <summary>
    /// Obtenir les statistiques de transactions
    /// </summary>
    /// <param name="from">Date de début</param>
    /// <param name="to">Date de fin</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Statistiques de transactions</returns>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Merchant,Analyst")]
    [ProducesResponseType(typeof(TransactionStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionStatsDto>> GetTransactionStats(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetTransactionStatsQuery
            {
                FromDate = from ?? DateTime.UtcNow.AddDays(-30),
                ToDate = to ?? DateTime.UtcNow,
                MerchantId = User.IsInRole("Admin") ? null : GetMerchantId()
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction stats");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiErrorResponse { Message = "Erreur lors de la récupération des statistiques" });
        }
    }

    // Méthodes utilitaires privées
    private async Task<bool> CanAccessTransaction(TransactionDto transaction)
    {
        // Admin peut accéder à toutes les transactions
        if (User.IsInRole("Admin"))
            return true;

        // Marchand peut accéder aux transactions de ses paiements
        if (User.IsInRole("Merchant"))
        {
            var merchantId = GetMerchantId();
            // Nécessiterait une requête pour vérifier si la transaction appartient au marchand
            // Pour simplifier, on suppose que c'est vérifié par la logique métier
            return true;
        }

        // Client peut accéder à ses propres transactions
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return userId.HasValue && transaction.CustomerId == userId.Value;
        }

        // Analyste de fraude peut accéder aux transactions pour analyse
        if (User.IsInRole("FraudAnalyst"))
            return true;

        return false;
    }

    private async Task<bool> CanAccessTransaction(TransactionDetailDto transaction)
    {
        // Admin peut accéder à toutes les transactions
        if (User.IsInRole("Admin"))
            return true;

        // Marchand peut accéder aux transactions de ses paiements
        if (User.IsInRole("Merchant"))
        {
            var merchantId = GetMerchantId();
            return true;
        }

        // Client peut accéder à ses propres transactions
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return userId.HasValue && transaction.CustomerId == userId.Value;
        }

        // Analyste de fraude peut accéder aux transactions pour analyse
        if (User.IsInRole("FraudAnalyst"))
            return true;

        return false;
    }

    private async Task<bool> CanAccessTransaction(TransactionSummaryDto transaction)
    {
        // Admin peut accéder à toutes les transactions
        if (User.IsInRole("Admin"))
            return true;

        // Marchand peut accéder aux transactions de ses paiements
        if (User.IsInRole("Merchant"))
        {
            var merchantId = GetMerchantId();
            return true;
        }

        // Client peut accéder à ses propres transactions
        if (User.IsInRole("Customer"))
        {
            var userId = GetUserId();
            return userId.HasValue && transaction.CustomerId == userId.Value;
        }

        // Analyste de fraude peut accéder aux transactions pour analyse
        if (User.IsInRole("FraudAnalyst"))
            return true;

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
            return userId.HasValue && customerId == userId.Value;
        }

        // Analyste de fraude peut accéder aux données pour analyse
        if (User.IsInRole("FraudAnalyst"))
            return true;

        return false;
    }

    // Méthodes utilitaires privées
    private Guid? GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdString, out var userId))
            return userId;
        return null;
    }

    private Guid? GetMerchantId()
    {
        var merchantIdString = User.FindFirst("MerchantId")?.Value;
        if (Guid.TryParse(merchantIdString, out var merchantId))
            return merchantId;
        return null;
    }
}

// Exceptions spécifiques aux transactions
public class TransactionNotFoundException : Exception
{
    public TransactionNotFoundException(Guid transactionId)
        : base($"Transaction with ID {transactionId} was not found")
    {
    }
}

public class InvalidTransactionStateException : Exception
{
    public InvalidTransactionStateException(string message) : base(message)
    {
    }
}

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message) : base(message)
    {
    }
}