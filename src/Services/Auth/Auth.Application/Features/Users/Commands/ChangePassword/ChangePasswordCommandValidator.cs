using FluentValidation;
using Auth.Domain.Interfaces;
using Auth.Application.Contracts.Services;

namespace Auth.Application.Features.Users.Commands.ChangePassword
{
    /// <summary>
    /// Change password command validator with security rules
    /// </summary>
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public ChangePasswordCommandValidator(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;

            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // User ID validation
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required")
                .MustAsync(UserExists)
                .WithMessage("User not found or inactive");

            // Current password validation
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required")
                .MinimumLength(1)
                .WithMessage("Current password cannot be empty");

            // New password validation - Complex requirements
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("New password is required")
                .MinimumLength(8)
                .WithMessage("New password must be at least 8 characters long")
                .MaximumLength(128)
                .WithMessage("New password cannot exceed 128 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)");

            // Confirm password validation
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Password confirmation is required")
                .Equal(x => x.NewPassword)
                .WithMessage("Password confirmation must match the new password");

            // Device key validation
            RuleFor(x => x.DeviceKey)
                .NotEmpty()
                .WithMessage("Device key is required")
                .Length(10, 100)
                .WithMessage("Device key must be between 10 and 100 characters");

            // Device name validation (optional)
            RuleFor(x => x.DeviceName)
                .MaximumLength(100)
                .WithMessage("Device name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.DeviceName));

            // IP Address validation (optional)
            RuleFor(x => x.IpAddress)
                .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$|^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
                .WithMessage("Invalid IP address format")
                .When(x => !string.IsNullOrEmpty(x.IpAddress));

            // Business rules validation
            RuleFor(x => x)
                .MustAsync(CurrentPasswordIsCorrect)
                .WithMessage("Current password is incorrect");

            RuleFor(x => x)
                .Must(NewPasswordIsDifferent)
                .WithMessage("New password must be different from current password");
        }

        /// <summary>
        /// Validates that the user exists and is active
        /// </summary>
        private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                return user != null && user.IsActive;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that the current password is correct
        /// </summary>
        private async Task<bool> CurrentPasswordIsCorrect(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
                if (user == null || !user.IsActive)
                    return false;

                return _passwordService.VerifyPassword(command.CurrentPassword, user.PasswordHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that the new password is different from current password
        /// </summary>
        private bool NewPasswordIsDifferent(ChangePasswordCommand command)
        {
            return !string.Equals(command.CurrentPassword, command.NewPassword, StringComparison.Ordinal);
        }
    }
}
