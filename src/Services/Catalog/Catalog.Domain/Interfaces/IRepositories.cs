using Catalog.Domain.Entities;
using NiesPro.Contracts.Infrastructure;

namespace Catalog.Domain.Interfaces
{
    /// <summary>
    /// Base repository interface with common operations
    /// </summary>
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Product repository interface with specific operations
    /// </summary>
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetByBrandAsync(Guid brandId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int page, int pageSize, string? category = null, string? brand = null, 
            decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null, 
            CancellationToken cancellationToken = default);
        Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
        Task UpdateStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsWithLowStockAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Category repository interface
    /// </summary>
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetCategoryHierarchyAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
        Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<int> GetProductCountAsync(Guid categoryId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Brand repository interface
    /// </summary>
    public interface IBrandRepository : IBaseRepository<Brand>
    {
        Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default);
        Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeBrandId = null, CancellationToken cancellationToken = default);
        Task<bool> HasProductsAsync(Guid brandId, CancellationToken cancellationToken = default);
        Task<int> GetProductCountAsync(Guid brandId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Product variant repository interface
    /// </summary>
    public interface IProductVariantRepository : IBaseRepository<ProductVariant>
    {
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeVariantId = null, CancellationToken cancellationToken = default);
        Task UpdateStockAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Product attribute repository interface
    /// </summary>
    public interface IProductAttributeRepository : IBaseRepository<ProductAttribute>
    {
        Task<IEnumerable<ProductAttribute>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductAttribute>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetDistinctAttributeNamesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Review repository interface
    /// </summary>
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken cancellationToken = default);
        Task<IEnumerable<Review>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedReviewsAsync(
            Guid? productId, int page, int pageSize, bool? isApproved = null, 
            CancellationToken cancellationToken = default);
        Task ApproveReviewAsync(Guid reviewId, string approvedBy, CancellationToken cancellationToken = default);
        Task<decimal> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}