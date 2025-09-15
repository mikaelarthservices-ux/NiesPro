namespace Auth.Application.Features.Users.Queries.GetAllUsers
{
    /// <summary>
    /// Get all users response with pagination
    /// </summary>
    public class GetAllUsersResponse
    {
        /// <summary>
        /// List of users
        /// </summary>
        public List<UserSummaryDto> Users { get; set; } = new();

        /// <summary>
        /// Pagination metadata
        /// </summary>
        public PaginationMetadata Pagination { get; set; } = new();

        /// <summary>
        /// Applied filters summary
        /// </summary>
        public FilterSummary? Filters { get; set; }
    }

    /// <summary>
    /// User summary DTO for list view
    /// </summary>
    public class UserSummaryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// User roles (if included)
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// Active device count (if included)
        /// </summary>
        public int? DeviceCount { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Full display name
        /// </summary>
        public string DisplayName => 
            !string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName)
                ? $"{FirstName} {LastName}".Trim()
                : Username;
    }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// Applied filters summary
    /// </summary>
    public class FilterSummary
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? EmailConfirmed { get; set; }
        public string? RoleName { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
    }
}
