using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Users.Commands.RegisterUser;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Common;
using Auth.Domain.Entities;
using NiesPro.Logging.Client;

namespace Auth.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Register user command handler with complete business logic
    /// </summary>
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiResponse<RegisterUserResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        private readonly ILogsServiceClient _logsService;
        private readonly IAuditServiceClient _auditService;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDeviceRepository deviceRepository,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork,
            ILogger<RegisterUserCommandHandler> logger,
            ILogsServiceClient logsService,
            IAuditServiceClient auditService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _deviceRepository = deviceRepository;
            _passwordService = passwordService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _logsService = logsService;
            _auditService = auditService;
        }

        public async Task<ApiResponse<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing user registration for email: {Email}", request.Email);

                // 1. Validate email uniqueness
                var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return ApiResponse<RegisterUserResponse>.CreateError("Email is already registered");
                }

                // 2. Validate username uniqueness
                var existingUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
                if (existingUsername != null)
                {
                    _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
                    return ApiResponse<RegisterUserResponse>.CreateError("Username is already taken");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // 3. Create new user entity
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email.ToLowerInvariant(),
                    PasswordHash = _passwordService.HashPassword(request.Password),
                    FirstName = request.FirstName?.Trim(),
                    LastName = request.LastName?.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    IsActive = true,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                // 4. Save user to database
                var createdUser = await _userRepository.AddAsync(user, cancellationToken);

                // 5. Assign default "User" role - TODO: Implement UserRole repository
                // var defaultRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
                // if (defaultRole != null)
                // {
                //     var userRole = new UserRole
                //     {
                //         UserId = createdUser.Id,
                //         RoleId = defaultRole.Id,
                //         AssignedAt = DateTime.UtcNow
                //     };
                //     await _userRoleRepository.AddAsync(userRole, cancellationToken);
                //     _logger.LogInformation("Default role assigned to user: {UserId}", createdUser.Id);
                // }

                // 6. Register user's device
                var device = new Device
                {
                    Id = Guid.NewGuid(),
                    DeviceKey = request.DeviceKey,
                    DeviceName = request.DeviceName,
                    DeviceType = DeviceType.Desktop, // Could be detected from UserAgent
                    UserId = createdUser.Id,
                    IsActive = true,
                    LastUsedAt = DateTime.UtcNow,
                    LastIpAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    CreatedAt = DateTime.UtcNow
                };

                await _deviceRepository.AddAsync(device, cancellationToken);

                // 7. Enregistrer dans l'audit centralisé NiesPro
                await _auditService.AuditCreateAsync(
                    userId: createdUser.Id.ToString(),
                    userName: createdUser.Username,
                    entityName: "User",
                    entityId: createdUser.Id.ToString(),
                    metadata: new Dictionary<string, object>
                    {
                        { "Email", createdUser.Email },
                        { "Username", createdUser.Username },
                        { "FirstName", createdUser.FirstName ?? string.Empty },
                        { "LastName", createdUser.LastName ?? string.Empty },
                        { "IpAddress", request.IpAddress ?? string.Empty },
                        { "UserAgent", request.UserAgent ?? string.Empty },
                        { "DeviceId", device.Id.ToString() }
                    });

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("User registration successful for: {UserId}", createdUser.Id);
                
                // Log centralizedNiesPro 
                await _logsService.LogAsync(
                    LogLevel.Information, 
                    $"User registration successful for UserId: {createdUser.Id}",
                    properties: new Dictionary<string, object>
                    {
                        { "UserId", createdUser.Id },
                        { "Email", createdUser.Email },
                        { "Username", createdUser.Username },
                        { "RegistrationTime", createdUser.CreatedAt },
                        { "IpAddress", request.IpAddress ?? string.Empty },
                        { "UserAgent", request.UserAgent ?? string.Empty }
                    });

                // 8. Create response
                var response = new RegisterUserResponse
                {
                    UserId = createdUser.Id,
                    Email = createdUser.Email,
                    Username = createdUser.Username,
                    CreatedAt = createdUser.CreatedAt
                };

                return ApiResponse<RegisterUserResponse>.CreateSuccess(response, "User registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
                
                // Log d'erreur centralisé NiesPro
                await _logsService.LogErrorAsync(ex, 
                    $"Error during user registration for email: {request.Email}",
                    properties: new Dictionary<string, object>
                    {
                        { "Email", request.Email },
                        { "Username", request.Username },
                        { "IpAddress", request.IpAddress ?? string.Empty },
                        { "UserAgent", request.UserAgent ?? string.Empty }
                    });
                
                try
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Error during transaction rollback");
                    await _logsService.LogErrorAsync(rollbackEx, "Error during transaction rollback");
                }

                return ApiResponse<RegisterUserResponse>.CreateError("Registration failed. Please try again.");
            }
        }
    }
}
