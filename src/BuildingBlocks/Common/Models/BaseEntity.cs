using System.ComponentModel.DataAnnotations;

namespace NiesPro.Common.Models
{
    /// <summary>
    /// Base class for all entities with common properties
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        public string? DeletedBy { get; set; }
    }
}