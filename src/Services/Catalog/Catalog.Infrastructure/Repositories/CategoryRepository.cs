using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    /// <summary>
    /// Category repository implementation
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context;

        public CategoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
        {
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Category> UpdateAsync(Category entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            var categories = new List<Category>();
            var currentCategory = await GetByIdAsync(categoryId, cancellationToken);
            
            while (currentCategory != null)
            {
                categories.Insert(0, currentCategory);
                if (currentCategory.ParentCategoryId.HasValue)
                    currentCategory = await GetByIdAsync(currentCategory.ParentCategoryId.Value, cancellationToken);
                else
                    break;
            }
            
            return categories;
        }

        public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.Where(c => c.Slug == slug);
            if (excludeCategoryId.HasValue)
                query = query.Where(c => c.Id != excludeCategoryId.Value);

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
        }

        public async Task<int> GetProductCountAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId && p.IsActive, cancellationToken);
        }
    }
}