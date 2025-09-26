using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Query to get a product by its ID - NiesPro Enterprise Implementation
    /// </summary>
    public record GetProductByIdQuery : IQuery<ApiResponse<ProductDto>>, IRequest<ApiResponse<ProductDto>>
    {
        // NiesPro Enterprise: Query audit properties
        public Guid QueryId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public Guid Id { get; init; }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}