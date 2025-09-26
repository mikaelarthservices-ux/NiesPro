using Catalog.Application.DTOs;
using MediatR;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Categories.Commands.CreateCategory
{
    /// <summary>
    /// Command to create a new category - NiesPro Enterprise Implementation
    /// </summary>
    public record CreateCategoryCommand : ICommand<ApiResponse<CategoryDto>>, IRequest<ApiResponse<CategoryDto>>
    {
        // NiesPro Enterprise: Command audit properties
        public Guid CommandId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public int SortOrder { get; init; } = 0;
        public bool IsActive { get; init; } = true;
        public Guid? ParentCategoryId { get; init; }
    }
}