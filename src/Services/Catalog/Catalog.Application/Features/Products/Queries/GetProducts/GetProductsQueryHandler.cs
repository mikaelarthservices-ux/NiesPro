using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Queries.GetProducts
{
    /// <summary>
    /// Handler for GetProductsQuery
    /// </summary>
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<PagedResultDto<ProductSummaryDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<GetProductsQueryHandler> _logger;

        public GetProductsQueryHandler(
            IProductRepository productRepository,
            ILogger<GetProductsQueryHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResultDto<ProductSummaryDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting products with filters - Page: {Page}, PageSize: {PageSize}", 
                    request.Page, request.PageSize);

                // Get paged products from repository
                var (products, totalCount) = await _productRepository.GetPagedProductsAsync(
                    request.Page,
                    request.PageSize,
                    request.CategoryId?.ToString(),
                    request.BrandId?.ToString(),
                    request.MinPrice,
                    request.MaxPrice,
                    request.SortBy,
                    cancellationToken);

                // Convert to DTOs
                var productDtos = products.Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    ComparePrice = p.ComparePrice,
                    ImageUrl = p.ImageUrl,
                    IsInStock = p.IsInStock,
                    AverageRating = p.AverageRating,
                    ReviewCount = p.ReviewCount,
                    DiscountPercentage = p.DiscountPercentage
                }).ToList();

                // Create paged result
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                var pagedResult = new PagedResultDto<ProductSummaryDto>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = request.Page < totalPages,
                    HasPreviousPage = request.Page > 1
                };

                _logger.LogInformation("Retrieved {Count} products out of {TotalCount}", 
                    productDtos.Count, totalCount);

                return ApiResponse<PagedResultDto<ProductSummaryDto>>.CreateSuccess(
                    pagedResult, 
                    $"Retrieved {productDtos.Count} products successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return ApiResponse<PagedResultDto<ProductSummaryDto>>.CreateError(
                    "An error occurred while retrieving products");
            }
        }
    }
}