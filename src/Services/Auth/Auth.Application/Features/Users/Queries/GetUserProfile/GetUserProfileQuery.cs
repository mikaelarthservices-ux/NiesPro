using MediatR;
using BuildingBlocks.Common.DTOs;

namespace Auth.Application.Features.Users.Queries.GetUserProfile
{
    /// <summary>
    /// Get user profile query
    /// </summary>
    public class GetUserProfileQuery : IRequest<ApiResponse<UserProfileResponse>>
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Include user roles in response
        /// </summary>
        public bool IncludeRoles { get; set; } = true;

        /// <summary>
        /// Include user permissions in response
        /// </summary>
        public bool IncludePermissions { get; set; } = false;

        /// <summary>
        /// Include user devices in response
        /// </summary>
        public bool IncludeDevices { get; set; } = false;
    }
}
