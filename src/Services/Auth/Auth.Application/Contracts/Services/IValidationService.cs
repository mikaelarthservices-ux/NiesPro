namespace Auth.Application.Contracts.Services;

/// <summary>
/// Service for validating unique constraints without causing DbContext concurrency issues
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Checks if an email address is unique (not already registered)
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a username is unique (not already taken)
    /// </summary>
    Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
}