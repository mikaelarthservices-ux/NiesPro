using Auth.Domain.Entities;

namespace Auth.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task IncrementFailedLoginAttemptsAsync(Guid userId);
        Task UpdateLastLoginAsync(Guid userId, string ipAddress);
        Task SaveChangesAsync();
    }

    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id);
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }

    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id);
        Task<Device?> GetByDeviceKeyAsync(string deviceKey);
        Task<Device?> GetByKeyAsync(string deviceKey);
        Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId);
        Task<Device> CreateAsync(Device device);
        Task<Device> UpdateAsync(Device device);
        Task<bool> DeleteAsync(Guid id);
        Task UpdateLastUsedAsync(Guid deviceId);
        Task SaveChangesAsync();
    }

    public interface IUserSessionRepository
    {
        Task<UserSession?> GetByIdAsync(Guid id);
        Task<UserSession?> GetByTokenAsync(string token);
        Task<UserSession?> GetActiveSessionAsync(Guid userId, Guid deviceId);
        Task<IEnumerable<UserSession>> GetByUserIdAsync(Guid userId);
        Task<UserSession> CreateAsync(UserSession session);
        Task<UserSession> UpdateAsync(UserSession session);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> DeactivateSessionAsync(Guid sessionId);
        Task SaveChangesAsync();
    }
}