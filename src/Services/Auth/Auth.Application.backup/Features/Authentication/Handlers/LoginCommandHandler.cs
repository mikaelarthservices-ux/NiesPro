using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Application.Interfaces;
using Auth.Application.Common;
using Auth.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Domain.Entities;

namespace Auth.Application.Features.Authentication.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        IUserSessionRepository userSessionRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _userSessionRepository = userSessionRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation de l'utilisateur
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Tentative de connexion avec un email inexistant: {Email}", request.Email);
                return ApiResponse<LoginResponse>.Error("Email ou mot de passe incorrect");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Tentative de connexion avec un compte désactivé: {Email}", request.Email);
                return ApiResponse<LoginResponse>.Error("Compte désactivé");
            }

            // Vérification du mot de passe
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Mot de passe incorrect pour l'utilisateur: {Email}", request.Email);
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id);
                return ApiResponse<LoginResponse>.Error("Email ou mot de passe incorrect");
            }

            // Validation de l'appareil si nécessaire
            Device? device = null;
            if (!string.IsNullOrEmpty(request.DeviceKey))
            {
                device = await _deviceRepository.GetByKeyAsync(request.DeviceKey);
                if (device == null || device.UserId != user.Id || !device.IsActive)
                {
                    _logger.LogWarning("Clé d'appareil invalide pour l'utilisateur: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.Error("Appareil non autorisé");
                }

                // Mise à jour de la dernière utilisation de l'appareil
                await _deviceRepository.UpdateLastUsedAsync(device.Id);
            }

            // Génération des tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Création de la session
            var session = new UserSession
            {
                UserId = user.Id,
                DeviceId = device?.Id,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // À configurer
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent
            };

            await _userSessionRepository.CreateAsync(session);

            // Mise à jour des informations de connexion
            await _userRepository.UpdateLastLoginAsync(user.Id, request.IpAddress);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // À configurer
                TokenType = "Bearer",
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
                }
            };

            _logger.LogInformation("Connexion réussie pour l'utilisateur: {Email}", request.Email);
            return ApiResponse<LoginResponse>.Success(response, "Connexion réussie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion pour l'utilisateur: {Email}", request.Email);
            return ApiResponse<LoginResponse>.Error("Erreur interne du serveur");
        }
    }
}