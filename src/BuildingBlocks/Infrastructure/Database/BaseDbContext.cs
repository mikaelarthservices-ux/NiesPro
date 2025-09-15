using Microsoft.EntityFrameworkCore;
using NiesPro.Common.Models;
using System.Linq.Expressions;

namespace NiesPro.Infrastructure.Database
{
    /// <summary>
    /// Base context with common functionality for all microservices
    /// </summary>
    public abstract class BaseDbContext : DbContext
    {
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// Override SaveChanges to automatically update audit fields
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update audit fields
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update CreatedAt, UpdatedAt fields automatically
        /// </summary>
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = GetCurrentUser();
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedBy = GetCurrentUser();
                        break;

                    case EntityState.Deleted:
                        // Soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.DeletedBy = GetCurrentUser();
                        break;
                }
            }
        }

        /// <summary>
        /// Get current user from context (to be implemented in derived contexts)
        /// </summary>
        protected virtual string? GetCurrentUser()
        {
            // TODO: Implement user context injection
            return "system";
        }

        /// <summary>
        /// Configure common entity properties
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global query filter for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Add global filter for soft delete
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));

                    // Configure common properties
                    modelBuilder.Entity(entityType.ClrType)
                        .Property("CreatedAt")
                        .IsRequired();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property("IsDeleted")
                        .IsRequired()
                        .HasDefaultValue(false);
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Create soft delete filter expression
        /// </summary>
        private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            return Expression.Lambda(condition, parameter);
        }
    }
}