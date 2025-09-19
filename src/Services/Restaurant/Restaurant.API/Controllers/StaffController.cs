using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Application.Features.Staff.Commands.CreateStaffMember;
using Restaurant.Application.Features.Staff.Commands.UpdateStaffMember;
using Restaurant.Application.Features.Staff.Commands.DeactivateStaffMember;
using Restaurant.Application.Features.Staff.Commands.ActivateStaffMember;
using Restaurant.Application.Features.Staff.Commands.UpdateStaffRole;
using Restaurant.Application.Features.Staff.Commands.UpdateStaffShift;
using Restaurant.Application.Features.Staff.Commands.RecordStaffTraining;
using Restaurant.Application.Features.Staff.Commands.EvaluateStaffPerformance;
using Restaurant.Application.Features.Staff.Queries.GetStaffMember;
using Restaurant.Application.Features.Staff.Queries.GetStaffMembers;
using Restaurant.Application.Features.Staff.Queries.GetStaffByRole;
using Restaurant.Application.Features.Staff.Queries.GetStaffByShift;
using Restaurant.Application.Features.Staff.Queries.GetStaffPerformance;
using Restaurant.Application.Features.Staff.Queries.GetStaffTrainingHistory;
using Restaurant.Application.Features.Staff.Queries.GetActiveStaff;
using MediatR;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Responses;

