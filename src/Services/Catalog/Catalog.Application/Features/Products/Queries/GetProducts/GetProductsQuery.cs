using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Queries.GetProducts
{
    /// <summary>
    /// Get products with filtering and pagination - NiesPro Enterprise Implementation
    /// </summary>
    public record GetProductsQuery : IQuery<ApiResponse<PagedResultDto<ProductSummaryDto>>>, IRequest<ApiResponse<PagedResultDto<ProductSummaryDto>>>
    {
        // NiesPro Enterprise: Query audit properties
        public Guid QueryId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
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