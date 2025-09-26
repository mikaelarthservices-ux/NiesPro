using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Categories.Commands.CreateCategory
{
    /// <summary>
    /// Handler for CreateCategoryCommand - NiesPro Enterprise Implementation
    /// </summary>
    public class CreateCategoryCommandHandler : 
        BaseCommandHandler<CreateCategoryCommand, ApiResponse<CategoryDto>>,
        IRequestHandler<CreateCategoryCommand, ApiResponse<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            ILogger<CreateCategoryCommandHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseCommandHandler
        /// </summary>
        public async Task<ApiResponse<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute create category command - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<CategoryDto>> ExecuteAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Creating new category: {command.Name}", new Dictionary<string, object>
                {
                    ["CategoryName"] = command.Name,
                    ["ParentCategoryId"] = command.ParentCategoryId ?? (object)"null",
                    ["IsActive"] = command.IsActive,
                    ["SortOrder"] = command.SortOrder,
                    ["CommandId"] = command.CommandId
                });

                // 1. Validate parent category exists if provided
                if (command.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
                    if (parentCategory == null || !parentCategory.IsActive)
                    {
                        await _logsService.LogWarningAsync($"Parent category not found or inactive with ID: {command.ParentCategoryId}", new Dictionary<string, object> 
                        { 
                            ["ParentCategoryId"] = command.ParentCategoryId ?? (object)"null",
                            ["CommandId"] = command.CommandId 
                        });
                        return ApiResponse<CategoryDto>.CreateError("Parent category not found or inactive");
                    }
                }

                // 2. Check if category with same name already exists
                var existingCategories = await _categoryRepository.GetAllAsync(cancellationToken);
                if (existingCategories.Any(c => c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    await _logsService.LogWarningAsync($"Category with name '{command.Name}' already exists", new Dictionary<string, object> 
                    { 
                        ["CategoryName"] = command.Name,
                        ["CommandId"] = command.CommandId 
                    });
                    return ApiResponse<CategoryDto>.CreateError("A category with this name already exists");
                }

                // 3. Create category entity
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = command.Name,
                    Description = command.Description,
                    Slug = GenerateSlug(command.Name),
                    ImageUrl = command.ImageUrl,
                    SortOrder = command.SortOrder,
                    IsActive = command.IsActive,
                    ParentCategoryId = command.ParentCategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 4. Save to repository
                await _categoryRepository.AddAsync(category, cancellationToken);

                // NiesPro Enterprise: Enhanced audit trail for successful creation
                await _logsService.LogInformationAsync($"Category created successfully: {category.Name}", new Dictionary<string, object>
                {
                    ["CategoryId"] = category.Id,
                    ["CategoryName"] = category.Name,
                    ["Slug"] = category.Slug,
                    ["ParentCategoryId"] = category.ParentCategoryId ?? (object)"null",
                    ["CommandId"] = command.CommandId
                });

                // 5. Convert to DTO
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

                return ApiResponse<CategoryDto>.CreateSuccess(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while creating category", new Dictionary<string, object>
                {
                    ["CategoryName"] = command.Name,
                    ["CommandId"] = command.CommandId
                });

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