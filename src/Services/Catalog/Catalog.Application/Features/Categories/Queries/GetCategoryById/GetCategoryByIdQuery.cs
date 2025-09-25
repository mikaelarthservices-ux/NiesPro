using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryById
{
    /// <summary>
    /// Query to get a category by its ID
    /// </summary>
    public record GetCategoryByIdQuery : IRequest<ApiResponse<CategoryDto>>
    {
        public Guid Id { get; init; }

        public GetCategoryByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}