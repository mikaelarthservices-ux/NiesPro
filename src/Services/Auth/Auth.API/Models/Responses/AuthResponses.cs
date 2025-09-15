namespace Auth.API.Models.Responses
{
    /// <summary>
    /// Authentication response model
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// JWT access token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token for token renewal
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (typically "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserInfo User { get; set; } = new();
    }

    /// <summary>
    /// User information model
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Full display name
        /// </summary>
        public string DisplayName => 
            !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
                ? $"{FirstName} {LastName}".Trim()
                : Username;

        /// <summary>
        /// User roles
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// User permissions
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }

    /// <summary>
    /// User registration response model
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Account active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Email confirmation status
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Account creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Registration success message
        /// </summary>
        public string Message { get; set; } = "Registration successful. Please check your email for verification instructions.";
    }

    /// <summary>
    /// Change password response model
    /// </summary>
    public class ChangePasswordResponse
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Password change timestamp
        /// </summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// Whether other sessions were invalidated
        /// </summary>
        public bool SessionsInvalidated { get; set; }

        /// <summary>
        /// Number of devices that were logged out
        /// </summary>
        public int DevicesLoggedOut { get; set; }

        /// <summary>
        /// Password change confirmation message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// User profile response model
    /// </summary>
    public class UserProfileResponse
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Account active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Email confirmation status
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Account creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User roles
        /// </summary>
        public List<RoleInfo>? Roles { get; set; }

        /// <summary>
        /// User permissions
        /// </summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// User active devices
        /// </summary>
        public List<DeviceInfo>? Devices { get; set; }

        /// <summary>
        /// Full display name
        /// </summary>
        public string DisplayName => 
            !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
                ? $"{FirstName} {LastName}".Trim()
                : Username;
    }

    /// <summary>
    /// Role information model
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// Role unique identifier
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Role description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Role assignment date
        /// </summary>
        public DateTime AssignedAt { get; set; }
    }

    /// <summary>
    /// Device information model
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Device unique identifier
        /// </summary>
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Device type
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Device active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Last usage timestamp
        /// </summary>
        public DateTime LastUsedAt { get; set; }

        /// <summary>
        /// Last known IP address
        /// </summary>
        public string? LastIpAddress { get; set; }
    }
}