using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AuthDbContext _context;

    public AuditLogRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.ToListAsync(cancellationToken);
    }

    public async Task<AuditLog> AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.AuditLogs.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Update(entity);
    }

    public async Task DeleteAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Remove(entity);
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
        return await _context.AuditLogs.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.IpAddress == ipAddress)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task CleanupOldLogsAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        var oldLogs = await _context.AuditLogs.Where(a => a.CreatedAt < beforeDate).ToListAsync(cancellationToken);
        if (oldLogs.Any())
        {
            _context.AuditLogs.RemoveRange(oldLogs);
        }
    }
}