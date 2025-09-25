using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for UserRole entity
    /// </summary>
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AuthDbContext context) : base(context)
        {
        }

        public async Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default)
        {
            _context.UserRoles.Add(userRole);
            return userRole;
        }

        public async Task<UserRole?> GetByUserAndRoleIdAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(UserRole userRole, CancellationToken cancellationToken = default)
        {
            _context.UserRoles.Remove(userRole);
        }
    }
}