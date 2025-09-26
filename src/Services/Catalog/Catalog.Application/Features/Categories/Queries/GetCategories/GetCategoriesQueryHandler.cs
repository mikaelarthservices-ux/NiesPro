using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Categories.Queries.GetCategories
{
    /// <summary>
    /// Handler for GetCategoriesQuery - NiesPro Enterprise Implementation
    /// </summary>
    public class GetCategoriesQueryHandler : 
        BaseQueryHandler<GetCategoriesQuery, ApiResponse<IEnumerable<CategoryDto>>>,
        IRequestHandler<GetCategoriesQuery, ApiResponse<IEnumerable<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoriesQueryHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public GetCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoriesQueryHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseQueryHandler
        /// </summary>
        public async Task<ApiResponse<IEnumerable<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute get categories query - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<IEnumerable<CategoryDto>>> ExecuteAsync(GetCategoriesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Retrieving categories - RootOnly: {query.RootOnly}, IncludeInactive: {query.IncludeInactive}", new Dictionary<string, object>
                {
                    ["RootOnly"] = query.RootOnly ?? false,
                    ["IncludeInactive"] = query.IncludeInactive ?? false,
                    ["QueryId"] = query.QueryId
                });

                // 1. Get categories from repository
                IEnumerable<Catalog.Domain.Entities.Category> categories;

                if (query.RootOnly == true)
                {
                    categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
                }
                else
                {
                    categories = await _categoryRepository.GetAllAsync(cancellationToken);
                }

                // 2. Filter by active status if needed
                if (query.IncludeInactive != true)
                {
                    categories = categories.Where(c => c.IsActive);
                }

                // 3. Convert to DTOs
                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ImageUrl = c.ImageUrl,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId,
                    ProductCount = c.Products?.Count ?? 0,
                    CreatedAt = c.CreatedAt
                }).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();

                // NiesPro Enterprise: Enhanced audit trail for successful retrieval
                await _logsService.LogInformationAsync($"Successfully retrieved {categoryDtos.Count} categories", new Dictionary<string, object>
                {
                    ["RetrievedCount"] = categoryDtos.Count,
                    ["RootOnly"] = query.RootOnly ?? false,
                    ["IncludeInactive"] = query.IncludeInactive ?? false,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                    categoryDtos, 
                    $"Retrieved {categoryDtos.Count} categories successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while retrieving categories", new Dictionary<string, object>
                {
                    ["RootOnly"] = query.RootOnly ?? false,
                    ["IncludeInactive"] = query.IncludeInactive ?? false,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<IEnumerable<CategoryDto>>.CreateError("An error occurred while retrieving categories");
            }
        }
    }
}