using MediatR;
using Microsoft.Extensions.Logging;
using Auth.Application.Features.Users.Queries.GetAllUsers;
using Auth.Domain.Interfaces;
using Auth.Application.Common.Models;
using NiesPro.Contracts.Common;
using Auth.Domain.Entities;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Logging.Client;

namespace Auth.Application.Features.Users.Queries.GetAllUsers
{
    /// <summary>
    /// Get all users query handler - NiesPro Enterprise Standard with BaseQueryHandler
    /// </summary>
    public class GetAllUsersQueryHandler : BaseQueryHandler<GetAllUsersQuery, ApiResponse<GetAllUsersResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogsServiceClient _logsService;

        public GetAllUsersQueryHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IDeviceRepository deviceRepository,
            ILogger<GetAllUsersQueryHandler> logger,
            ILogsServiceClient logsService)
            : base(logger) // NiesPro Enterprise: BaseQueryHandler inheritance
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _deviceRepository = deviceRepository;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseQueryHandler
        /// </summary>
        public async Task<ApiResponse<GetAllUsersResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await HandleAsync(request, cancellationToken);
        }

        /// <summary>
        /// NiesPro Enterprise: Execute business logic with automatic logging
        /// </summary>
        protected override async Task<ApiResponse<GetAllUsersResponse>> ExecuteAsync(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Retrieving users list. Page: {Page}, Size: {Size}, Search: {Search}", 
                    request.PageNumber, request.PageSize, request.SearchTerm ?? "None");

                // 1. Build query parameters
                var queryParams = new UserQueryParameters
                {
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    IsActive = request.IsActive,
                    EmailConfirmed = request.EmailConfirmed,
                    RoleName = request.RoleName,
                    SortBy = request.SortBy,
                    SortDirection = request.SortDirection
                };

                // 2. Get paginated users
                var (users, totalCount) = await _userRepository.GetUsersAsync(request.PageNumber, request.PageSize, cancellationToken);

                // 3. Calculate pagination metadata
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
                var pagination = new PaginationMetadata
                {
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = request.PageNumber < totalPages,
                    HasPreviousPage = request.PageNumber > 1
                };

                // 4. Convert to DTOs
                var userDtos = new List<UserSummaryDto>();
                foreach (var user in users)
                {
                    var userDto = new UserSummaryDto
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt ?? user.CreatedAt
                    };

                    // 5. Load roles if requested
                    if (request.IncludeRoles)
                    {
                        var userRoles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
                        userDto.Roles = userRoles.Select(r => r.Name).ToList();
                    }

                    // 6. Load device count if requested
                    if (request.IncludeDeviceCount)
                    {
                        var deviceCount = await _deviceRepository.GetActiveDeviceCountByUserIdAsync(user.Id, cancellationToken);
                        userDto.DeviceCount = deviceCount;
                    }

                    // 7. Get last login (would need UserSession repository)
                    // userDto.LastLoginAt = await GetLastLoginAsync(user.Id, cancellationToken);

                    userDtos.Add(userDto);
                }

                // 8. Build response
                var response = new GetAllUsersResponse
                {
                    Users = userDtos,
                    Pagination = pagination,
                    Filters = new FilterSummary
                    {
                        SearchTerm = request.SearchTerm,
                        IsActive = request.IsActive,
                        EmailConfirmed = request.EmailConfirmed,
                        RoleName = request.RoleName,
                        SortBy = request.SortBy,
                        SortDirection = request.SortDirection
                    }
                };

                Logger.LogInformation("Successfully retrieved {UserCount} users out of {TotalCount} total", 
                    userDtos.Count, totalCount);

                // NiesPro Enterprise: Log query execution
                await _logsService.LogAsync(
                    Microsoft.Extensions.Logging.LogLevel.Information,
                    $"GetAllUsers query executed successfully - {userDtos.Count} users returned",
                    properties: new Dictionary<string, object>
                    {
                        { "PageNumber", request.PageNumber },
                        { "PageSize", request.PageSize },
                        { "SearchTerm", request.SearchTerm ?? string.Empty },
                        { "TotalCount", totalCount },
                        { "ReturnedCount", userDtos.Count }
                    });

                return ApiResponse<GetAllUsersResponse>.CreateSuccess(response, 
                    $"Retrieved {userDtos.Count} users successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving users list");
                
                // NiesPro Enterprise: Log error centrally
                await _logsService.LogErrorAsync(ex, 
                    "Error retrieving users list",
                    properties: new Dictionary<string, object>
                    {
                        { "PageNumber", request.PageNumber },
                        { "PageSize", request.PageSize },
                        { "SearchTerm", request.SearchTerm ?? string.Empty }
                    });
                
                return ApiResponse<GetAllUsersResponse>.CreateError("Failed to retrieve users list");
            }
        }

        /// <summary>
        /// Get user's last login timestamp from sessions
        /// </summary>
        private Task<DateTime?> GetLastLoginAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                // Would implement with UserSession repository
                // var lastSession = await _userSessionRepository.GetLastSessionByUserIdAsync(userId, cancellationToken);
                // return lastSession?.CreatedAt;
                return Task.FromResult<DateTime?>(null);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error getting last login for user: {UserId}", userId);
                return Task.FromResult<DateTime?>(null);
            }
        }
    }

    /// <summary>
    /// User query parameters for repository
    /// </summary>
    public class UserQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? EmailConfirmed { get; set; }
        public string? RoleName { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "Desc";
    }
}
