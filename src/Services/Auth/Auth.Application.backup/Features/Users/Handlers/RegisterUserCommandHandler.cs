using Auth.Application.Features.Users.Commands.RegisterUser;
using Auth.Domain.Interfaces;
using BuildingBlocks.Common.Models;
using BuildingBlocks.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Domain.Entities;
using BuildingBlocks.Common.Constants;

namespace Auth.Application.Features.Users.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordService passwordService,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<ApiResponse<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Vérification si l'email existe déjà
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentative d'inscription avec un email déjà existant: {Email}", request.Email);
                return ApiResponse<RegisterUserResponse>.Error("Un compte avec cet email existe déjà");
            }

            // Hachage du mot de passe
            var passwordHash = _passwordService.HashPassword(request.Password);

            // Création de l'utilisateur
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = passwordHash,
                IsActive = true,
                EmailConfirmed = false, // À confirmer par email
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            // Attribution du rôle par défaut (User)
            var defaultRole = await _roleRepository.GetByNameAsync(AppConstants.Roles.USER);
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id,
                    AssignedAt = DateTime.UtcNow
                };

                user.UserRoles.Add(userRole);
                await _userRepository.UpdateAsync(user);
            }

            var response = new RegisterUserResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Message = "Inscription réussie. Veuillez confirmer votre email."
            };

            _logger.LogInformation("Nouvel utilisateur inscrit: {Email}", request.Email);
            return ApiResponse<RegisterUserResponse>.Success(response, "Inscription réussie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription de l'utilisateur: {Email}", request.Email);
            return ApiResponse<RegisterUserResponse>.Error("Erreur interne du serveur");
        }
    }
}