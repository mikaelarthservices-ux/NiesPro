using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategories
{
    /// <summary>
    /// Handler for GetCategoriesQuery
    /// </summary>
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, ApiResponse<IEnumerable<CategoryDto>>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoriesQueryHandler> _logger;

        public GetCategoriesQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoriesQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting categories - RootOnly: {RootOnly}, IncludeInactive: {IncludeInactive}", 
                    request.RootOnly, request.IncludeInactive);

                IEnumerable<Catalog.Domain.Entities.Category> categories;

                if (request.RootOnly == true)
                {
                    categories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
                }
                else
                {
                    categories = await _categoryRepository.GetAllAsync(cancellationToken);
                }

                // Filter by active status if needed
                if (request.IncludeInactive != true)
                {
                    categories = categories.Where(c => c.IsActive);
                }

                // Convert to DTOs
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

                _logger.LogInformation("Successfully retrieved {Count} categories", categoryDtos.Count);

                return ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                    categoryDtos, 
                    $"Retrieved {categoryDtos.Count} categories successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return ApiResponse<IEnumerable<CategoryDto>>.CreateError("An error occurred while retrieving categories");
            }
        }
    }
}