using Catalog.Application.DTOs;
using MediatR;
using BuildingBlocks.Common.DTOs;

namespace Catalog.Application.Features.Products.Queries.GetProducts
{
    /// <summary>
    /// Get products with filtering and pagination
    /// </summary>
    public record GetProductsQuery : IRequest<ApiResponse<PagedResultDto<ProductSummaryDto>>>
    {
        public string? SearchTerm { get; init; }
        public Guid? CategoryId { get; init; }
        public Guid? BrandId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public bool? InStockOnly { get; init; }
        public bool? FeaturedOnly { get; init; }
        public string? SortBy { get; init; } = "name";
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}