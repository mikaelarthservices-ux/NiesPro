using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _context;

    public RoleRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.ToListAsync(cancellationToken);
    }

    public async Task<Role> AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.Roles.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task UpdateAsync(Role entity, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(entity);
    }

    public async Task DeleteAsync(Role entity, CancellationToken cancellationToken = default)
    {
        _context.Roles.Remove(entity);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.Where(r => r.IsActive && !r.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.Role).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetRolesByUserAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Select(rp => rp.Permission).ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AnyAsync(r => r.Name == name, cancellationToken);
    }
}