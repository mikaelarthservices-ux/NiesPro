using Catalog.Application.DTOs;
using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Products.Queries.GetProducts
{
    /// <summary>
    /// Handler for GetProductsQuery - NiesPro Enterprise Implementation
    /// </summary>
    public class GetProductsQueryHandler : 
        BaseQueryHandler<GetProductsQuery, ApiResponse<PagedResultDto<ProductSummaryDto>>>,
        IRequestHandler<GetProductsQuery, ApiResponse<PagedResultDto<ProductSummaryDto>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<GetProductsQueryHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public GetProductsQueryHandler(
            IProductRepository productRepository,
            ILogger<GetProductsQueryHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _productRepository = productRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseQueryHandler
        /// </summary>
        public async Task<ApiResponse<PagedResultDto<ProductSummaryDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute get products query - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<PagedResultDto<ProductSummaryDto>>> ExecuteAsync(GetProductsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Retrieving products with filters - Page: {query.Page}, PageSize: {query.PageSize}", new Dictionary<string, object>
                {
                    ["Page"] = query.Page,
                    ["PageSize"] = query.PageSize,
                    ["SearchTerm"] = query.SearchTerm ?? (object)"null",
                    ["CategoryId"] = query.CategoryId ?? (object)"null",
                    ["BrandId"] = query.BrandId ?? (object)"null",
                    ["MinPrice"] = query.MinPrice ?? (object)"null",
                    ["MaxPrice"] = query.MaxPrice ?? (object)"null",
                    ["InStockOnly"] = query.InStockOnly ?? (object)"null",
                    ["FeaturedOnly"] = query.FeaturedOnly ?? (object)"null",
                    ["SortBy"] = query.SortBy ?? "name",
                    ["QueryId"] = query.QueryId
                });

                // 1. Get paged products from repository
                var (products, totalCount) = await _productRepository.GetPagedProductsAsync(
                    query.Page,
                    query.PageSize,
                    query.CategoryId?.ToString(),
                    query.BrandId?.ToString(),
                    query.MinPrice,
                    query.MaxPrice,
                    query.SortBy,
                    cancellationToken);

                // 2. Convert to DTOs
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

                // 3. Create paged result
                var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);
                var pagedResult = new PagedResultDto<ProductSummaryDto>
                {
                    Items = productDtos,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = query.Page < totalPages,
                    HasPreviousPage = query.Page > 1
                };

                // NiesPro Enterprise: Enhanced audit trail for successful retrieval
                await _logsService.LogInformationAsync($"Retrieved {productDtos.Count} products out of {totalCount}", new Dictionary<string, object>
                {
                    ["RetrievedCount"] = productDtos.Count,
                    ["TotalCount"] = totalCount,
                    ["Page"] = query.Page,
                    ["TotalPages"] = totalPages,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<PagedResultDto<ProductSummaryDto>>.CreateSuccess(
                    pagedResult, 
                    $"Retrieved {productDtos.Count} products successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while retrieving products", new Dictionary<string, object>
                {
                    ["Page"] = query.Page,
                    ["PageSize"] = query.PageSize,
                    ["QueryId"] = query.QueryId
                });

                return ApiResponse<PagedResultDto<ProductSummaryDto>>.CreateError(
                    "An error occurred while retrieving products");
            }
        }
    }
}