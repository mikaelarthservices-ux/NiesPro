using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategories
{
    /// <summary>
    /// Query to get all categories - NiesPro Enterprise Implementation
    /// </summary>
    public record GetCategoriesQuery : IQuery<ApiResponse<IEnumerable<CategoryDto>>>, IRequest<ApiResponse<IEnumerable<CategoryDto>>>
    {
        // NiesPro Enterprise: Query audit properties
        public Guid QueryId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public bool? IncludeInactive { get; init; } = false;
        public bool? RootOnly { get; init; } = false;
    }
}