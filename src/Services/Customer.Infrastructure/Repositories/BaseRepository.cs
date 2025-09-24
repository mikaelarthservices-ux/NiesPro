using Microsoft.EntityFrameworkCore;
using NiesPro.Contracts.Common;
using NiesPro.Contracts.Infrastructure;

namespace Customer.Infrastructure.Repositories
{
    /// <summary>
    /// Base repository implementation
    /// </summary>
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly CustomerContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(CustomerContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(e => !e.IsDeleted).ToListAsync(cancellationToken);
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDeleted = false;
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.MarkAsDeleted();
            await UpdateAsync(entity, cancellationToken);
        }

        public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(e => !e.IsDeleted, cancellationToken);
        }
    }
}