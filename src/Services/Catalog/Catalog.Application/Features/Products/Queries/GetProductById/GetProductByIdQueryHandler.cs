using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Handler for GetProductByIdQuery - NiesPro Enterprise Implementation
    /// </summary>
    public class GetProductByIdQueryHandler : 
        BaseQueryHandler<GetProductByIdQuery, ApiResponse<ProductDto>>,
        IRequestHandler<GetProductByIdQuery, ApiResponse<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public GetProductByIdQueryHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            ILogger<GetProductByIdQueryHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseQueryHandler
        /// </summary>
        public async Task<ApiResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute get product by ID query - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<ProductDto>> ExecuteAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Retrieving product with ID: {query.Id}", new Dictionary<string, object>
                {
                    ["ProductId"] = query.Id,
                    ["QueryId"] = query.QueryId
                });

                // 1. Get product from repository
                var product = await _productRepository.GetByIdAsync(query.Id, cancellationToken);
                
                if (product == null)
                {
                    await _logsService.LogWarningAsync($"Product not found with ID: {query.Id}", new Dictionary<string, object> 
                    { 
                        ["ProductId"] = query.Id,
                        ["QueryId"] = query.QueryId
                    });
                    return ApiResponse<ProductDto>.CreateError("Product not found");
                }

                // 2. Get category and brand details
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);

                var brand = product.BrandId.HasValue 
                    ? await _brandRepository.GetByIdAsync(product.BrandId.Value, cancellationToken)
                    : null;

                // 3. Convert to DTO
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

                // NiesPro Enterprise: Enhanced audit trail for successful retrieval
                await _logsService.LogInformationAsync($"Successfully retrieved product: {product.Name}", new Dictionary<string, object>
                {
                    ["ProductId"] = product.Id,
                    ["ProductName"] = product.Name,
                    ["SKU"] = product.SKU,
                    ["CategoryName"] = category?.Name ?? "Unknown",
                    ["BrandName"] = brand?.Name ?? "No Brand",
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<ProductDto>.CreateSuccess(productDto, "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while retrieving product", new Dictionary<string, object>
                {
                    ["ProductId"] = query.Id,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<ProductDto>.CreateError("An error occurred while retrieving the product");
            }
        }
    }
}