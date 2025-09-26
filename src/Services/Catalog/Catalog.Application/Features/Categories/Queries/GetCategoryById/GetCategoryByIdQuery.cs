using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryById
{
    /// <summary>
    /// Query to get a category by its ID - NiesPro Enterprise Implementation
    /// </summary>
    public record GetCategoryByIdQuery : IQuery<ApiResponse<CategoryDto>>, IRequest<ApiResponse<CategoryDto>>
    {
        // NiesPro Enterprise: Query audit properties
        public Guid QueryId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public Guid Id { get; init; }

        public GetCategoryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}