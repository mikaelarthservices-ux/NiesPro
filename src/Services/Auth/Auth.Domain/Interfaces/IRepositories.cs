using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Interface de base pour les repositories
/// </summary>
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository User
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Role
/// </summary>
public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Permission
/// </summary>
public interface IPermissionRepository : IBaseRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> GetPermissionsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Device
/// </summary>
public interface IDeviceRepository : IBaseRepository<Device>
{
    Task<Device?> GetByDeviceKeyAsync(string deviceKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Device>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Device>> GetActiveDevicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Device>> GetTrustedDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Device>> GetDevicesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetActiveDeviceCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeviceKeyExistsAsync(string deviceKey, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceTrustedAsync(string deviceKey, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository UserSession
/// </summary>
public interface IUserSessionRepository : IBaseRepository<UserSession>
{
    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceKey, CancellationToken cancellationToken = default);
    Task<UserSession> CreateAsync(UserSession userSession, CancellationToken cancellationToken = default);
    Task ExpireSessionAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ExpireAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository AuditLog
/// </summary>
public interface IAuditLogRepository : IBaseRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task CleanupOldLogsAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
}