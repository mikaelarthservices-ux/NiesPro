using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.DeleteProduct
{
    /// <summary>
    /// Command to delete a product
    /// </summary>
    public record DeleteProductCommand : IRequest<ApiResponse<bool>>
    {
        public Guid Id { get; init; }

        public DeleteProductCommand(Guid id)
        {
            Id = id;
        }
    }
}