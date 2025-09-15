namespace Auth.API.Models.Responses
{
    /// <summary>
    /// Get all users API response model
    /// </summary>
    public class GetAllUsersApiResponse
    {
        /// <summary>
        /// List of users
        /// </summary>
        public List<UserSummary> Users { get; set; } = new();

        /// <summary>
        /// Pagination metadata
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new();

        /// <summary>
        /// Applied filters summary
        /// </summary>
        public FilterInfo? Filters { get; set; }
    }

    /// <summary>
    /// User summary model for list view
    /// </summary>
    public class UserSummary
    {
        /// <summary>
        /// User unique identifier
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Account active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Email confirmation status
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Account creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
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
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pagination information model
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// Applied filters information model
    /// </summary>
    public class FilterInfo
    {
        /// <summary>
        /// Search term used
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Active status filter
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Email confirmed filter
        /// </summary>
        public bool? EmailConfirmed { get; set; }

        /// <summary>
        /// Role name filter
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Sort field used
        /// </summary>
        public string SortBy { get; set; } = string.Empty;

        /// <summary>
        /// Sort direction used
        /// </summary>
        public string SortDirection { get; set; } = string.Empty;
    }
}