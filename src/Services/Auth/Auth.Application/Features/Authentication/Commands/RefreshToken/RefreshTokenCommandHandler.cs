using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Authentication.Commands.RefreshToken;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Common;

namespace Auth.Application.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Refresh token command handler for token renewal
    /// </summary>
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IUserSessionRepository userSessionRepository,
            IUserRepository userRepository,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _userSessionRepository = userSessionRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing token refresh request");

                // 1. Find session by refresh token
                var userSession = await _userSessionRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
                if (userSession == null)
                {
                    _logger.LogWarning("Invalid refresh token provided");
                    return ApiResponse<RefreshTokenResponse>.CreateError("Invalid refresh token");
                }

                // 2. Check if session is active and not expired
                if (!userSession.IsActive || userSession.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired or inactive for session: {SessionId}", userSession.Id);
                    
                    // Deactivate expired session
                    userSession.IsActive = false;
                    await _userSessionRepository.UpdateAsync(userSession, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    return ApiResponse<RefreshTokenResponse>.CreateError("Refresh token expired");
                }

                // 3. Get user details
                var user = await _userRepository.GetByIdAsync(userSession.UserId, cancellationToken);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive for session: {SessionId}", userSession.Id);
                    return ApiResponse<RefreshTokenResponse>.CreateError("User not found or inactive");
                }

                // 4. Generate new tokens
                var userRoles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                var newAccessToken = _jwtService.GenerateToken(user.Id, user.Email, userRoles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newExpiresAt = _jwtService.GetTokenExpiration(newAccessToken);

                // 5. Update session with new tokens
                userSession.AccessToken = newAccessToken;
                userSession.RefreshToken = newRefreshToken;
                userSession.ExpiresAt = newExpiresAt;
                userSession.LastActivityAt = DateTime.UtcNow;
                userSession.IpAddress = request.IpAddress;
                userSession.UserAgent = request.UserAgent;

                await _userSessionRepository.UpdateAsync(userSession, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);

                // 6. Create response
                var response = new RefreshTokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = newExpiresAt,
                    ExpiresIn = (int)(newExpiresAt - DateTime.UtcNow).TotalSeconds,
                    TokenType = "Bearer"
                };

                return ApiResponse<RefreshTokenResponse>.CreateSuccess(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<RefreshTokenResponse>.CreateError("Token refresh failed");
            }
        }
    }
}