using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _context;

    public PermissionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.ToListAsync(cancellationToken);
    }

    public async Task<Permission> AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.Permissions.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task UpdateAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Update(entity);
    }

    public async Task DeleteAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        _context.Permissions.Remove(entity);
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
        return await _context.Permissions.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.Where(p => p.IsActive && !p.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Select(rp => rp.Permission).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions.AnyAsync(p => p.Name == name, cancellationToken);
    }
}