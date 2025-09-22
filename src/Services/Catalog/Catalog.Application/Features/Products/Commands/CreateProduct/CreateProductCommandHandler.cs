using Catalog.Application.DTOs;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Handler for CreateProductCommand
    /// </summary>
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<CreateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new product with SKU: {SKU}", request.SKU);

                // 1. Validate SKU uniqueness
                if (!await _productRepository.IsSkuUniqueAsync(request.SKU, cancellationToken: cancellationToken))
                {
                    _logger.LogWarning("Attempt to create product with duplicate SKU: {SKU}", request.SKU);
                    return ApiResponse<ProductDto>.CreateError("A product with this SKU already exists");
                }

                // 2. Validate category exists
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
                if (category == null || !category.IsActive)
                {
                    _logger.LogWarning("Attempt to create product with invalid category: {CategoryId}", request.CategoryId);
                    return ApiResponse<ProductDto>.CreateError("Category not found or inactive");
                }

                // 3. Validate brand if provided
                Brand? brand = null;
                if (request.BrandId.HasValue)
                {
                    brand = await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken);
                    if (brand == null || !brand.IsActive)
                    {
                        _logger.LogWarning("Attempt to create product with invalid brand: {BrandId}", request.BrandId);
                        return ApiResponse<ProductDto>.CreateError("Brand not found or inactive");
                    }
                }

                // 4. Create product entity
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    SKU = request.SKU,
                    Description = request.Description,
                    LongDescription = request.LongDescription,
                    Price = request.Price,
                    ComparePrice = request.ComparePrice,
                    CostPrice = request.CostPrice,
                    TrackQuantity = request.TrackQuantity,
                    Quantity = request.Quantity,
                    LowStockThreshold = request.LowStockThreshold,
                    Weight = request.Weight,
                    ImageUrl = request.ImageUrl,
                    ImageGallery = request.ImageGallery,
                    IsActive = request.IsActive,
                    IsFeatured = request.IsFeatured,
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    PublishedAt = request.IsActive ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow
                };

                // 5. Save product
                var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

                _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);

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
                _logger.LogError(ex, "Error creating product with SKU: {SKU}", request.SKU);
                return ApiResponse<ProductDto>.CreateError("An error occurred while creating the product");
            }
        }
    }
}