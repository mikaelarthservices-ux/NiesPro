using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.UpdateProduct
{
    /// <summary>
    /// Command to update an existing product
    /// </summary>
    public record UpdateProductCommand : IRequest<ApiResponse<ProductDto>>
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string SKU { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal? ComparePrice { get; init; }
        public bool TrackQuantity { get; init; } = true;
        public int Quantity { get; init; }
        public decimal? Weight { get; init; }
        public string? ImageUrl { get; init; }
        public bool IsActive { get; init; } = true;
        public bool IsFeatured { get; init; } = false;
        public Guid CategoryId { get; init; }
        public Guid? BrandId { get; init; }
    }
}