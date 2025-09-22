using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Authentication.Commands.Login;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Common;
using Auth.Domain.Entities;

namespace Auth.Application.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Login command handler
    /// </summary>
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IDeviceRepository deviceRepository,
            IUserSessionRepository userSessionRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IUnitOfWork unitOfWork,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _deviceRepository = deviceRepository;
            _userSessionRepository = userSessionRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing login request for email: {Email}", request.Email);

                // 1. Validate user credentials
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.CreateError("Invalid email or password");
                }

                // 2. Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
                    return ApiResponse<LoginResponse>.CreateError("Account is deactivated");
                }

                // 3. Validate device
                var device = await _deviceRepository.GetByDeviceKeyAsync(request.DeviceKey, cancellationToken);
                if (device == null)
                {
                    _logger.LogWarning("Invalid device attempt for user: {UserId}", user.Id);
                    return ApiResponse<LoginResponse>.CreateError("Device not authorized");
                }

                // 4. Generate tokens
                var userRoles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
                var accessToken = _jwtService.GenerateToken(user.Id, user.Email, userRoles);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var expiresAt = _jwtService.GetTokenExpiration(accessToken);

                // 5. Create user session
                var userSession = new UserSession
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DeviceId = device.Id,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userSessionRepository.CreateAsync(userSession, cancellationToken);

                // 6. Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successful login for user: {UserId}", user.Id);

                // 7. Create response
                var loginResponse = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    ExpiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds,
                    TokenType = "Bearer",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        FirstName = user.FirstName ?? string.Empty,
                        LastName = user.LastName ?? string.Empty,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Roles = userRoles
                    }
                };

                return ApiResponse<LoginResponse>.CreateSuccess(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing login request for email: {Email}", request.Email);
                return ApiResponse<LoginResponse>.CreateError("An error occurred during login");
            }
        }
    }
}
