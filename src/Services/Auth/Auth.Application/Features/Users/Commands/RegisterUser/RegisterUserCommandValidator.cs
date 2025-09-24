using FluentValidation;
using Auth.Application.Features.Users.Commands.RegisterUser;
using Auth.Application.Contracts.Services;

namespace Auth.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Register user command validator with business rules
    /// </summary>
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IValidationService _validationService;

        public RegisterUserCommandValidator(IValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, dots, hyphens and underscores")
                .MustAsync(BeUniqueUsername).WithMessage("Username is already taken");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email address is required")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
                .MustAsync(BeUniqueEmail).WithMessage("Email address is already registered");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, one digit and one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(x => x.Password).WithMessage("Password and confirmation password do not match");

            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.DeviceKey)
                .NotEmpty().WithMessage("Device key is required")
                .MaximumLength(512).WithMessage("Device key must not exceed 512 characters");

            RuleFor(x => x.DeviceName)
                .NotEmpty().WithMessage("Device name is required")
                .MaximumLength(100).WithMessage("Device name must not exceed 100 characters");

            RuleFor(x => x.IpAddress)
                .MaximumLength(45).WithMessage("IP address must not exceed 45 characters");

            RuleFor(x => x.UserAgent)
                .MaximumLength(1000).WithMessage("User agent must not exceed 1000 characters");
        }

        private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
        {
            return await _validationService.IsUsernameUniqueAsync(username, cancellationToken);
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return await _validationService.IsEmailUniqueAsync(email, cancellationToken);
        }
    }
}
