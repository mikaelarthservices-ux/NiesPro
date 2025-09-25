using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryById
{
    /// <summary>
    /// Handler for GetCategoryByIdQuery
    /// </summary>
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

        public GetCategoryByIdQueryHandler(
            ICategoryRepository categoryRepository,
            ILogger<GetCategoryByIdQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting category with ID: {CategoryId}", request.Id);

                var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
                
                if (category == null)
                {
                    _logger.LogWarning("Category not found with ID: {CategoryId}", request.Id);
                    return ApiResponse<CategoryDto>.CreateError("Category not found");
                }

                // Convert to DTO
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

                _logger.LogInformation("Successfully retrieved category: {CategoryName} (ID: {CategoryId})", 
                    category.Name, category.Id);

                return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category with ID: {CategoryId}", request.Id);
                return ApiResponse<CategoryDto>.CreateError("An error occurred while retrieving the category");
            }
        }
    }
}