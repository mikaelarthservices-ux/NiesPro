using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Authentication.Commands.RefreshToken;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Common;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Logging.Client;

namespace Auth.Application.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Refresh token command handler - NiesPro Enterprise Standard with BaseCommandHandler
    /// </summary>
    public class RefreshTokenCommandHandler : BaseCommandHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
    {
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogsServiceClient _logsService;
        private readonly IAuditServiceClient _auditService;

        public RefreshTokenCommandHandler(
            IUserSessionRepository userSessionRepository,
            IUserRepository userRepository,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            ILogger<RefreshTokenCommandHandler> logger,
            ILogsServiceClient logsService,
            IAuditServiceClient auditService)
            : base(logger) // NiesPro Enterprise: BaseCommandHandler inheritance
        {
            _userSessionRepository = userSessionRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _logsService = logsService;
            _auditService = auditService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseCommandHandler
        /// </summary>
        public async Task<ApiResponse<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await HandleAsync(request, cancellationToken);
        }

        /// <summary>
        /// NiesPro Enterprise: Execute business logic with automatic logging
        /// </summary>
        protected override async Task<ApiResponse<RefreshTokenResponse>> ExecuteAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Processing token refresh request");

                // 1. Find session by refresh token
                var userSession = await _userSessionRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
                if (userSession == null)
                {
                    Logger.LogWarning("Invalid refresh token provided");
                    return ApiResponse<RefreshTokenResponse>.CreateError("Invalid refresh token");
                }

                // 2. Check if session is active and not expired
                if (!userSession.IsActive || userSession.ExpiresAt <= DateTime.UtcNow)
                {
                    Logger.LogWarning("Refresh token expired or inactive for session: {SessionId}", userSession.Id);
                    
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
                    Logger.LogWarning("User not found or inactive for session: {SessionId}", userSession.Id);
                    return ApiResponse<RefreshTokenResponse>.CreateError("User not found or inactive");
                }

                // 4. Generate new tokens
                var userRoles = user.UserRoles?.Select(ur => ur.Role?.Name).Where(name => !string.IsNullOrEmpty(name)).Cast<string>().ToList() ?? new List<string>();
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

                Logger.LogInformation("Token refresh successful for user: {UserId}", user.Id);

                // NiesPro Enterprise: Audit token refresh
                await _auditService.AuditUpdateAsync(
                    userId: user.Id.ToString(),
                    userName: user.Username,
                    entityName: "UserSession",
                    entityId: userSession.Id.ToString(),
                    metadata: new Dictionary<string, object>
                    {
                        { "RefreshTime", DateTime.UtcNow },
                        { "IpAddress", request.IpAddress ?? string.Empty },
                        { "UserAgent", request.UserAgent ?? string.Empty },
                        { "SessionId", userSession.Id.ToString() }
                    });

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
                Logger.LogError(ex, "Error during token refresh");
                
                // NiesPro Enterprise: Log error centrally
                await _logsService.LogErrorAsync(ex, 
                    "Error during token refresh",
                    properties: new Dictionary<string, object>
                    {
                        { "RefreshToken", request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length)) + "..." },
                        { "IpAddress", request.IpAddress ?? string.Empty },
                        { "UserAgent", request.UserAgent ?? string.Empty }
                    });
                
                return ApiResponse<RefreshTokenResponse>.CreateError("Token refresh failed");
            }
        }
    }
}