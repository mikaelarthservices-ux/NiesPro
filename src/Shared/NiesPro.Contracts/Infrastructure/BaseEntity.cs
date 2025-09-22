using System.ComponentModel.DataAnnotations;

namespace NiesPro.Contracts.Infrastructure
{
    /// <summary>
    /// Base entity interface for all domain entities
    /// Provides common properties and behaviors for entity management
    /// Consolidated from BuildingBlocks.Common.Models
    /// </summary>
    public interface IBaseEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Timestamp when the entity was created
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the entity was last updated
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag - indicates if entity is logically deleted
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// User ID who created this entity
        /// </summary>
        Guid? CreatedBy { get; set; }

        /// <summary>
        /// User ID who last updated this entity
        /// </summary>
        Guid? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Base abstract entity implementation for infrastructure concerns
    /// All domain entities should inherit from this class when needing audit capabilities
    /// Consolidated from BuildingBlocks.Common.Models.BaseEntity
    /// </summary>
    public abstract class BaseEntity : IBaseEntity
    {
        /// <inheritdoc />
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();

        /// <inheritdoc />
        public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <inheritdoc />
        public virtual DateTime? UpdatedAt { get; set; }

        /// <inheritdoc />
        public virtual bool IsDeleted { get; set; } = false;

        /// <inheritdoc />
        public virtual Guid? CreatedBy { get; set; }

        /// <inheritdoc />
        public virtual Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Mark entity as updated with optional user information
        /// </summary>
        public virtual void MarkAsUpdated(Guid? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Soft delete the entity
        /// </summary>
        public virtual void MarkAsDeleted(Guid? deletedBy = null)
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
        }

        /// <summary>
        /// Restore soft deleted entity
        /// </summary>
        public virtual void Restore(Guid? restoredBy = null)
        {
            IsDeleted = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = restoredBy;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity? left, BaseEntity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseEntity? left, BaseEntity? right)
        {
            return !Equals(left, right);
        }
    }

    /// <summary>
    /// Extends BaseEntity with comprehensive audit fields
    /// For entities requiring full audit trail
    /// </summary>
    public abstract class AuditableEntity : BaseEntity
    {
        /// <summary>
        /// Version number for optimistic concurrency control
        /// </summary>
        [Timestamp]
        public virtual byte[]? RowVersion { get; set; }

        /// <summary>
        /// IP address from which the entity was created/modified
        /// </summary>
        [StringLength(45)] // IPv6 length
        public virtual string? IpAddress { get; set; }

        /// <summary>
        /// User agent information for web requests
        /// </summary>
        [StringLength(500)]
        public virtual string? UserAgent { get; set; }

        /// <summary>
        /// Additional metadata in JSON format
        /// </summary>
        public virtual string? Metadata { get; set; }
    }
}