using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Handler for UpdateProductCommand
    /// </summary>
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating product with ID: {ProductId}", request.Id);

                // Get existing product
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                    return ApiResponse<ProductDto>.CreateError("Product not found");
                }

                // Validate category exists
                var categoryEntity = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
                if (categoryEntity == null)
                {
                    _logger.LogWarning("Category not found with ID: {CategoryId}", request.CategoryId);
                    return ApiResponse<ProductDto>.CreateError("Category not found");
                }

                // Validate brand exists if provided
                if (request.BrandId.HasValue)
                {
                    var brandEntity = await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken);
                    if (brandEntity == null)
                    {
                        _logger.LogWarning("Brand not found with ID: {BrandId}", request.BrandId);
                        return ApiResponse<ProductDto>.CreateError("Brand not found");
                    }
                }

                // Update product properties
                product.Name = request.Name;
                product.Description = request.Description;
                product.SKU = request.SKU;
                product.Price = request.Price;
                product.ComparePrice = request.ComparePrice;
                product.TrackQuantity = request.TrackQuantity;
                product.Quantity = request.Quantity;
                product.Weight = request.Weight;
                product.ImageUrl = request.ImageUrl;
                product.IsActive = request.IsActive;
                product.IsFeatured = request.IsFeatured;
                product.CategoryId = request.CategoryId;
                product.BrandId = request.BrandId;
                product.UpdatedAt = DateTime.UtcNow;

                // Update in repository (assuming the repository has an Update method)
                await _productRepository.UpdateAsync(product, cancellationToken);

                // Get updated details for response
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);
                var brand = product.BrandId.HasValue 
                    ? await _brandRepository.GetByIdAsync(product.BrandId.Value, cancellationToken)
                    : null;

                // Convert to DTO
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
                    CategoryName = category?.Name,
                    BrandId = product.BrandId,
                    BrandName = brand?.Name
                };

                _logger.LogInformation("Successfully updated product: {ProductName} (ID: {ProductId})", 
                    product.Name, product.Id);

                return ApiResponse<ProductDto>.CreateSuccess(productDto, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", request.Id);
                return ApiResponse<ProductDto>.CreateError("An error occurred while updating the product");
            }
        }
    }
}