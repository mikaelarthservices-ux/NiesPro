using System.ComponentModel.DataAnnotations;
using NiesPro.Contracts.Infrastructure;

namespace Auth.Domain.Entities
{
    /// <summary>
    /// User entity
    /// </summary>
    public class User : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool EmailConfirmed { get; set; } = false;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool HasValidDevices => Devices.Any(d => d.IsActive);
        public IEnumerable<string> RoleNames => UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name);
    }

    /// <summary>
    /// Role entity
    /// </summary>
    public class Role : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        // Computed properties
        public IEnumerable<string> PermissionNames => RolePermissions.Where(rp => rp.Permission != null).Select(rp => rp.Permission!.Name);
    }

    /// <summary>
    /// Permission entity
    /// </summary>
    public class Permission : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Module { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// Many-to-many relationship between User and Role
    /// </summary>
    public class UserRole
    {
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid RoleId { get; set; }
        public virtual Role? Role { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }
    }

    /// <summary>
    /// Many-to-many relationship between Role and Permission
    /// </summary>
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public virtual Role? Role { get; set; }

        public Guid PermissionId { get; set; }
        public virtual Permission? Permission { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string? AssignedBy { get; set; }
    }

    /// <summary>
    /// Device entity for device-based authentication
    /// </summary>
    public class Device : BaseEntity
    {
        [Required]
        [StringLength(255)]
        public string DeviceKey { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        public DeviceType DeviceType { get; set; } = DeviceType.Desktop;

        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsTrusted { get; set; } = false;

        public DateTime? LastUsedAt { get; set; }

        [StringLength(45)]
        public string? LastIpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        // Navigation properties
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }

    /// <summary>
    /// Device types enumeration
    /// </summary>
    public enum DeviceType
    {
        Desktop = 1,
        Mobile = 2,
        Tablet = 3,
        Web = 4
    }

    /// <summary>
    /// User session entity for token management
    /// </summary>
    public class UserSession : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid? DeviceId { get; set; }
        public virtual Device? Device { get; set; }

        public string? AccessToken { get; set; }

        [StringLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime? LastActivityAt { get; set; }

        // Helper methods
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValidSession => IsActive && !IsExpired && !IsDeleted;
    }

    /// <summary>
    /// Audit log entity for security tracking
    /// </summary>
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public virtual User? User { get; set; }

        public Guid? DeviceId { get; set; }
        public virtual Device? Device { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntityType { get; set; }

        [StringLength(50)]
        public string? EntityId { get; set; }

        public string? OldValues { get; set; } // JSON

        public string? NewValues { get; set; } // JSON

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Module { get; set; }

        public AuditLogLevel Level { get; set; } = AuditLogLevel.Info;
    }

    /// <summary>
    /// Audit log levels
    /// </summary>
    public enum AuditLogLevel
    {
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }
}