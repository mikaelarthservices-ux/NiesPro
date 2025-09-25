using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Commands.CreateCategory
{
    /// <summary>
    /// Command to create a new category
    /// </summary>
    public record CreateCategoryCommand : IRequest<ApiResponse<CategoryDto>>
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public int SortOrder { get; init; } = 0;
        public bool IsActive { get; init; } = true;
        public Guid? ParentCategoryId { get; init; }
    }
}