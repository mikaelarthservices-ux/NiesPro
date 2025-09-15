namespace Catalog.Application.DTOs
{
    /// <summary>
    /// Product Data Transfer Objects
    /// </summary>
    public record ProductDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string SKU { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public bool TrackQuantity { get; init; }
        public int Quantity { get; init; }
        public decimal? Weight { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsActive { get; init; }
        public bool IsFeatured { get; init; }
        public DateTime? PublishedAt { get; init; }
        public Guid CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public Guid? BrandId { get; init; }
        public string? BrandName { get; init; }
        public bool IsInStock { get; init; }
        public bool IsLowStock { get; init; }
        public decimal? DiscountPercentage { get; init; }
        public decimal AverageRating { get; init; }
        public int ReviewCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record ProductSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string SKU { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsInStock { get; init; }
        public decimal AverageRating { get; init; }
        public int ReviewCount { get; init; }
        public decimal? DiscountPercentage { get; init; }
    }

    public record CreateProductDto
    {
        public string Name { get; init; } = string.Empty;
        public string SKU { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LongDescription { get; init; }
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public decimal? CostPrice { get; init; }
        public bool TrackQuantity { get; init; } = true;
        public int Quantity { get; init; } = 0;
        public int? LowStockThreshold { get; init; }
        public decimal? Weight { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageGallery { get; init; }
        public bool IsActive { get; init; } = true;
        public bool IsFeatured { get; init; } = false;
        public Guid CategoryId { get; init; }
        public Guid? BrandId { get; init; }
    }

    public record UpdateProductDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LongDescription { get; init; }
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public decimal? CostPrice { get; init; }
        public bool TrackQuantity { get; init; }
        public int Quantity { get; init; }
        public int? LowStockThreshold { get; init; }
        public decimal? Weight { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageGallery { get; init; }
        public bool IsActive { get; init; }
        public bool IsFeatured { get; init; }
        public Guid CategoryId { get; init; }
        public Guid? BrandId { get; init; }
    }

    /// <summary>
    /// Category Data Transfer Objects
    /// </summary>
    public record CategoryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public int SortOrder { get; init; }
        public bool IsActive { get; init; }
        public Guid? ParentCategoryId { get; init; }
        public string? ParentCategoryName { get; init; }
        public bool HasSubCategories { get; init; }
        public int ProductCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record CreateCategoryDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public int SortOrder { get; init; } = 0;
        public bool IsActive { get; init; } = true;
        public Guid? ParentCategoryId { get; init; }
    }

    /// <summary>
    /// Brand Data Transfer Objects
    /// </summary>
    public record BrandDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public string? Website { get; init; }
        public bool IsActive { get; init; }
        public int ProductCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record CreateBrandDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
        public string? Website { get; init; }
        public bool IsActive { get; init; } = true;
    }

    /// <summary>
    /// Paged result wrapper
    /// </summary>
    public record PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; init; } = new List<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public bool HasNextPage { get; init; }
        public bool HasPreviousPage { get; init; }
    }

    /// <summary>
    /// Search and filter DTOs
    /// </summary>
    public record ProductSearchDto
    {
        public string? SearchTerm { get; init; }
        public Guid? CategoryId { get; init; }
        public Guid? BrandId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public bool? InStockOnly { get; init; }
        public bool? FeaturedOnly { get; init; }
        public string? SortBy { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    /// <summary>
    /// Stock update DTOs
    /// </summary>
    public record UpdateStockDto
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public string? Reason { get; init; }
    }

    /// <summary>
    /// Review DTOs
    /// </summary>
    public record ReviewDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Comment { get; init; }
        public bool IsApproved { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public record CreateReviewDto
    {
        public Guid ProductId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string? CustomerEmail { get; init; }
        public int Rating { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Comment { get; init; }
    }
}