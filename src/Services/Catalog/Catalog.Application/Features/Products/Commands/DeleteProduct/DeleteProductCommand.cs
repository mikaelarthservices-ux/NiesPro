using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.DeleteProduct
{
    /// <summary>
    /// Command to delete a product - NiesPro Enterprise Implementation
    /// </summary>
    public record DeleteProductCommand : ICommand<ApiResponse<bool>>, IRequest<ApiResponse<bool>>
    {
        // NiesPro Enterprise: Command audit properties
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public Guid Id { get; init; }

        public DeleteProductCommand(Guid id)
        {
            Id = id;
        }
    }
}