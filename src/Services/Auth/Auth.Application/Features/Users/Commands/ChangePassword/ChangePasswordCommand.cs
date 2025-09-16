using MediatR;
using BuildingBlocks.Common.DTOs;

namespace Auth.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Change password command with security validation
    /// </summary>
    public class ChangePasswordCommand : IRequest<ApiResponse<ChangePasswordResponse>>
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Current password for verification
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Device key for audit trail
        /// </summary>
        public string DeviceKey { get; set; } = string.Empty;

        /// <summary>
        /// Device name for audit trail
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// Client IP address for audit
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent for audit
        /// </summary>
        public string? UserAgent { get; set; }
    }
}
