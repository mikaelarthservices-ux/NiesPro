using System.ComponentModel.DataAnnotations;

namespace Auth.API.Models.Requests
{
    /// <summary>
    /// Get all users request model with pagination and filtering
    /// </summary>
    public class GetAllUsersRequest
    {
        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term for username, email, first name, or last name
        /// </summary>
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filter by email confirmed status
        /// </summary>
        public bool? EmailConfirmed { get; set; }

        /// <summary>
        /// Filter by role name
        /// </summary>
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public string? RoleName { get; set; }

        /// <summary>
        /// Sort field (Username, Email, CreatedAt, etc.)
        /// </summary>
        [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Sort direction (Asc, Desc)
        /// </summary>
        [RegularExpression("^(Asc|Desc|asc|desc)$", ErrorMessage = "Sort direction must be 'Asc' or 'Desc'")]
        public string SortDirection { get; set; } = "Desc";

        /// <summary>
        /// Include user roles in response
        /// </summary>
        public bool IncludeRoles { get; set; } = false;

        /// <summary>
        /// Include device count in response
        /// </summary>
        public bool IncludeDeviceCount { get; set; } = false;
    }
}