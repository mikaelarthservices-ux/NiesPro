using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Auth.Application.Features.Users.Queries.GetUserProfile;
using Auth.Application.Features.Users.Queries.GetAllUsers;
using Auth.API.Models.Requests;
using Auth.API.Models.Responses;
using BuildingBlocks.Common.DTOs;

namespace Auth.API.Controllers.V1
{
    /// <summary>
    /// User management controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        /// <param name="includeRoles">Include user roles in response</param>
        /// <param name="includePermissions">Include user permissions in response</param>
        /// <param name="includeDevices">Include user devices in response</param>
        /// <returns>Current user profile information</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">User not found</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<Models.Responses.UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Models.Responses.UserProfileResponse>>> GetProfile(
            [FromQuery] bool includeRoles = true,
            [FromQuery] bool includePermissions = false,
            [FromQuery] bool includeDevices = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("Getting profile for user: {UserId}", userId);

                var query = new GetUserProfileQuery
                {
                    UserId = userId,
                    IncludeRoles = includeRoles,
                    IncludePermissions = includePermissions,
                    IncludeDevices = includeDevices
                };

                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                {
                    var response = MapToUserProfileResponse(result.Data!);
                    return Ok(ApiResponse<Models.Responses.UserProfileResponse>.CreateSuccess(response, "Profile retrieved successfully"));
                }

                _logger.LogWarning("Profile not found for user: {UserId}", userId);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving profile"));
            }
        }

        /// <summary>
        /// Get user profile by ID (Admin only)
        /// </summary>
        /// <param name="userId">User unique identifier</param>
        /// <param name="includeRoles">Include user roles in response</param>
        /// <param name="includePermissions">Include user permissions in response</param>
        /// <param name="includeDevices">Include user devices in response</param>
        /// <returns>User profile information</returns>
        /// <response code="200">Profile retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        /// <response code="404">User not found</response>
        [HttpGet("{userId:guid}/profile")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Models.Responses.UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Models.Responses.UserProfileResponse>>> GetUserProfile(
            [FromRoute] Guid userId,
            [FromQuery] bool includeRoles = true,
            [FromQuery] bool includePermissions = false,
            [FromQuery] bool includeDevices = false)
        {
            try
            {
                _logger.LogInformation("Admin getting profile for user: {UserId}", userId);

                var query = new GetUserProfileQuery
                {
                    UserId = userId,
                    IncludeRoles = includeRoles,
                    IncludePermissions = includePermissions,
                    IncludeDevices = includeDevices
                };

                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                {
                    var response = MapToUserProfileResponse(result.Data!);
                    return Ok(ApiResponse<Models.Responses.UserProfileResponse>.CreateSuccess(response, "Profile retrieved successfully"));
                }

                _logger.LogWarning("Profile not found for user: {UserId}", userId);
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user: {UserId}", userId);
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving profile"));
            }
        }

        /// <summary>
        /// Get all users with pagination and filtering (Admin only)
        /// </summary>
        /// <param name="request">Query parameters for filtering and pagination</param>
        /// <returns>Paginated list of users</returns>
        /// <response code="200">Users retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden - Admin access required</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<GetAllUsersApiResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<GetAllUsersApiResponse>>> GetAllUsers([FromQuery] GetAllUsersRequest request)
        {
            try
            {
                _logger.LogInformation("Admin getting all users. Page: {Page}, Size: {Size}", request.PageNumber, request.PageSize);

                var query = new GetAllUsersQuery
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    IsActive = request.IsActive,
                    EmailConfirmed = request.EmailConfirmed,
                    RoleName = request.RoleName,
                    SortBy = request.SortBy,
                    SortDirection = request.SortDirection,
                    IncludeRoles = request.IncludeRoles,
                    IncludeDeviceCount = request.IncludeDeviceCount
                };

                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                {
                    var response = MapToGetAllUsersResponse(result.Data!);
                    return Ok(ApiResponse<GetAllUsersApiResponse>.CreateSuccess(response, "Users retrieved successfully"));
                }

                _logger.LogWarning("Failed to retrieve users");
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving users"));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        /// <summary>
        /// Map application response to API response
        /// </summary>
        private Models.Responses.UserProfileResponse MapToUserProfileResponse(Auth.Application.Features.Users.Queries.GetUserProfile.UserProfileResponse source)
        {
            return new Models.Responses.UserProfileResponse
            {
                UserId = source.UserId,
                Username = source.Username,
                Email = source.Email,
                FirstName = source.FirstName,
                LastName = source.LastName,
                PhoneNumber = source.PhoneNumber,
                IsActive = source.IsActive,
                EmailConfirmed = source.EmailConfirmed,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.UpdatedAt,
                Roles = source.Roles?.Select(r => new RoleInfo
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    AssignedAt = r.AssignedAt
                }).ToList(),
                Permissions = source.Permissions,
                Devices = source.Devices?.Select(d => new DeviceInfo
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    DeviceType = d.DeviceType,
                    IsActive = d.IsActive,
                    LastUsedAt = d.LastUsedAt,
                    LastIpAddress = d.LastIpAddress
                }).ToList()
            };
        }

        /// <summary>
        /// Map application GetAllUsers response to API response
        /// </summary>
        private GetAllUsersApiResponse MapToGetAllUsersResponse(Auth.Application.Features.Users.Queries.GetAllUsers.GetAllUsersResponse source)
        {
            return new GetAllUsersApiResponse
            {
                Users = source.Users.Select(u => new UserSummary
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.Roles,
                    DeviceCount = u.DeviceCount,
                    LastLoginAt = u.LastLoginAt,
                    DisplayName = u.DisplayName
                }).ToList(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = source.Pagination.CurrentPage,
                    PageSize = source.Pagination.PageSize,
                    TotalCount = source.Pagination.TotalCount,
                    TotalPages = source.Pagination.TotalPages,
                    HasNextPage = source.Pagination.HasNextPage,
                    HasPreviousPage = source.Pagination.HasPreviousPage
                },
                Filters = source.Filters != null ? new FilterInfo
                {
                    SearchTerm = source.Filters.SearchTerm,
                    IsActive = source.Filters.IsActive,
                    EmailConfirmed = source.Filters.EmailConfirmed,
                    RoleName = source.Filters.RoleName,
                    SortBy = source.Filters.SortBy,
                    SortDirection = source.Filters.SortDirection
                } : null
            };
        }

        #endregion
    }
}