using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    /// <summary>
    /// Product repository implementation
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .Include(p => p.Attributes)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            _context.Products.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Product> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByBrandAsync(Guid brandId, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.BrandId == brandId && p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsFeatured && p.IsActive)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.TrackQuantity && p.LowStockThreshold.HasValue && p.Quantity <= p.LowStockThreshold.Value)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name.Contains(searchTerm) || p.Description!.Contains(searchTerm))
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedProductsAsync(
            int page, int pageSize, string? category = null, string? brand = null,
            decimal? minPrice = null, decimal? maxPrice = null, string? sortBy = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(category) && Guid.TryParse(category, out var categoryId))
                query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(brand) && Guid.TryParse(brand, out var brandId))
                query = query.Where(p => p.BrandId == brandId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "price" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Products.Where(p => p.SKU == sku);
            if (excludeProductId.HasValue)
                query = query.Where(p => p.Id != excludeProductId.Value);

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task UpdateStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
            if (product != null)
            {
                product.Quantity = quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Product>> GetProductsWithLowStockAsync(CancellationToken cancellationToken = default)
        {
            return await GetLowStockProductsAsync(cancellationToken);
        }
    }
}