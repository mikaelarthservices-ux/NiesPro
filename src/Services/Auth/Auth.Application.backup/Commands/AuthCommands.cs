using MediatR;
using NiesPro.Services.Auth.Application.DTOs;

namespace NiesPro.Services.Auth.Application.Commands
{
    /// <summary>
    /// Login command
    /// </summary>
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Register device command
    /// </summary>
    public class RegisterDeviceCommand : IRequest<RegisterDeviceResponse>
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "desktop";
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Refresh token command
    /// </summary>
    public class RefreshTokenCommand : IRequest<LoginResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Logout command
    /// </summary>
    public class LogoutCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string? DeviceKey { get; set; }
        public string? RefreshToken { get; set; }
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// Create user command
    /// </summary>
    public class CreateUserCommand : IRequest<UserDto>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Update user command
    /// </summary>
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Roles { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Change password command
    /// </summary>
    public class ChangePasswordCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? ChangedBy { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceKey { get; set; }
    }

    /// <summary>
    /// Deactivate device command
    /// </summary>
    public class DeactivateDeviceCommand : IRequest<bool>
    {
        public int DeviceId { get; set; }
        public string? Reason { get; set; }
        public string? DeactivatedBy { get; set; }
    }

    /// <summary>
    /// Create audit log command
    /// </summary>
    public class CreateAuditLogCommand : IRequest<bool>
    {
        public int? UserId { get; set; }
        public int? DeviceId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public object? OldValues { get; set; }
        public object? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Module { get; set; }
        public string Level { get; set; } = "Info";
    }
}