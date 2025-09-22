using MediatR;
using NiesPro.Contracts.Common;

namespace Auth.Application.Features.Users.Queries.GetAllUsers
{
    /// <summary>
    /// Get all users query with pagination and filtering
    /// </summary>
    public class GetAllUsersQuery : IRequest<ApiResponse<GetAllUsersResponse>>
    {
        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term for username, email, first name, or last name
        /// </summary>
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
        public string? RoleName { get; set; }

        /// <summary>
        /// Sort field (Username, Email, CreatedAt, etc.)
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Sort direction (Asc, Desc)
        /// </summary>
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
