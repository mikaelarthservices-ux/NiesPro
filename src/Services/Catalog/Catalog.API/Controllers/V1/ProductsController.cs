using Catalog.Application.DTOs;
using Catalog.Application.Features.Products.Commands.CreateProduct;
using Catalog.Application.Features.Products.Commands.UpdateProduct;
using Catalog.Application.Features.Products.Commands.DeleteProduct;
using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NiesPro.Contracts.Common;

namespace Catalog.API.Controllers.V1
{
    /// <summary>
    /// Products management controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with optional filtering and pagination
        /// </summary>
        /// <param name="categoryId">Filter by category</param>
        /// <param name="brandId">Filter by brand</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="sortBy">Sort field</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="onlyActive">Only active products</param>
        /// <param name="onlyFeatured">Only featured products</param>
        /// <returns>Paginated list of products</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ProductSummaryDto>>>> GetProducts(
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? brandId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyFeatured = false)
        {
            var query = new GetProductsQuery
            {
                CategoryId = categoryId,
                BrandId = brandId,
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                Page = pageNumber,
                PageSize = pageSize,
                InStockOnly = onlyActive ? true : null,
                FeaturedOnly = onlyFeatured ? true : null
            };

            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(Guid id)
        {
            // Validate GUID format
            if (id == Guid.Empty)
            {
                return BadRequest(ApiResponse<ProductDto>.CreateError("Invalid product ID"));
            }

            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors?.Any() == true && result.Errors.First().Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="command">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="command">Product update data</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(ApiResponse<ProductDto>.CreateError("Product ID in URL does not match request body"));
            }

            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors?.Any() == true && result.Errors.First().Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Delete a product (soft delete)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(Guid id)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return NoContent(); // HTTP 204 for successful deletion
            }

            if (result.Errors?.Any() == true && result.Errors.First().Contains("not found"))
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}