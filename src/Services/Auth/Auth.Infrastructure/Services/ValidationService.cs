using Auth.Application.Contracts.Services;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services
{
    /// <summary>
    /// Service for validating unique constraints with proper scope management
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(IServiceScopeFactory scopeFactory, ILogger<ValidationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            try
            {
                var exists = await userRepository.EmailExistsAsync(email, cancellationToken);
                return !exists;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking email uniqueness for {Email}", email);
                // En cas d'erreur, on considère que l'email n'est pas unique pour la sécurité
                return false;
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            try
            {
                var exists = await userRepository.ExistsByUsernameAsync(username, cancellationToken);
                return !exists;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking username uniqueness for {Username}", username);
                // En cas d'erreur, on considère que le username n'est pas unique pour la sécurité
                return false;
            }
        }
    }
}