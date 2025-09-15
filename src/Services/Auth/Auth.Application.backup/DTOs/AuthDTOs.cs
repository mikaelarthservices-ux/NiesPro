namespace NiesPro.Services.Auth.Application.DTOs
{
    /// <summary>
    /// Login request DTO
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// Login response DTO
    /// </summary>
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserDto User { get; set; } = new();
    }

    /// <summary>
    /// Register device request DTO
    /// </summary>
    public class RegisterDeviceRequest
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "desktop";
        public string? UserAgent { get; set; }
    }

    /// <summary>
    /// Register device response DTO
    /// </summary>
    public class RegisterDeviceResponse
    {
        public string DeviceKey { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Refresh token request DTO
    /// </summary>
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string DeviceKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// User DTO
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// Create user request DTO
    /// </summary>
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// Update user request DTO
    /// </summary>
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Roles { get; set; }
    }

    /// <summary>
    /// Change password request DTO
    /// </summary>
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Device DTO
    /// </summary>
    public class DeviceDto
    {
        public int Id { get; set; }
        public string DeviceKey { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? LastIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Role DTO
    /// </summary>
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Permission DTO
    /// </summary>
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Audit log DTO
    /// </summary>
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public int? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Module { get; set; }
        public string Level { get; set; } = string.Empty;
    }
}