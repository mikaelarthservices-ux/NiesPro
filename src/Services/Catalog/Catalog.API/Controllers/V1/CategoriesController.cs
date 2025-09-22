using Catalog.Application.DTOs;
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
            // TODO: Implement GetCategoriesQuery
            _logger.LogInformation("Getting all categories");
            
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                new List<CategoryDto>(), 
                "Categories functionality not implemented yet"));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(Guid id)
        {
            // TODO: Implement GetCategoryByIdQuery
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            
            return NotFound(ApiResponse<CategoryDto>.CreateError("Category not found"));
        }

        /// <summary>
        /// Get root categories (no parent)
        /// </summary>
        /// <returns>List of root categories</returns>
        [HttpGet("root")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetRootCategories()
        {
            // TODO: Implement GetRootCategoriesQuery
            _logger.LogInformation("Getting root categories");
            
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.CreateSuccess(
                new List<CategoryDto>(), 
                "Root categories functionality not implemented yet"));
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="command">Category creation data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto command)
        {
            // TODO: Implement CreateCategoryCommand
            _logger.LogInformation("Creating new category: {CategoryName}", command.Name);
            
            return BadRequest(ApiResponse<CategoryDto>.CreateError("Create category functionality not implemented yet"));
        }
    }
}