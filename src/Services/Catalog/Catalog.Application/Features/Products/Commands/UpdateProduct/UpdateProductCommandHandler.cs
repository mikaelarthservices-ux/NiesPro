using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Handler for UpdateProductCommand - NiesPro Enterprise Implementation
    /// </summary>
    public class UpdateProductCommandHandler : 
        BaseCommandHandler<UpdateProductCommand, ApiResponse<ProductDto>>,
        IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<UpdateProductCommandHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<UpdateProductCommandHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseCommandHandler
        /// </summary>
        public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute update product command - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<ProductDto>> ExecuteAsync(UpdateProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Updating product with ID: {command.Id}", new Dictionary<string, object>
                {
                    ["ProductId"] = command.Id,
                    ["ProductName"] = command.Name,
                    ["SKU"] = command.SKU,
                    ["CategoryId"] = command.CategoryId,
                    ["BrandId"] = command.BrandId ?? (object)"null",
                    ["Price"] = command.Price,
                    ["CommandId"] = command.CommandId
                });

                // 1. Get existing product
                var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
                if (product == null)
                {
                    await _logsService.LogWarningAsync($"Product not found with ID: {command.Id}", new Dictionary<string, object> 
                    { 
                        ["ProductId"] = command.Id 
                    });
                    return ApiResponse<ProductDto>.CreateError("Product not found");
                }

                // 2. Validate category exists
                var categoryEntity = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
                if (categoryEntity == null || !categoryEntity.IsActive)
                {
                    await _logsService.LogWarningAsync($"Category not found or inactive with ID: {command.CategoryId}", new Dictionary<string, object> 
                    { 
                        ["CategoryId"] = command.CategoryId 
                    });
                    return ApiResponse<ProductDto>.CreateError("Category not found or inactive");
                }

                // 3. Validate brand exists if provided
                Brand? brand = null;
                if (command.BrandId.HasValue)
                {
                    brand = await _brandRepository.GetByIdAsync(command.BrandId.Value, cancellationToken);
                    if (brand == null || !brand.IsActive)
                    {
                        await _logsService.LogWarningAsync($"Brand not found or inactive with ID: {command.BrandId}", new Dictionary<string, object> 
                        { 
                            ["BrandId"] = command.BrandId ?? (object)"null" 
                        });
                        return ApiResponse<ProductDto>.CreateError("Brand not found or inactive");
                    }
                }

                // 4. Update product properties
                product.Name = command.Name;
                product.Description = command.Description;
                product.SKU = command.SKU;
                product.Price = command.Price;
                product.ComparePrice = command.ComparePrice;
                product.TrackQuantity = command.TrackQuantity;
                product.Quantity = command.Quantity;
                product.Weight = command.Weight;
                product.ImageUrl = command.ImageUrl;
                product.IsActive = command.IsActive;
                product.IsFeatured = command.IsFeatured;
                product.CategoryId = command.CategoryId;
                product.BrandId = command.BrandId;
                product.UpdatedAt = DateTime.UtcNow;

                // 5. Update in repository
                await _productRepository.UpdateAsync(product, cancellationToken);

                // NiesPro Enterprise: Enhanced audit trail for successful update
                await _logsService.LogInformationAsync($"Product updated successfully with ID: {product.Id}", new Dictionary<string, object>
                {
                    ["ProductId"] = product.Id,
                    ["SKU"] = product.SKU,
                    ["Name"] = product.Name,
                    ["CategoryId"] = product.CategoryId,
                    ["Price"] = product.Price,
                    ["CommandId"] = command.CommandId
                });

                // 6. Create response DTO
                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    SKU = product.SKU,
                    Price = product.Price,
                    ComparePrice = product.ComparePrice,
                    TrackQuantity = product.TrackQuantity,
                    Quantity = product.Quantity,
                    Weight = product.Weight,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    IsInStock = product.IsInStock,
                    IsLowStock = product.IsLowStock,
                    AverageRating = product.AverageRating,
                    ReviewCount = product.ReviewCount,
                    DiscountPercentage = product.DiscountPercentage,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    CategoryId = product.CategoryId,
                    CategoryName = categoryEntity.Name,
                    BrandId = product.BrandId,
                    BrandName = brand?.Name
                };

                return ApiResponse<ProductDto>.CreateSuccess(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while updating product", new Dictionary<string, object>
                {
                    ["ProductId"] = command.Id,
                    ["ProductName"] = command.Name,
                    ["CommandId"] = command.CommandId
                });

                return ApiResponse<ProductDto>.CreateError("An error occurred while updating the product");
            }
        }
    }
}