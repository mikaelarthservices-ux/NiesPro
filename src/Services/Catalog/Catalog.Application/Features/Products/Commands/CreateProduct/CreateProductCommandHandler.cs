using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Create product command handler - NiesPro Enterprise Standard with BaseCommandHandler
    /// </summary>
    public class CreateProductCommandHandler : BaseCommandHandler<CreateProductCommand, ApiResponse<ProductDto>>, IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogsServiceClient _logsService;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<CreateProductCommandHandler> logger,
            ILogsServiceClient logsService)
            : base(logger) // NiesPro Enterprise: BaseCommandHandler inheritance
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseCommandHandler
        /// </summary>
        public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            return await HandleAsync(request, cancellationToken);
        }

        /// <summary>
        /// Execute create product command - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<ProductDto>> ExecuteAsync(CreateProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Creating new product with SKU: {command.SKU}", new Dictionary<string, object>
                {
                    ["SKU"] = command.SKU,
                    ["ProductName"] = command.Name,
                    ["CategoryId"] = command.CategoryId,
                    ["BrandId"] = command.BrandId ?? (object)"null",
                    ["Price"] = command.Price,
                    ["IsActive"] = command.IsActive,
                    ["CommandId"] = command.CommandId
                });

                    // 1. Validate SKU uniqueness
                    if (!await _productRepository.IsSkuUniqueAsync(command.SKU, cancellationToken: cancellationToken))
                    {
                        await _logsService.LogWarningAsync($"Attempt to create product with duplicate SKU: {command.SKU}", new Dictionary<string, object> { ["SKU"] = command.SKU });
                        return ApiResponse<ProductDto>.CreateError("A product with this SKU already exists");
                    }

                    // 2. Validate category exists
                    var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
                    if (category == null || !category.IsActive)
                    {
                        await _logsService.LogWarningAsync($"Attempt to create product with invalid category: {command.CategoryId}", new Dictionary<string, object> { ["CategoryId"] = command.CategoryId });
                        return ApiResponse<ProductDto>.CreateError("Category not found or inactive");
                    }

                    // 3. Validate brand if provided
                    Brand? brand = null;
                    if (command.BrandId.HasValue)
                    {
                        brand = await _brandRepository.GetByIdAsync(command.BrandId.Value, cancellationToken);
                        if (brand == null || !brand.IsActive)
                        {
                            await _logsService.LogWarningAsync($"Attempt to create product with invalid brand: {command.BrandId}", new Dictionary<string, object> { ["BrandId"] = command.BrandId ?? (object)"null" });
                            return ApiResponse<ProductDto>.CreateError("Brand not found or inactive");
                        }
                    }

                    // 4. Create product entity
                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = command.Name,
                        SKU = command.SKU,
                        Description = command.Description,
                        LongDescription = command.LongDescription,
                        Price = command.Price,
                        ComparePrice = command.ComparePrice,
                        CostPrice = command.CostPrice,
                        TrackQuantity = command.TrackQuantity,
                        Quantity = command.Quantity,
                        LowStockThreshold = command.LowStockThreshold,
                        Weight = command.Weight,
                        ImageUrl = command.ImageUrl,
                        ImageGallery = command.ImageGallery,
                        IsActive = command.IsActive,
                        IsFeatured = command.IsFeatured,
                        CategoryId = command.CategoryId,
                        BrandId = command.BrandId,
                        PublishedAt = command.IsActive ? DateTime.UtcNow : null,
                        CreatedAt = DateTime.UtcNow
                    };

                    // 5. Save product
                    var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

                    // NiesPro Enterprise: Enhanced audit trail for successful creation
                    await _logsService.LogInformationAsync($"Product created successfully with ID: {createdProduct.Id}", new Dictionary<string, object>
                    {
                        ["ProductId"] = createdProduct.Id,
                        ["SKU"] = createdProduct.SKU,
                        ["Name"] = createdProduct.Name,
                        ["CategoryId"] = createdProduct.CategoryId,
                        ["Price"] = createdProduct.Price,
                        ["CommandId"] = command.CommandId
                    });

                    // 6. Create response DTO
                    var productDto = new ProductDto
                    {
                        Id = createdProduct.Id,
                        Name = createdProduct.Name,
                        SKU = createdProduct.SKU,
                        Description = createdProduct.Description,
                        Price = createdProduct.Price,
                        ComparePrice = createdProduct.ComparePrice,
                        TrackQuantity = createdProduct.TrackQuantity,
                        Quantity = createdProduct.Quantity,
                        Weight = createdProduct.Weight,
                        ImageUrl = createdProduct.ImageUrl,
                        IsActive = createdProduct.IsActive,
                        IsFeatured = createdProduct.IsFeatured,
                        PublishedAt = createdProduct.PublishedAt,
                        CategoryId = createdProduct.CategoryId,
                        CategoryName = category.Name,
                        BrandId = createdProduct.BrandId,
                        BrandName = brand?.Name,
                        IsInStock = createdProduct.IsInStock,
                        IsLowStock = createdProduct.IsLowStock,
                        DiscountPercentage = createdProduct.DiscountPercentage,
                        AverageRating = 0,
                        ReviewCount = 0,
                        CreatedAt = createdProduct.CreatedAt,
                        UpdatedAt = createdProduct.UpdatedAt
                    };

                return ApiResponse<ProductDto>.CreateSuccess(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while creating product", new Dictionary<string, object>
                {
                    ["SKU"] = command.SKU,
                    ["ProductName"] = command.Name,
                    ["CategoryId"] = command.CategoryId,
                    ["CommandId"] = command.CommandId
                });

                return ApiResponse<ProductDto>.CreateError("An error occurred while creating the product");
            }
        }
    }
}
