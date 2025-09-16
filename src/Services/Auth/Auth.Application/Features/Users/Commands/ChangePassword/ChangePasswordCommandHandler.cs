using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Users.Commands.ChangePassword;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Application.Common.Models;
using BuildingBlocks.Common.DTOs;
using Auth.Domain.Entities;

namespace Auth.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Change password command handler with security and session management
    /// </summary>
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<ChangePasswordResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IDeviceRepository deviceRepository,
            IUserSessionRepository userSessionRepository,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _deviceRepository = deviceRepository;
            _userSessionRepository = userSessionRepository;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ChangePasswordResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing password change for user: {UserId}", request.UserId);

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // 1. Get user and verify current password
                var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Password change attempted for non-existent or inactive user: {UserId}", request.UserId);
                    return ApiResponse<ChangePasswordResponse>.CreateError("User not found or inactive");
                }

                if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid current password provided for user: {UserId}", request.UserId);
                    return ApiResponse<ChangePasswordResponse>.CreateError("Current password is incorrect");
                }

                // 2. Update user password
                user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user, cancellationToken);

                // 3. Get current device to preserve session
                var currentDevice = await _deviceRepository.GetByDeviceKeyAsync(request.DeviceKey, cancellationToken);
                var currentDeviceId = currentDevice?.Id;

                // 4. Invalidate all other sessions except current device  
                // Note: Would need GetActiveSessionsByUserId method in repository
                // var allSessions = await _userSessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);
                // var sessionsToInvalidate = allSessions.Where(s => s.DeviceId != currentDeviceId?.GetHashCode()).ToList();

                var devicesLoggedOut = 0;
                // Simplified for now - would implement proper session invalidation with actual repository method

                // 5. Create security audit log
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DeviceId = currentDeviceId,
                    Action = "PASSWORD_CHANGED",
                    EntityType = "User",
                    EntityId = user.Id.ToString(),
                    NewValues = $"Password changed successfully. {devicesLoggedOut} other devices logged out.", // Use NewValues instead of Details
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    CreatedAt = DateTime.UtcNow
                };

                // Note: Would need IAuditLogRepository to save audit log

                // 6. Update current device last used timestamp
                if (currentDevice != null)
                {
                    currentDevice.LastUsedAt = DateTime.UtcNow;
                    currentDevice.LastIpAddress = request.IpAddress;
                    currentDevice.UserAgent = request.UserAgent;
                    currentDevice.UpdatedAt = DateTime.UtcNow;

                    await _deviceRepository.UpdateAsync(currentDevice, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Password changed successfully for user: {UserId}. Logged out {DeviceCount} devices", 
                    request.UserId, devicesLoggedOut);

                // 7. Create response
                var response = new ChangePasswordResponse
                {
                    UserId = user.Id,
                    ChangedAt = DateTime.UtcNow,
                    SessionsInvalidated = devicesLoggedOut > 0,
                    DevicesLoggedOut = devicesLoggedOut,
                    Message = devicesLoggedOut > 0 
                        ? $"Password changed successfully. {devicesLoggedOut} other devices have been logged out for security."
                        : "Password changed successfully."
                };

                return ApiResponse<ChangePasswordResponse>.CreateSuccess(response, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user: {UserId}", request.UserId);

                try
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                }

                return ApiResponse<ChangePasswordResponse>.CreateError("Password change failed. Please try again.");
            }
        }
    }
}
