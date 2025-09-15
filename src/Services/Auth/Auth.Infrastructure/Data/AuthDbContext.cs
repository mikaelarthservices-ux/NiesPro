using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data.Configurations;

namespace Auth.Infrastructure.Data
{
    /// <summary>
    /// Auth database context with Entity Framework configuration
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // DbSets for Auth entities
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);

            // Configure table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Permission>().ToTable("Permissions");
            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<RolePermission>().ToTable("RolePermissions");
            modelBuilder.Entity<Device>().ToTable("Devices");
            modelBuilder.Entity<UserSession>().ToTable("UserSessions");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default configuration for development
                optionsBuilder.UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=NiesPro_Auth;Trusted_Connection=true;MultipleActiveResultSets=true",
                    options => options.EnableRetryOnFailure()
                );
            }

            // Enable development features
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }
}
