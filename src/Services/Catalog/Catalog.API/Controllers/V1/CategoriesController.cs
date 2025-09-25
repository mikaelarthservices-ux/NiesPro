using Catalog.Application.DTOs;
using Catalog.Application.Features.Categories.Commands.CreateCategory;
using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NiesPro.Contracts.Common;

namespace Catalog.API.Controllers.V1
{
    /// <summary>
    /// Categories management controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
        {
            var query = new GetCategoriesQuery();
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(Guid id)
        {
            var query = new GetCategoryByIdQuery(id);
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Message == "Category not found")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get root categories (no parent)
        /// </summary>
        /// <returns>List of root categories</returns>
        [HttpGet("root")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetRootCategories()
        {
            var query = new GetCategoriesQuery { RootOnly = true };
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="request">Category creation data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryCommand request)
        {
            var result = await _mediator.Send(request);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetCategory), new { id = result.Data?.Id }, result);
            }

            return BadRequest(result);
        }
    }
}