using FluentValidation;
using Auth.Domain.Interfaces;

namespace Auth.Application.Features.Users.Queries.GetUserProfile
{
    /// <summary>
    /// Get user profile query validator
    /// </summary>
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        private readonly IUserRepository _userRepository;

        public GetUserProfileQueryValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

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

            // Include flags validation (optional business rules)
            RuleFor(x => x)
                .Must(x => x.IncludeRoles || x.IncludePermissions || x.IncludeDevices || true)
                .WithMessage("At least one include option should be specified for efficiency")
                .When(x => !x.IncludeRoles && !x.IncludePermissions && !x.IncludeDevices);
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
    }
}
