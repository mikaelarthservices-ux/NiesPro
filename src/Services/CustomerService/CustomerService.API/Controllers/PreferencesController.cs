using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using CustomerService.Application.Commands.Preferences;
using CustomerService.Application.Queries.Preferences;
using CustomerService.Application.DTOs.Preferences;
using Serilog;

namespace CustomerService.API.Controllers
{
    /// <summary>
    /// API Controller pour la gestion des préférences clients - NiesPro Customer Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PreferencesController> _logger;

        public PreferencesController(IMediator mediator, ILogger<PreferencesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // ================================
        // GESTION DES PRÉFÉRENCES GÉNÉRALES
        // ================================

        /// <summary>
        /// Récupérer les préférences d'un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="includeInactive">Inclure les préférences inactives</param>
        /// <param name="category">Catégorie de préférences</param>
        /// <returns>Préférences du client</returns>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(CustomerPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerPreferencesDto>> GetCustomerPreferences(
            Guid customerId,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? category = null)
        {
            var query = new GetCustomerPreferencesQuery
            {
                CustomerId = customerId,
                IncludeInactive = includeInactive,
                Category = category
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Customer with ID {customerId} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour les préférences d'un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Données de mise à jour</param>
        /// <returns>Préférences mises à jour</returns>
        [HttpPut("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(CustomerPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerPreferencesDto>> UpdateCustomerPreferences(
            Guid customerId, 
            [FromBody] UpdateCustomerPreferencesCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating preferences for customer: {CustomerId}", customerId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer preferences updated successfully: {CustomerId}", customerId);
            
            return Ok(result);
        }

        // ================================
        // PRÉFÉRENCES DE COMMUNICATION
        // ================================

        /// <summary>
        /// Récupérer les préférences de communication
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <returns>Préférences de communication</returns>
        [HttpGet("customer/{customerId:guid}/communication")]
        [ProducesResponseType(typeof(CommunicationPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommunicationPreferencesDto>> GetCommunicationPreferences(Guid customerId)
        {
            var query = new GetCommunicationPreferencesQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Communication preferences for customer {customerId} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour les préférences de communication
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Nouvelles préférences de communication</param>
        /// <returns>Préférences mises à jour</returns>
        [HttpPut("customer/{customerId:guid}/communication")]
        [ProducesResponseType(typeof(CommunicationPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommunicationPreferencesDto>> UpdateCommunicationPreferences(
            Guid customerId, 
            [FromBody] UpdateCommunicationPreferencesCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating communication preferences for customer: {CustomerId}", customerId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Communication preferences updated successfully: {CustomerId}", customerId);
            
            return Ok(result);
        }

        // ================================
        // PRÉFÉRENCES ALIMENTAIRES
        // ================================

        /// <summary>
        /// Récupérer les préférences alimentaires
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="includeAllergies">Inclure les allergies</param>
        /// <param name="includeRestrictions">Inclure les restrictions</param>
        /// <returns>Préférences alimentaires</returns>
        [HttpGet("customer/{customerId:guid}/dietary")]
        [ProducesResponseType(typeof(DietaryPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DietaryPreferencesDto>> GetDietaryPreferences(
            Guid customerId,
            [FromQuery] bool includeAllergies = true,
            [FromQuery] bool includeRestrictions = true)
        {
            var query = new GetDietaryPreferencesQuery 
            { 
                CustomerId = customerId,
                IncludeAllergies = includeAllergies,
                IncludeRestrictions = includeRestrictions
            };
            
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Dietary preferences for customer {customerId} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour les préférences alimentaires
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Nouvelles préférences alimentaires</param>
        /// <returns>Préférences mises à jour</returns>
        [HttpPut("customer/{customerId:guid}/dietary")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(DietaryPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DietaryPreferencesDto>> UpdateDietaryPreferences(
            Guid customerId, 
            [FromBody] UpdateDietaryPreferencesCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating dietary preferences for customer: {CustomerId}", customerId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Dietary preferences updated successfully: {CustomerId}", customerId);
            
            return Ok(result);
        }

        /// <summary>
        /// Ajouter une allergie
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Données de l'allergie</param>
        /// <returns>Allergie ajoutée</returns>
        [HttpPost("customer/{customerId:guid}/allergies")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(typeof(CustomerAllergyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerAllergyDto>> AddCustomerAllergy(
            Guid customerId, 
            [FromBody] AddCustomerAllergyCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Adding allergy for customer: {CustomerId}, Allergen: {Allergen}", 
                customerId, command.Allergen);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Allergy added successfully for customer: {CustomerId}", customerId);
            
            return CreatedAtAction(nameof(GetDietaryPreferences), new { customerId }, result);
        }

        /// <summary>
        /// Supprimer une allergie
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="allergyId">ID de l'allergie</param>
        /// <returns>Confirmation de suppression</returns>
        [HttpDelete("customer/{customerId:guid}/allergies/{allergyId:guid}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RemoveCustomerAllergy(Guid customerId, Guid allergyId)
        {
            _logger.LogInformation("Removing allergy {AllergyId} for customer: {CustomerId}", allergyId, customerId);
            
            var command = new RemoveCustomerAllergyCommand 
            { 
                CustomerId = customerId,
                AllergyId = allergyId
            };
            
            await _mediator.Send(command);
            
            _logger.LogInformation("Allergy removed successfully for customer: {CustomerId}", customerId);
            
            return NoContent();
        }

        // ================================
        // PRÉFÉRENCES DE MARKETING
        // ================================

        /// <summary>
        /// Récupérer les préférences marketing
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <returns>Préférences marketing</returns>
        [HttpGet("customer/{customerId:guid}/marketing")]
        [ProducesResponseType(typeof(MarketingPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MarketingPreferencesDto>> GetMarketingPreferences(Guid customerId)
        {
            var query = new GetMarketingPreferencesQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Marketing preferences for customer {customerId} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Mettre à jour les préférences marketing
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Nouvelles préférences marketing</param>
        /// <returns>Préférences mises à jour</returns>
        [HttpPut("customer/{customerId:guid}/marketing")]
        [ProducesResponseType(typeof(MarketingPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MarketingPreferencesDto>> UpdateMarketingPreferences(
            Guid customerId, 
            [FromBody] UpdateMarketingPreferencesCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Updating marketing preferences for customer: {CustomerId}", customerId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Marketing preferences updated successfully: {CustomerId}", customerId);
            
            return Ok(result);
        }

        /// <summary>
        /// Opt-in/Opt-out marketing
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Commande d'opt-in/out</param>
        /// <returns>Préférences mises à jour</returns>
        [HttpPost("customer/{customerId:guid}/marketing/opt")]
        [ProducesResponseType(typeof(MarketingPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MarketingPreferencesDto>> OptInOutMarketing(
            Guid customerId, 
            [FromBody] OptInOutMarketingCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Processing marketing opt-{Action} for customer: {CustomerId}", 
                command.OptIn ? "in" : "out", customerId);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Marketing opt-{Action} processed successfully for customer: {CustomerId}", 
                command.OptIn ? "in" : "out", customerId);
            
            return Ok(result);
        }

        // ================================
        // GESTION DES CONSENTEMENTS
        // ================================

        /// <summary>
        /// Récupérer les consentements d'un client
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="consentType">Type de consentement</param>
        /// <param name="activeOnly">Consentements actifs seulement</param>
        /// <returns>Liste des consentements</returns>
        [HttpGet("customer/{customerId:guid}/consents")]
        [ProducesResponseType(typeof(List<CustomerConsentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CustomerConsentDto>>> GetCustomerConsents(
            Guid customerId,
            [FromQuery] string? consentType = null,
            [FromQuery] bool activeOnly = true)
        {
            var query = new GetCustomerConsentsQuery 
            { 
                CustomerId = customerId,
                ConsentType = consentType,
                ActiveOnly = activeOnly
            };
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Enregistrer un consentement
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="command">Données du consentement</param>
        /// <returns>Consentement enregistré</returns>
        [HttpPost("customer/{customerId:guid}/consents")]
        [ProducesResponseType(typeof(CustomerConsentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerConsentDto>> RecordCustomerConsent(
            Guid customerId, 
            [FromBody] RecordCustomerConsentCommand command)
        {
            if (customerId != command.CustomerId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            _logger.LogInformation("Recording consent for customer: {CustomerId}, Type: {ConsentType}", 
                customerId, command.ConsentType);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Consent recorded successfully for customer: {CustomerId}", customerId);
            
            return CreatedAtAction(nameof(GetCustomerConsents), new { customerId }, result);
        }

        /// <summary>
        /// Révoquer un consentement
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="consentId">ID du consentement</param>
        /// <param name="reason">Raison de révocation</param>
        /// <returns>Confirmation de révocation</returns>
        [HttpDelete("customer/{customerId:guid}/consents/{consentId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RevokeCustomerConsent(
            Guid customerId, 
            Guid consentId,
            [FromQuery] string? reason = null)
        {
            _logger.LogInformation("Revoking consent {ConsentId} for customer: {CustomerId}", consentId, customerId);
            
            var command = new RevokeCustomerConsentCommand 
            { 
                CustomerId = customerId,
                ConsentId = consentId,
                Reason = reason ?? "Revoked by user"
            };
            
            await _mediator.Send(command);
            
            _logger.LogInformation("Consent revoked successfully for customer: {CustomerId}", customerId);
            
            return NoContent();
        }

        // ================================
        // ANALYTICS ET RAPPORTS
        // ================================

        /// <summary>
        /// Statistiques des préférences
        /// </summary>
        /// <param name="category">Catégorie de préférences</param>
        /// <param name="segmentId">ID du segment (optionnel)</param>
        /// <param name="startDate">Date de début</param>
        /// <param name="endDate">Date de fin</param>
        /// <returns>Statistiques des préférences</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(PreferenceStatisticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<PreferenceStatisticsDto>> GetPreferenceStatistics(
            [FromQuery] string? category = null,
            [FromQuery] Guid? segmentId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = new GetPreferenceStatisticsQuery
            {
                Category = category,
                SegmentId = segmentId,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-3),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Analyse des tendances de préférences
        /// </summary>
        /// <param name="timeframe">Période d'analyse (month, quarter, year)</param>
        /// <param name="includeSegmentation">Inclure la segmentation</param>
        /// <param name="includePredictions">Inclure les prédictions</param>
        /// <returns>Analyse des tendances</returns>
        [HttpGet("analytics/trends")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(PreferenceTrendsAnalysisDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<PreferenceTrendsAnalysisDto>> GetPreferenceTrends(
            [FromQuery] string timeframe = "quarter",
            [FromQuery] bool includeSegmentation = true,
            [FromQuery] bool includePredictions = false)
        {
            var query = new GetPreferenceTrendsQuery
            {
                Timeframe = timeframe,
                IncludeSegmentation = includeSegmentation,
                IncludePredictions = includePredictions
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Recommandations personnalisées basées sur les préférences
        /// </summary>
        /// <param name="customerId">ID du client</param>
        /// <param name="category">Catégorie de recommandations</param>
        /// <param name="maxRecommendations">Nombre maximum de recommandations</param>
        /// <param name="includeExplanations">Inclure les explications</param>
        /// <returns>Recommandations personnalisées</returns>
        [HttpGet("customer/{customerId:guid}/recommendations")]
        [ProducesResponseType(typeof(List<PersonalizedRecommendationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PersonalizedRecommendationDto>>> GetPersonalizedRecommendations(
            Guid customerId,
            [FromQuery] string? category = null,
            [FromQuery] int maxRecommendations = 10,
            [FromQuery] bool includeExplanations = true)
        {
            var query = new GetPersonalizedRecommendationsQuery
            {
                CustomerId = customerId,
                Category = category,
                MaxRecommendations = Math.Min(maxRecommendations, 20),
                IncludeExplanations = includeExplanations
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // ================================
        // CONFIGURATION ET EXPORTS
        // ================================

        /// <summary>
        /// Récupérer les catégories de préférences disponibles
        /// </summary>
        /// <returns>Liste des catégories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<PreferenceCategoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PreferenceCategoryDto>>> GetPreferenceCategories()
        {
            var query = new GetPreferenceCategoriesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Export des préférences clients
        /// </summary>
        /// <param name="category">Catégorie à exporter</param>
        /// <param name="segmentId">Segment spécifique</param>
        /// <param name="includePersonalData">Inclure les données personnelles</param>
        /// <param name="format">Format d'export (csv, excel)</param>
        /// <returns>Fichier d'export</returns>
        [HttpGet("export")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportPreferences(
            [FromQuery] string? category = null,
            [FromQuery] Guid? segmentId = null,
            [FromQuery] bool includePersonalData = false,
            [FromQuery] string format = "csv")
        {
            if (!new[] { "csv", "excel" }.Contains(format.ToLower()))
            {
                return BadRequest("Format must be 'csv' or 'excel'");
            }

            _logger.LogInformation("Exporting preferences - Category: {Category}, Format: {Format}", category, format);

            // Dans une implémentation réelle, on utiliserait un service d'export
            var fileName = $"preferences_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Placeholder pour l'export réel
            var content = "Export functionality to be implemented";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, contentType, fileName);
        }
    }
}