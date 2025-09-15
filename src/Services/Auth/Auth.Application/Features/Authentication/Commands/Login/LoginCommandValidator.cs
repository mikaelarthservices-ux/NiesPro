using FluentValidation;
using Auth.Application.Features.Authentication.Commands.Login;

namespace Auth.Application.Features.Authentication.Commands.Login
{
    /// <summary>
    /// Login command validator
    /// </summary>
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Valid email address is required")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

            RuleFor(x => x.DeviceKey)
                .NotEmpty().WithMessage("Device key is required")
                .MaximumLength(512).WithMessage("Device key must not exceed 512 characters");

            RuleFor(x => x.IpAddress)
                .MaximumLength(45).WithMessage("IP address must not exceed 45 characters"); // IPv6 max length

            RuleFor(x => x.UserAgent)
                .MaximumLength(1000).WithMessage("User agent must not exceed 1000 characters");
        }
    }
}
