using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.Data.Configurations
{
    /// <summary>
    /// User entity configuration
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(191); // Max pour index MySQL utf8mb4: 767/4 = 191

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.FirstName)
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .HasMaxLength(100);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            // Indexes
            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            // Relationships
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Devices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(u => u.Sessions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Role entity configuration
    /// </summary>
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name");

            // Relationships
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Permission entity configuration
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.Module)
                .IsRequired()
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Permissions_Name");

            builder.HasIndex(p => p.Module)
                .HasDatabaseName("IX_Permissions_Module");

            // Relationships
            builder.HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// UserRole junction table configuration
    /// </summary>
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.AssignedBy)
                .HasMaxLength(50);

            // Relationships configured in User and Role configurations
        }
    }

    /// <summary>
    /// RolePermission junction table configuration
    /// </summary>
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Property(rp => rp.AssignedBy)
                .HasMaxLength(50);

            // Relationships configured in Role and Permission configurations
        }
    }

    /// <summary>
    /// Device entity configuration
    /// </summary>
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DeviceKey)
                .IsRequired()
                .HasMaxLength(191); // Max pour index MySQL utf8mb4

            builder.Property(d => d.DeviceName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.DeviceType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(d => d.LastIpAddress)
                .HasMaxLength(45);

            builder.Property(d => d.UserAgent)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(d => d.DeviceKey)
                .IsUnique()
                .HasDatabaseName("IX_Devices_DeviceKey");

            builder.HasIndex(d => d.UserId)
                .HasDatabaseName("IX_Devices_UserId");

            builder.HasIndex(d => d.IsActive)
                .HasDatabaseName("IX_Devices_IsActive");

            // Relationships
            builder.HasMany(d => d.Sessions)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(d => d.AuditLogs)
                .WithOne(al => al.Device)
                .HasForeignKey(al => al.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    /// <summary>
    /// UserSession entity configuration
    /// </summary>
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.AccessToken)
                .HasMaxLength(1000);

            builder.Property(s => s.RefreshToken)
                .HasMaxLength(250); // Réduit pour MySQL index key length

            builder.Property(s => s.IpAddress)
                .HasMaxLength(45);

            builder.Property(s => s.UserAgent)
                .HasMaxLength(191); // Max pour index MySQL utf8mb4

            // Indexes
            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("IX_UserSessions_UserId");

            builder.HasIndex(s => s.DeviceId)
                .HasDatabaseName("IX_UserSessions_DeviceId");

            // Index sur RefreshToken avec préfixe pour MySQL
            builder.HasIndex(s => s.RefreshToken)
                .HasDatabaseName("IX_UserSessions_RefreshToken");

            builder.HasIndex(s => s.ExpiresAt)
                .HasDatabaseName("IX_UserSessions_ExpiresAt");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_UserSessions_IsActive");
        }
    }

    /// <summary>
    /// AuditLog entity configuration
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(al => al.Id);

            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityType)
                .HasMaxLength(100);

            builder.Property(al => al.EntityId)
                .HasMaxLength(50);

            builder.Property(al => al.OldValues)
                .HasColumnType("json");

            builder.Property(al => al.NewValues)
                .HasColumnType("json");

            builder.Property(al => al.IpAddress)
                .HasMaxLength(45);

            builder.Property(al => al.UserAgent)
                .HasMaxLength(500);

            builder.Property(al => al.Module)
                .HasMaxLength(100);

            builder.Property(al => al.Level)
                .IsRequired()
                .HasConversion<string>();

            // Indexes
            builder.HasIndex(al => al.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(al => al.DeviceId)
                .HasDatabaseName("IX_AuditLogs_DeviceId");

            builder.HasIndex(al => al.Action)
                .HasDatabaseName("IX_AuditLogs_Action");

            builder.HasIndex(al => al.EntityType)
                .HasDatabaseName("IX_AuditLogs_EntityType");

            builder.HasIndex(al => al.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            builder.HasIndex(al => al.Module)
                .HasDatabaseName("IX_AuditLogs_Module");
        }
    }
}
