namespace Auth.Application.Features.Users.Queries.GetUserProfile
{
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
        /// User roles (if included)
        /// </summary>
        public List<UserRoleDto>? Roles { get; set; }

        /// <summary>
        /// User permissions (if included)
        /// </summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// User devices (if included)
        /// </summary>
        public List<UserDeviceDto>? Devices { get; set; }
    }

    /// <summary>
    /// User role DTO
    /// </summary>
    public class UserRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime AssignedAt { get; set; }
    }

    /// <summary>
    /// User device DTO
    /// </summary>
    public class UserDeviceDto
    {
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastUsedAt { get; set; }
        public string? LastIpAddress { get; set; }
    }
}
