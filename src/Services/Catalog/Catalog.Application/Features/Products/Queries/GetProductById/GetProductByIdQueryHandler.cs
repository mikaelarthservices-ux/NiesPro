using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Handler for GetProductByIdQuery
    /// </summary>
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<GetProductByIdQueryHandler> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);

                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
                
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                    return ApiResponse<ProductDto>.CreateError("Product not found");
                }

                // Get category and brand details
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
                    Weight = product.Weight,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    IsFeatured = product.IsFeatured,
                    TrackQuantity = product.TrackQuantity,
                    Quantity = product.Quantity,
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

                _logger.LogInformation("Successfully retrieved product: {ProductName} (ID: {ProductId})", 
                    product.Name, product.Id);

                return ApiResponse<ProductDto>.CreateSuccess(productDto, "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", request.Id);
                return ApiResponse<ProductDto>.CreateError("An error occurred while retrieving the product");
            }
        }
    }
}