using Auth.Application.Features.Users.Queries.GetUserProfile;
using Auth.Domain.Interfaces;
using BuildingBlocks.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Users.Handlers;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<UserProfileDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Utilisateur non trouvé: {UserId}", request.UserId);
                return ApiResponse<UserProfileDto>.Error("Utilisateur non trouvé");
            }

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Permissions = user.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToList()
            };

            return ApiResponse<UserProfileDto>.Success(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du profil utilisateur: {UserId}", request.UserId);
            return ApiResponse<UserProfileDto>.Error("Erreur interne du serveur");
        }
    }
}