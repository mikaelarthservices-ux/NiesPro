using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Queries.GetProductById
{
    /// <summary>
    /// Query to get a product by its ID
    /// </summary>
    public record GetProductByIdQuery : IRequest<ApiResponse<ProductDto>>
    {
        public Guid Id { get; init; }

        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}