namespace Restaurant.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion du personnel du restaurant
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Staff Management")]
public class StaffController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    public StaffController(
        IMediator mediator, 
        ILogger<StaffController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Créer un nouveau membre du personnel
    /// </summary>
    /// <param name="command">Données de création du personnel</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Membre du personnel créé</returns>
    [HttpPost]
    [Authorize(Roles = "HR,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<CreateStaffMemberResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateStaffMemberResponse>>> CreateStaffMember(
        [FromBody] CreateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating staff member with email {Email} and role {Role}", 
                command.Email, command.Role);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff member created successfully with ID {StaffId} and employee number {EmployeeNumber}", 
                response.Id, response.EmployeeNumber);
            
            return CreatedAtAction(
                nameof(GetStaffMember),
                new { id = response.Id },
                ApiResponse<CreateStaffMemberResponse>.Success(response, "Staff member created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member with email {Email}", command.Email);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir un membre du personnel par son ID
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="includePerformance">Inclure les données de performance</param>
    /// <param name="includeTraining">Inclure l'historique des formations</param>
    /// <param name="includeSchedule">Inclure les horaires</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du membre du personnel</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffMemberResponse>>> GetStaffMember(
        [FromRoute] Guid id,
        [FromQuery] bool includePerformance = false,
        [FromQuery] bool includeTraining = false,
        [FromQuery] bool includeSchedule = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffMemberQuery(id, includePerformance, includeTraining, includeSchedule);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffMemberResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir un membre du personnel par son numéro d'employé
    /// </summary>
    /// <param name="employeeNumber">Numéro d'employé</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Détails du membre du personnel</returns>
    [HttpGet("by-employee-number/{employeeNumber}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffMemberResponse>>> GetStaffMemberByEmployeeNumber(
        [FromRoute] string employeeNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffMemberByEmployeeNumberQuery(employeeNumber);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffMemberResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff member with employee number {EmployeeNumber}", employeeNumber);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir tous les membres du personnel avec pagination
    /// </summary>
    /// <param name="query">Paramètres de recherche et pagination</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Liste paginée du personnel</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffMembersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffMembersResponse>>> GetStaffMembers(
        [FromQuery] GetStaffMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<GetStaffMembersResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff members");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir le personnel actif
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Personnel actuellement actif</returns>
    [HttpGet("active")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetActiveStaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetActiveStaffResponse>>> GetActiveStaff(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetActiveStaffQuery();
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetActiveStaffResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active staff");
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir le personnel par rôle
    /// </summary>
    /// <param name="role">Rôle du personnel</param>
    /// <param name="activeOnly">Seulement le personnel actif</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Personnel du rôle spécifié</returns>
    [HttpGet("by-role/{role}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffByRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffByRoleResponse>>> GetStaffByRole(
        [FromRoute] string role,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.StaffRole>(role, true, out var roleEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid role value"));
            }

            var query = new GetStaffByRoleQuery(roleEnum, activeOnly);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffByRoleResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff for role {Role}", role);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir le personnel par service/équipe
    /// </summary>
    /// <param name="shift">Service ou équipe</param>
    /// <param name="date">Date du service</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Personnel du service spécifié</returns>
    [HttpGet("by-shift/{shift}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffByShiftResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffByShiftResponse>>> GetStaffByShift(
        [FromRoute] string shift,
        [FromQuery] DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<Restaurant.Domain.Enums.WorkShift>(shift, true, out var shiftEnum))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid shift value"));
            }

            var targetDate = date ?? DateTime.UtcNow.Date;
            var query = new GetStaffByShiftQuery(shiftEnum, targetDate);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffByShiftResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff for shift {Shift}", shift);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour les informations d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Données de mise à jour</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Personnel mis à jour</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "HR,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateStaffMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateStaffMemberResponse>>> UpdateStaffMember(
        [FromRoute] Guid id,
        [FromBody] UpdateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating staff member with ID {StaffId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff member updated successfully with ID {StaffId}", id);
            
            return Ok(ApiResponse<UpdateStaffMemberResponse>.Success(response, "Staff member updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Changer le rôle d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Nouveau rôle</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation du changement de rôle</returns>
    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateStaffRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateStaffRoleResponse>>> UpdateStaffRole(
        [FromRoute] Guid id,
        [FromBody] UpdateStaffRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.StaffId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating role for staff member {StaffId} to {NewRole}", id, command.NewRole);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff role updated successfully for ID {StaffId}", id);
            
            return Ok(ApiResponse<UpdateStaffRoleResponse>.Success(response, "Staff role updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Mettre à jour l'équipe/service d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Nouvel horaire</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation du changement d'équipe</returns>
    [HttpPatch("{id:guid}/shift")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateStaffShiftResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UpdateStaffShiftResponse>>> UpdateStaffShift(
        [FromRoute] Guid id,
        [FromBody] UpdateStaffShiftCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.StaffId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Updating shift for staff member {StaffId} to {NewShift}", id, command.NewShift);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff shift updated successfully for ID {StaffId}", id);
            
            return Ok(ApiResponse<UpdateStaffShiftResponse>.Success(response, "Staff shift updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift for staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Activer un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'activation</returns>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "HR,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ActivateStaffMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateStaffMemberResponse>>> ActivateStaffMember(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating staff member with ID {StaffId}", id);
            
            var command = new ActivateStaffMemberCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff member activated successfully with ID {StaffId}", id);
            
            return Ok(ApiResponse<ActivateStaffMemberResponse>.Success(response, "Staff member activated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Désactiver un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Raison de désactivation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation de désactivation</returns>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "HR,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DeactivateStaffMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DeactivateStaffMemberResponse>>> DeactivateStaffMember(
        [FromRoute] Guid id,
        [FromBody] DeactivateStaffMemberCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.StaffId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Deactivating staff member with ID {StaffId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Staff member deactivated successfully with ID {StaffId}", id);
            
            return Ok(ApiResponse<DeactivateStaffMemberResponse>.Success(response, "Staff member deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating staff member with ID {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Enregistrer une formation pour un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Données de la formation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'enregistrement de formation</returns>
    [HttpPost("{id:guid}/training")]
    [Authorize(Roles = "HR,Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RecordStaffTrainingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RecordStaffTrainingResponse>>> RecordStaffTraining(
        [FromRoute] Guid id,
        [FromBody] RecordStaffTrainingCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.StaffId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Recording training for staff member {StaffId}: {TrainingType}", id, command.TrainingType);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Training recorded successfully for staff member {StaffId}", id);
            
            return CreatedAtAction(
                nameof(GetStaffTrainingHistory),
                new { id },
                ApiResponse<RecordStaffTrainingResponse>.Success(response, "Training recorded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording training for staff member {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir l'historique des formations d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Historique des formations</returns>
    [HttpGet("{id:guid}/training")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffTrainingHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffTrainingHistoryResponse>>> GetStaffTrainingHistory(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffTrainingHistoryQuery(id);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffTrainingHistoryResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training history for staff member {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Évaluer la performance d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="command">Données d'évaluation</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Confirmation d'évaluation</returns>
    [HttpPost("{id:guid}/performance")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<EvaluateStaffPerformanceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EvaluateStaffPerformanceResponse>>> EvaluateStaffPerformance(
        [FromRoute] Guid id,
        [FromBody] EvaluateStaffPerformanceCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (id != command.StaffId)
            {
                return BadRequest(ApiErrorResponse.BadRequest("ID mismatch between route and body"));
            }

            _logger.LogInformation("Evaluating performance for staff member {StaffId}", id);
            
            var response = await _mediator.Send(command, cancellationToken);
            
            _logger.LogInformation("Performance evaluation completed for staff member {StaffId}", id);
            
            return CreatedAtAction(
                nameof(GetStaffPerformance),
                new { id },
                ApiResponse<EvaluateStaffPerformanceResponse>.Success(response, "Performance evaluation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating performance for staff member {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir les évaluations de performance d'un membre du personnel
    /// </summary>
    /// <param name="id">Identifiant du membre du personnel</param>
    /// <param name="fromDate">Date de début</param>
    /// <param name="toDate">Date de fin</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Évaluations de performance</returns>
    [HttpGet("{id:guid}/performance")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffPerformanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffPerformanceResponse>>> GetStaffPerformance(
        [FromRoute] Guid id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffPerformanceQuery(id, fromDate, toDate);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffPerformanceResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance data for staff member {StaffId}", id);
            return HandleError(ex);
        }
    }

    /// <summary>
    /// Obtenir le planning du personnel pour une période
    /// </summary>
    /// <param name="fromDate">Date de début</param>
    /// <param name="toDate">Date de fin</param>
    /// <param name="shift">Filtre par équipe</param>
    /// <param name="role">Filtre par rôle</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Planning du personnel</returns>
    [HttpGet("schedule")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStaffScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStaffScheduleResponse>>> GetStaffSchedule(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? shift = null,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Restaurant.Domain.Enums.WorkShift? shiftEnum = null;
            if (!string.IsNullOrEmpty(shift) && !Enum.TryParse<Restaurant.Domain.Enums.WorkShift>(shift, true, out var parsedShift))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid shift value"));
            }
            else if (!string.IsNullOrEmpty(shift))
            {
                shiftEnum = parsedShift;
            }

            Restaurant.Domain.Enums.StaffRole? roleEnum = null;
            if (!string.IsNullOrEmpty(role) && !Enum.TryParse<Restaurant.Domain.Enums.StaffRole>(role, true, out var parsedRole))
            {
                return BadRequest(ApiErrorResponse.BadRequest("Invalid role value"));
            }
            else if (!string.IsNullOrEmpty(role))
            {
                roleEnum = parsedRole;
            }

            var query = new GetStaffScheduleQuery(fromDate, toDate, shiftEnum, roleEnum);
            var response = await _mediator.Send(query, cancellationToken);
            
            return Ok(ApiResponse<GetStaffScheduleResponse>.Success(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff schedule from {FromDate} to {ToDate}", fromDate, toDate);
            return HandleError(ex);
        }
    }
}