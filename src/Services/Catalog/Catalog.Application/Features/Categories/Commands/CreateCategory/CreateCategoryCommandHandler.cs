using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Commands.CreateCategory
{
    /// <summary>
    /// Handler for CreateCategoryCommand
    /// </summary>
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new category: {CategoryName}", request.Name);

                // Validate parent category exists if provided
                if (request.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                    if (parentCategory == null)
                    {
                        _logger.LogWarning("Parent category not found with ID: {ParentCategoryId}", request.ParentCategoryId);
                        return ApiResponse<CategoryDto>.CreateError("Parent category not found");
                    }
                }

                // Check if category with same name already exists
                var existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
                if (existingCategories.Any(c => c.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Category with name '{CategoryName}' already exists", request.Name);
                    return ApiResponse<CategoryDto>.CreateError("A category with this name already exists");
                }

                // Create category entity
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Slug = GenerateSlug(request.Name),
                    ImageUrl = request.ImageUrl,
                    SortOrder = request.SortOrder,
                    IsActive = request.IsActive,
                    ParentCategoryId = request.ParentCategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save to repository
                await _categoryRepository.AddAsync(category, cancellationToken);

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
                    ProductCount = 0,
                    CreatedAt = category.CreatedAt
                };

                _logger.LogInformation("Successfully created category: {CategoryName} with ID: {CategoryId}", 
                    category.Name, category.Id);

                return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", request.Name);
                return ApiResponse<CategoryDto>.CreateError("An error occurred while creating the category");
            }
        }

        private static string GenerateSlug(string name)
        {
            // Simple slug generation - convert to lowercase and replace spaces with hyphens
            return name.ToLowerInvariant()
                      .Replace(" ", "-")
                      .Replace("'", "")
                      .Replace("\"", "")
                      .Trim('-');
        }
    }
}