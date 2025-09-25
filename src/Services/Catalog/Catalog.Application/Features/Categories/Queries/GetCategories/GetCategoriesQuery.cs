using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategories
{
    /// <summary>
    /// Query to get all categories
    /// </summary>
    public record GetCategoriesQuery : IRequest<ApiResponse<IEnumerable<CategoryDto>>>
    {
        public bool? IncludeInactive { get; init; } = false;
        public bool? RootOnly { get; init; } = false;
    }
}