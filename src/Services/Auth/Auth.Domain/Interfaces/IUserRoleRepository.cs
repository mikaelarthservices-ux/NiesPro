using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for UserRole entity
    /// </summary>
    public interface IUserRoleRepository
    {
        Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
        Task<UserRole?> GetByUserAndRoleIdAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteAsync(UserRole userRole, CancellationToken cancellationToken = default);
    }
}