using FluentValidation;

namespace Auth.Application.Features.Users.Queries.GetAllUsers
{
    /// <summary>
    /// Get all users query validator
    /// </summary>
    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        private readonly string[] _validSortFields = { "Username", "Email", "FirstName", "LastName", "CreatedAt", "UpdatedAt" };
        private readonly string[] _validSortDirections = { "Asc", "Desc", "asc", "desc" };

        public GetAllUsersQueryValidator()
        {
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // Pagination validation
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size cannot exceed 100 for performance reasons");

            // Search term validation
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            // Role name validation
            RuleFor(x => x.RoleName)
                .MaximumLength(50)
                .WithMessage("Role name cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.RoleName));

            // Sort field validation
            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage($"Sort field must be one of: {string.Join(", ", _validSortFields)}");

            // Sort direction validation
            RuleFor(x => x.SortDirection)
                .Must(BeValidSortDirection)
                .WithMessage("Sort direction must be 'Asc' or 'Desc'");

            // Business rule: Limit concurrent large requests
            RuleFor(x => x)
                .Must(x => !(x.PageSize > 50 && x.IncludeRoles && x.IncludeDeviceCount))
                .WithMessage("Large page sizes with multiple includes can impact performance. Please reduce page size or includes.");
        }

        /// <summary>
        /// Validates sort field is supported
        /// </summary>
        private bool BeValidSortField(string sortField)
        {
            return _validSortFields.Contains(sortField, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates sort direction is supported
        /// </summary>
        private bool BeValidSortDirection(string sortDirection)
        {
            return _validSortDirections.Contains(sortDirection, StringComparer.OrdinalIgnoreCase);
        }
    }
}
