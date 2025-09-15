using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly CatalogDbContext _context;

        public BrandRepository(CatalogDbContext context) => _context = context;

        public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        public async Task<IEnumerable<Brand>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.Brands.Where(b => b.IsActive).ToListAsync(cancellationToken);

        public async Task<Brand> AddAsync(Brand entity, CancellationToken cancellationToken = default)
        {
            _context.Brands.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Brand> UpdateAsync(Brand entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Brands.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Brands.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Brands.AnyAsync(b => b.Id == id, cancellationToken);

        public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            await _context.Brands.FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default) =>
            await _context.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(cancellationToken);

        public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeBrandId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Brands.Where(b => b.Slug == slug);
            if (excludeBrandId.HasValue)
                query = query.Where(b => b.Id != excludeBrandId.Value);
            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> HasProductsAsync(Guid brandId, CancellationToken cancellationToken = default) =>
            await _context.Products.AnyAsync(p => p.BrandId == brandId, cancellationToken);

        public async Task<int> GetProductCountAsync(Guid brandId, CancellationToken cancellationToken = default) =>
            await _context.Products.CountAsync(p => p.BrandId == brandId && p.IsActive, cancellationToken);
    }

    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly CatalogDbContext _context;

        public ProductVariantRepository(CatalogDbContext context) => _context = context;

        public async Task<ProductVariant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.Id == id, cancellationToken);

        public async Task<IEnumerable<ProductVariant>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.Include(pv => pv.Product).ToListAsync(cancellationToken);

        public async Task<ProductVariant> AddAsync(ProductVariant entity, CancellationToken cancellationToken = default)
        {
            _context.ProductVariants.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<ProductVariant> UpdateAsync(ProductVariant entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProductVariants.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProductVariants.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.AnyAsync(pv => pv.Id == id, cancellationToken);

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.Where(pv => pv.ProductId == productId && pv.IsActive).ToListAsync(cancellationToken);

        public async Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.Include(pv => pv.Product).FirstOrDefaultAsync(pv => pv.SKU == sku, cancellationToken);

        public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeVariantId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ProductVariants.Where(pv => pv.SKU == sku);
            if (excludeVariantId.HasValue)
                query = query.Where(pv => pv.Id != excludeVariantId.Value);
            return !await query.AnyAsync(cancellationToken);
        }

        public async Task UpdateStockAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default)
        {
            var variant = await _context.ProductVariants.FindAsync(new object[] { variantId }, cancellationToken);
            if (variant != null)
            {
                variant.Quantity = quantity;
                variant.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<ProductVariant>> GetLowStockVariantsAsync(CancellationToken cancellationToken = default) =>
            await _context.ProductVariants.Include(pv => pv.Product)
                .Where(pv => pv.Quantity <= 5 && pv.IsActive).ToListAsync(cancellationToken);
    }

    public class ProductAttributeRepository : IProductAttributeRepository
    {
        private readonly CatalogDbContext _context;

        public ProductAttributeRepository(CatalogDbContext context) => _context = context;

        public async Task<ProductAttribute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.FirstOrDefaultAsync(pa => pa.Id == id, cancellationToken);

        public async Task<IEnumerable<ProductAttribute>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.ToListAsync(cancellationToken);

        public async Task<ProductAttribute> AddAsync(ProductAttribute entity, CancellationToken cancellationToken = default)
        {
            _context.ProductAttributes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<ProductAttribute> UpdateAsync(ProductAttribute entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProductAttributes.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ProductAttributes.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.AnyAsync(pa => pa.Id == id, cancellationToken);

        public async Task<IEnumerable<ProductAttribute>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.Where(pa => pa.ProductId == productId).OrderBy(pa => pa.SortOrder).ToListAsync(cancellationToken);

        public async Task<IEnumerable<ProductAttribute>> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.Where(pa => pa.Name == name).ToListAsync(cancellationToken);

        public async Task DeleteByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var attributes = await _context.ProductAttributes.Where(pa => pa.ProductId == productId).ToListAsync(cancellationToken);
            foreach (var attr in attributes)
            {
                attr.IsDeleted = true;
                attr.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<string>> GetDistinctAttributeNamesAsync(CancellationToken cancellationToken = default) =>
            await _context.ProductAttributes.Select(pa => pa.Name).Distinct().ToListAsync(cancellationToken);
    }

    public class ReviewRepository : IReviewRepository
    {
        private readonly CatalogDbContext _context;

        public ReviewRepository(CatalogDbContext context) => _context = context;

        public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Reviews.Include(r => r.Product).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        public async Task<IEnumerable<Review>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.Reviews.Include(r => r.Product).ToListAsync(cancellationToken);

        public async Task<Review> AddAsync(Review entity, CancellationToken cancellationToken = default)
        {
            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Review> UpdateAsync(Review entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Reviews.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Reviews.AnyAsync(r => r.Id == id, cancellationToken);

        public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, bool approvedOnly = true, CancellationToken cancellationToken = default)
        {
            var query = _context.Reviews.Where(r => r.ProductId == productId);
            if (approvedOnly)
                query = query.Where(r => r.IsApproved);
            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync(CancellationToken cancellationToken = default) =>
            await _context.Reviews.Include(r => r.Product).Where(r => !r.IsApproved).OrderBy(r => r.CreatedAt).ToListAsync(cancellationToken);

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedReviewsAsync(
            Guid? productId, int page, int pageSize, bool? isApproved = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Reviews.Include(r => r.Product).AsQueryable();
            
            if (productId.HasValue)
                query = query.Where(r => r.ProductId == productId.Value);
            
            if (isApproved.HasValue)
                query = query.Where(r => r.IsApproved == isApproved.Value);

            var totalCount = await query.CountAsync(cancellationToken);
            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (reviews, totalCount);
        }

        public async Task ApproveReviewAsync(Guid reviewId, string approvedBy, CancellationToken cancellationToken = default)
        {
            var review = await _context.Reviews.FindAsync(new object[] { reviewId }, cancellationToken);
            if (review != null)
            {
                review.IsApproved = true;
                review.ApprovedAt = DateTime.UtcNow;
                review.ApprovedBy = approvedBy;
                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<decimal> GetAverageRatingAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Select(r => r.Rating)
                .ToListAsync(cancellationToken);

            return ratings.Any() ? (decimal)ratings.Average() : 0;
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .GroupBy(r => r.Rating)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }
    }
}