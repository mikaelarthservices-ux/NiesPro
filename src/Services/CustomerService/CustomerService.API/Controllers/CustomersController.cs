using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CustomerService.Application.Commands.Customer;
using CustomerService.Application.Queries.Customer;
using CustomerService.Application.DTOs.Customer;
using Serilog;

namespace CustomerService.API.Controllers
{
    /// <summary>
    /// API Controller pour la gestion des clients - NiesPro Customer Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Créer un nouveau client
        /// </summary>
        /// <param name="command">Données du client à créer</param>
        /// <returns>Client créé</returns>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CustomerDetailDto>> CreateCustomer([FromBody] CreateCustomerCommand command)
        {
            _logger.LogInformation("Creating new customer: {Email}", command.Email);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer created successfully: {CustomerId}", result.Id);
            
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Récupérer un client par son ID
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <param name="includeAnalytics">Inclure les analytics</param>
        /// <param name="includeInteractions">Inclure les interactions</param>
        /// <param name="includePreferences">Inclure les préférences</param>
        /// <returns>Détails du client</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerById(
            Guid id, 
            [FromQuery] bool includeAnalytics = false,
            [FromQuery] bool includeInteractions = false,
            [FromQuery] bool includePreferences = false)
        {
            var query = new GetCustomerByIdQuery
            {
                CustomerId = id,
                IncludeAnalytics = includeAnalytics,
                IncludeInteractions = includeInteractions,
                IncludePreferences = includePreferences
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Récupérer un client par email
        /// </summary>
        /// <param name="email">Email du client</param>
        /// <returns>Détails du client</returns>
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerByEmail(string email)
        {
            var query = new GetCustomerByEmailQuery { Email = email };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Customer with email {email} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Rechercher des clients
        /// </summary>
        /// <param name="searchTerm">Terme de recherche</param>
        /// <param name="status">Statut du client</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <param name="sortBy">Tri par</param>
        /// <param name="sortDescending">Tri descendant</param>
        /// <returns>Liste paginée de clients</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResult<CustomerSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerSummaryDto>>> SearchCustomers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "LastName",
            [FromQuery] bool sortDescending = false)
        {
            var query = new SearchCustomersQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100), // Limite max
                SortBy = sortBy,
                SortDescending = sortDescending,
                IncludeMetrics = true
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour un client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <param name="command">Données de mise à jour</param>
        /// <returns>Client mis à jour</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDetailDto>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating customer: {CustomerId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer updated successfully: {CustomerId}", id);
            
            return Ok(result);
        }

        /// <summary>
        /// Désactiver un client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <param name="reason">Raison de la désactivation</param>
        /// <returns>Confirmation de désactivation</returns>
        [HttpPut("{id:guid}/deactivate")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateCustomer(Guid id, [FromBody] string reason)
        {
            var command = new DeactivateCustomerCommand { CustomerId = id, Reason = reason };
            
            _logger.LogInformation("Deactivating customer: {CustomerId}, Reason: {Reason}", id, reason);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer deactivated successfully: {CustomerId}", id);
            
            return Ok(new { Message = "Customer deactivated successfully", Success = result });
        }

        /// <summary>
        /// Réactiver un client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <returns>Confirmation de réactivation</returns>
        [HttpPut("{id:guid}/reactivate")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReactivateCustomer(Guid id)
        {
            var command = new ReactivateCustomerCommand { CustomerId = id };
            
            _logger.LogInformation("Reactivating customer: {CustomerId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer reactivated successfully: {CustomerId}", id);
            
            return Ok(new { Message = "Customer reactivated successfully", Success = result });
        }

        /// <summary>
        /// Vérifier l'email d'un client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <returns>Confirmation de vérification</returns>
        [HttpPut("{id:guid}/verify-email")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> VerifyCustomerEmail(Guid id)
        {
            var command = new VerifyCustomerEmailCommand { CustomerId = id };
            
            _logger.LogInformation("Verifying customer email: {CustomerId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer email verified successfully: {CustomerId}", id);
            
            return Ok(new { Message = "Email verified successfully", Success = result });
        }

        /// <summary>
        /// Enregistrer une visite client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <param name="command">Données de la visite</param>
        /// <returns>Confirmation d'enregistrement</returns>
        [HttpPost("{id:guid}/visits")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RecordCustomerVisit(Guid id, [FromBody] RecordCustomerVisitCommand command)
        {
            if (id != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Recording customer visit: {CustomerId}", id);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer visit recorded successfully: {CustomerId}", id);
            
            return Ok(new { Message = "Visit recorded successfully", Success = result });
        }

        /// <summary>
        /// Récupérer le profil complet d'un client
        /// </summary>
        /// <param name="id">ID du client</param>
        /// <param name="includeFullHistory">Inclure l'historique complet</param>
        /// <returns>Profil complet du client</returns>
        [HttpGet("{id:guid}/profile")]
        [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerProfileDto>> GetCustomerProfile(Guid id, [FromQuery] bool includeFullHistory = false)
        {
            var query = new GetCustomerProfileQuery
            {
                CustomerId = id,
                IncludeFullHistory = includeFullHistory
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Customer profile with ID {id} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Récupérer les statistiques des clients
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <param name="includeTrends">Inclure les tendances</param>
        /// <param name="topCountries">Nombre de pays top</param>
        /// <returns>Statistiques des clients</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(CustomerStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CustomerStatsDto>> GetCustomerStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeTrends = true,
            [FromQuery] int topCountries = 10)
        {
            var query = new GetCustomerStatsQuery
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow,
                IncludeTrends = includeTrends,
                TopCountries = topCountries
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer les clients avec anniversaires
        /// </summary>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>Liste des clients avec anniversaires</returns>
        [HttpGet("birthdays")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(List<CustomerBirthdayDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CustomerBirthdayDto>>> GetCustomerBirthdays(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = new GetCustomerBirthdaysQuery
            {
                StartDate = startDate ?? DateTime.Today,
                EndDate = endDate ?? DateTime.Today.AddDays(30)
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Récupérer les clients inactifs
        /// </summary>
        /// <param name="inactiveDays">Nombre de jours d'inactivité</param>
        /// <param name="pageNumber">Numéro de page</param>
        /// <param name="pageSize">Taille de page</param>
        /// <returns>Liste paginée des clients inactifs</returns>
        [HttpGet("inactive")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(PagedResult<CustomerSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CustomerSummaryDto>>> GetInactiveCustomers(
            [FromQuery] int inactiveDays = 90,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetInactiveCustomersQuery
            {
                InactiveDays = inactiveDays,
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                SortBy = "LastInteractionDate",
                SortDescending = true
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Vérifier l'unicité d'un email
        /// </summary>
        /// <param name="email">Email à vérifier</param>
        /// <param name="excludeCustomerId">ID du client à exclure</param>
        /// <returns>True si l'email est unique</returns>
        [HttpGet("check-email-uniqueness")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckEmailUniqueness(
            [FromQuery] string email,
            [FromQuery] Guid? excludeCustomerId = null)
        {
            var query = new CheckEmailUniquenessQuery
            {
                Email = email,
                ExcludeCustomerId = excludeCustomerId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}