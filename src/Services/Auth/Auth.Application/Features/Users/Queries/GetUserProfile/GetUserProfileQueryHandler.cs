using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Users.Queries.GetUserProfile;
using Auth.Domain.Interfaces;
using Auth.Application.Common.Models;
using BuildingBlocks.Common.DTOs;
using Auth.Domain.Entities;

namespace Auth.Application.Features.Users.Queries.GetUserProfile
{
    /// <summary>
    /// Get user profile query handler with role and permission loading
    /// </summary>
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<UserProfileResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDeviceRepository deviceRepository,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving user profile for: {UserId}", request.UserId);

                // 1. Get user basic information
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return ApiResponse<UserProfileResponse>.CreateError("User not found");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user profile requested: {UserId}", request.UserId);
                    return ApiResponse<UserProfileResponse>.CreateError("User account is inactive");
                }

                // 2. Build base response
                var response = new UserProfileResponse
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt ?? user.CreatedAt
                };

                // 3. Load roles if requested
                if (request.IncludeRoles)
                {
                    var userRoles = await _roleRepository.GetRolesByUserIdAsync(request.UserId, cancellationToken);
                    response.Roles = userRoles.Select(role => new UserRoleDto
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        Description = role.Description,
                        AssignedAt = DateTime.UtcNow // Would come from UserRole join table
                    }).ToList();

                    _logger.LogDebug("Loaded {RoleCount} roles for user: {UserId}", response.Roles.Count, request.UserId);
                }

                // 4. Load permissions if requested
                if (request.IncludePermissions)
                {
                    var userPermissions = await GetUserPermissionsAsync(request.UserId, cancellationToken);
                    response.Permissions = userPermissions.ToList();

                    _logger.LogDebug("Loaded {PermissionCount} permissions for user: {UserId}", 
                        response.Permissions.Count, request.UserId);
                }

                // 5. Load devices if requested
                if (request.IncludeDevices)
                {
                    var userDevices = await _deviceRepository.GetDevicesByUserIdAsync(request.UserId, cancellationToken);
                    response.Devices = userDevices.Select(device => new UserDeviceDto
                    {
                        DeviceId = device.Id,
                        DeviceName = device.DeviceName,
                        DeviceType = device.DeviceType.ToString(),
                        IsActive = device.IsActive,
                        LastUsedAt = device.LastUsedAt ?? DateTime.UtcNow,
                        LastIpAddress = device.LastIpAddress
                    }).ToList();

                    _logger.LogDebug("Loaded {DeviceCount} devices for user: {UserId}", 
                        response.Devices.Count, request.UserId);
                }

                _logger.LogInformation("Successfully retrieved user profile: {UserId}", request.UserId);

                return ApiResponse<UserProfileResponse>.CreateSuccess(response, "User profile retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile: {UserId}", request.UserId);
                return ApiResponse<UserProfileResponse>.CreateError("Failed to retrieve user profile");
            }
        }

        /// <summary>
        /// Get user permissions from roles and direct assignments
        /// </summary>
        private async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var permissions = new HashSet<string>();

            try
            {
                // Get permissions from user roles
                var userRoles = await _roleRepository.GetRolesByUserIdAsync(userId, cancellationToken);
                foreach (var role in userRoles)
                {
                    var rolePermissions = await _roleRepository.GetPermissionsByRoleIdAsync(role.Id, cancellationToken);
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission.Name);
                    }
                }

                // Note: Could also include direct user permissions if that model exists
                // var directPermissions = await _userRepository.GetDirectPermissionsAsync(userId, cancellationToken);
                // foreach (var permission in directPermissions)
                // {
                //     permissions.Add(permission.Name);
                // }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions for user: {UserId}", userId);
            }

            return permissions;
        }
    }
}
