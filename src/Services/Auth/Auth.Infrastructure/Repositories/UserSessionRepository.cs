using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AuthDbContext _context;

    public UserSessionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions.ToListAsync(cancellationToken);
    }

    public async Task<UserSession> AddAsync(UserSession entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.UserSessions.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task UpdateAsync(UserSession entity, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Update(entity);
    }

    public async Task DeleteAsync(UserSession entity, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Remove(entity);
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
        return await _context.UserSessions.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceKey, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Device != null && s.Device.DeviceKey == deviceKey && s.IsActive && s.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<UserSession> CreateAsync(UserSession userSession, CancellationToken cancellationToken = default)
    {
        return await AddAsync(userSession, cancellationToken);
    }

    public async Task ExpireSessionAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var session = await GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (session != null)
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(session, cancellationToken);
        }
    }

    public async Task ExpireAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _context.UserSessions.Where(s => s.UserId == userId && s.IsActive).ToListAsync(cancellationToken);
        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.UpdatedAt = DateTime.UtcNow;
        }
        if (sessions.Any())
        {
            _context.UserSessions.UpdateRange(sessions);
        }
    }

    public async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        var expiredSessions = await _context.UserSessions
            .Where(s => s.ExpiresAt <= DateTime.UtcNow || !s.IsActive)
            .ToListAsync(cancellationToken);
        if (expiredSessions.Any())
        {
            _context.UserSessions.RemoveRange(expiredSessions);
        }
    }
}