namespace Auth.Application.Contracts.Services
{
    /// <summary>
    /// Password service contract for password operations
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Hash a plain text password
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify if a password matches the hashed version
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);

        /// <summary>
        /// Generate a random password
        /// </summary>
        string GenerateRandomPassword(int length = 12);
    }

    /// <summary>
    /// JWT service contract for token operations
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate an access token for a user
        /// </summary>
        string GenerateToken(Guid userId, string email, List<string> roles);

        /// <summary>
        /// Generate a refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate a JWT token
        /// </summary>
        bool ValidateToken(string token);

        /// <summary>
        /// Get user ID from a valid token
        /// </summary>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// Get token expiration date
        /// </summary>
        DateTime GetTokenExpiration(string token);
    }

    /// <summary>
    /// Unit of Work pattern for transaction management
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
