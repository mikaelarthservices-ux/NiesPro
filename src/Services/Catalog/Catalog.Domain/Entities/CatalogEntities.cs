using System.ComponentModel.DataAnnotations;
using NiesPro.Contracts.Infrastructure;

namespace Catalog.Domain.Entities
{
    /// <summary>
    /// Product entity - Core entity of the catalog
    /// </summary>
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? LongDescription { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal? ComparePrice { get; set; }

        public decimal? CostPrice { get; set; }

        public bool TrackQuantity { get; set; } = true;

        public int Quantity { get; set; } = 0;

        public int? LowStockThreshold { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(2000)]
        public string? ImageGallery { get; set; } // JSON array of image URLs

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        // Foreign Keys
        public Guid CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public Guid? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        // Navigation properties
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Computed properties
        public bool IsInStock => !TrackQuantity || Quantity > 0;
        public bool IsLowStock => TrackQuantity && LowStockThreshold.HasValue && Quantity <= LowStockThreshold.Value;
        public decimal? DiscountPercentage => ComparePrice.HasValue && ComparePrice > Price ? 
            Math.Round(((ComparePrice.Value - Price) / ComparePrice.Value) * 100, 2) : null;
        public decimal AverageRating => Reviews.Any() ? (decimal)Reviews.Average(r => r.Rating) : 0;
        public int ReviewCount => Reviews.Count(r => r.IsApproved);
    }

    /// <summary>
    /// Category entity for product organization
    /// </summary>
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Hierarchical structure
        public Guid? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        // Computed properties
        public bool HasSubCategories => SubCategories.Any();
        public int ProductCount => Products.Count(p => p.IsActive && !p.IsDeleted);
    }

    /// <summary>
    /// Brand entity for product branding
    /// </summary>
    public class Brand : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(300)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        // Computed properties
        public int ProductCount => Products.Count(p => p.IsActive && !p.IsDeleted);
    }

    /// <summary>
    /// Product variant for different options (size, color, etc.)
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        public decimal? PriceAdjustment { get; set; } = 0;

        public int Quantity { get; set; } = 0;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        // Computed properties
        public decimal FinalPrice => (Product?.Price ?? 0) + (PriceAdjustment ?? 0);
        public bool IsInStock => Quantity > 0;
    }

    /// <summary>
    /// Product attributes for flexible metadata
    /// </summary>
    public class ProductAttribute : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;

        public AttributeType Type { get; set; } = AttributeType.Text;

        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// Product review entity
    /// </summary>
    public class Review : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        public string? CustomerEmail { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime? ApprovedAt { get; set; }

        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        // Computed properties
        public bool IsRecentReview => CreatedAt > DateTime.UtcNow.AddDays(-30);
    }

    /// <summary>
    /// Attribute types enumeration
    /// </summary>
    public enum AttributeType
    {
        Text = 1,
        Number = 2,
        Boolean = 3,
        Date = 4,
        Color = 5,
        Size = 6,
        Material = 7,
        Other = 8
    }

    /// <summary>
    /// Product status enumeration
    /// </summary>
    public enum ProductStatus
    {
        Draft = 1,
        Active = 2,
        Inactive = 3,
        OutOfStock = 4,
        Discontinued = 5
    }
}