using System.ComponentModel.DataAnnotations;

namespace Auth.API.Models.Requests
{
    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Valid email address is required")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Device identifier for multi-device support
        /// </summary>
        [Required(ErrorMessage = "Device key is required")]
        [StringLength(100, ErrorMessage = "Device key cannot exceed 100 characters")]
        public string DeviceKey { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable device name
        /// </summary>
        [StringLength(100, ErrorMessage = "Device name cannot exceed 100 characters")]
        public string? DeviceName { get; set; }

        /// <summary>
        /// Remember login for extended session
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// User registration request model
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Unique username
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, dots, hyphens and underscores")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User password
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password confirmation
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// User first name
        /// </summary>
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string? FirstName { get; set; }

        /// <summary>
        /// User last name
        /// </summary>
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string? LastName { get; set; }

        /// <summary>
        /// User phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Device identifier for registration tracking
        /// </summary>
        [Required(ErrorMessage = "Device key is required")]
        [StringLength(100, ErrorMessage = "Device key cannot exceed 100 characters")]
        public string DeviceKey { get; set; } = string.Empty;

        /// <summary>
        /// Device name for registration tracking
        /// </summary>
        [StringLength(100, ErrorMessage = "Device name cannot exceed 100 characters")]
        public string? DeviceName { get; set; }
    }

    /// <summary>
    /// Change password request model
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Current password for verification
        /// </summary>
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 128 characters")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// New password confirmation
        /// </summary>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Device identifier for audit trail
        /// </summary>
        [Required(ErrorMessage = "Device key is required")]
        [StringLength(100, ErrorMessage = "Device key cannot exceed 100 characters")]
        public string DeviceKey { get; set; } = string.Empty;

        /// <summary>
        /// Device name for audit trail
        /// </summary>
        [StringLength(100, ErrorMessage = "Device name cannot exceed 100 characters")]
        public string? DeviceName { get; set; }
    }

    /// <summary>
    /// Refresh token request model
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Refresh token value
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Device identifier for security validation
        /// </summary>
        [Required(ErrorMessage = "Device key is required")]
        [StringLength(100, ErrorMessage = "Device key cannot exceed 100 characters")]
        public string DeviceKey { get; set; } = string.Empty;
    }
}