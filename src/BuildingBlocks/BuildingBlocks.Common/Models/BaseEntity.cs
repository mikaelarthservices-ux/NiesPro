using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Common.Models
{
    /// <summary>
    /// Base entity interface for all domain entities
    /// Provides common properties and behaviors for entity management
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
    /// Base abstract entity implementation
    /// All domain entities should inherit from this class
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
        /// Marks the entity as updated with optional user tracking
        /// </summary>
        /// <param name="updatedBy">ID of the user making the update</param>
        public virtual void MarkAsUpdated(Guid? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Performs soft delete on the entity with optional user tracking
        /// </summary>
        /// <param name="deletedBy">ID of the user performing the deletion</param>
        public virtual void MarkAsDeleted(Guid? deletedBy = null)
        {
            IsDeleted = true;
            MarkAsUpdated(deletedBy);
        }

        /// <summary>
        /// Restores a soft-deleted entity with optional user tracking
        /// </summary>
        /// <param name="restoredBy">ID of the user performing the restoration</param>
        public virtual void MarkAsRestored(Guid? restoredBy = null)
        {
            IsDeleted = false;
            MarkAsUpdated(restoredBy);
        }

        /// <summary>
        /// Determines if two entities are equal based on their ID
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not BaseEntity other || GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id);
        }

        /// <summary>
        /// Gets hash code based on entity ID
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// String representation of the entity
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} [Id: {Id}]";
        }
    }

    /// <summary>
    /// Base entity with audit trail information
    /// Extends BaseEntity with comprehensive audit fields
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