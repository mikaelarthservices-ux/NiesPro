using Microsoft.EntityFrameworkCore;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AuthDbContext _context;

    public DeviceRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices.ToListAsync(cancellationToken);
    }

    public async Task<Device> AddAsync(Device entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.Devices.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task UpdateAsync(Device entity, CancellationToken cancellationToken = default)
    {
        _context.Devices.Update(entity);
    }

    public async Task DeleteAsync(Device entity, CancellationToken cancellationToken = default)
    {
        _context.Devices.Remove(entity);
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
        return await _context.Devices.FindAsync(new object[] { id }, cancellationToken) != null;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.Where(d => d.UserId == userId && !d.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Devices.Where(d => d.IsActive && !d.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetTrustedDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.Where(d => d.UserId == userId && d.IsTrusted && !d.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Device>> GetDevicesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<int> GetActiveDeviceCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.CountAsync(d => d.UserId == userId && d.IsActive && !d.IsDeleted, cancellationToken);
    }

    public async Task<bool> DeviceKeyExistsAsync(string deviceKey, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.AnyAsync(d => d.DeviceKey == deviceKey, cancellationToken);
    }

    public async Task<bool> IsDeviceTrustedAsync(string deviceKey, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Devices.AnyAsync(d => d.DeviceKey == deviceKey && d.UserId == userId && d.IsTrusted, cancellationToken);
    }
}