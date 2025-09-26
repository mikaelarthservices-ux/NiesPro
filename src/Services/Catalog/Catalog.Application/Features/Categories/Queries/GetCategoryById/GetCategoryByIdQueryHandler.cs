using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryById
{
    /// <summary>
    /// Handler for GetCategoryByIdQuery - NiesPro Enterprise Implementation
    /// </summary>
    public class GetCategoryByIdQueryHandler : 
        BaseQueryHandler<GetCategoryByIdQuery, ApiResponse<CategoryDto>>,
        IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public GetCategoryByIdQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoryByIdQueryHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseQueryHandler
        /// </summary>
        public async Task<ApiResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute get category by ID query - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<CategoryDto>> ExecuteAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Retrieving category with ID: {query.Id}", new Dictionary<string, object>
                {
                    ["CategoryId"] = query.Id,
                    ["QueryId"] = query.QueryId
                });

                // 1. Get category from repository
                var category = await _categoryRepository.GetByIdAsync(query.Id, cancellationToken);
                
                if (category == null)
                {
                    await _logsService.LogWarningAsync($"Category not found with ID: {query.Id}", new Dictionary<string, object> 
                    { 
                        ["CategoryId"] = query.Id,
                        ["QueryId"] = query.QueryId
                    });
                    return ApiResponse<CategoryDto>.CreateError("Category not found");
                }

                // 2. Convert to DTO
                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    Slug = category.Slug,
                    ImageUrl = category.ImageUrl,
                    SortOrder = category.SortOrder,
                    IsActive = category.IsActive,
                    ParentCategoryId = category.ParentCategoryId,
                    ProductCount = category.Products?.Count ?? 0,
                    CreatedAt = category.CreatedAt
                };

                // NiesPro Enterprise: Enhanced audit trail for successful retrieval
                await _logsService.LogInformationAsync($"Successfully retrieved category: {category.Name}", new Dictionary<string, object>
                {
                    ["CategoryId"] = category.Id,
                    ["CategoryName"] = category.Name,
                    ["Slug"] = category.Slug,
                    ["IsActive"] = category.IsActive,
                    ["ProductCount"] = categoryDto.ProductCount,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while retrieving category", new Dictionary<string, object>
                {
                    ["CategoryId"] = query.Id,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<CategoryDto>.CreateError("An error occurred while retrieving the category");
            }
        }
    }
}