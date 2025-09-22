using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.CreateProduct
{
    /// <summary>
    /// Create product command
    /// </summary>
    public record CreateProductCommand : IRequest<ApiResponse<ProductDto>>
    {
        public string Name { get; init; } = string.Empty;
        public string SKU { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LongDescription { get; init; }
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public decimal? CostPrice { get; init; }
        public bool TrackQuantity { get; init; } = true;
        public int Quantity { get; init; } = 0;
        public int? LowStockThreshold { get; init; }
        public decimal? Weight { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageGallery { get; init; }
        public bool IsActive { get; init; } = true;
        public bool IsFeatured { get; init; } = false;
        public Guid CategoryId { get; init; }
        public Guid? BrandId { get; init; }
    }
}