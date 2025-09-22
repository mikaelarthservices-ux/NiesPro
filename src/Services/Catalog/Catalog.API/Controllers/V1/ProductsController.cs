using Catalog.Application.DTOs;
using Catalog.Application.Features.Products.Commands.CreateProduct;
using Catalog.Application.Features.Products.Queries.GetProducts;
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
        /// Get products with filtering and pagination
        /// </summary>
        /// <param name="searchTerm">Search term for product name or description</param>
        /// <param name="categoryId">Filter by category ID</param>
        /// <param name="brandId">Filter by brand ID</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="inStockOnly">Filter only products in stock</param>
        /// <param name="featuredOnly">Filter only featured products</param>
        /// <param name="sortBy">Sort by field (name, price, newest)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of products</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<ProductSummaryDto>>>> GetProducts(
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? brandId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? inStockOnly = null,
            [FromQuery] bool? featuredOnly = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new GetProductsQuery
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                BrandId = brandId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStockOnly,
                FeaturedOnly = featuredOnly,
                SortBy = sortBy,
                Page = page,
                PageSize = pageSize
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
            // TODO: Implement GetProductByIdQuery
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            
            return NotFound(ApiResponse<ProductDto>.CreateError("Product not found"));
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
                return CreatedAtAction(nameof(GetProduct), new { id = result.Data?.Id }, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Update product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="command">Product update data</param>
        /// <returns>Updated product</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductDto command)
        {
            // TODO: Implement UpdateProductCommand
            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            
            return NotFound(ApiResponse<ProductDto>.CreateError("Update functionality not implemented yet"));
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Result of deletion</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(Guid id)
        {
            // TODO: Implement DeleteProductCommand
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            
            return NotFound(ApiResponse<string>.CreateError("Delete functionality not implemented yet"));
        }

        /// <summary>
        /// Update product stock
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="stockUpdate">Stock update data</param>
        /// <returns>Result of stock update</returns>
        [HttpPatch("{id:guid}/stock")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStock(Guid id, [FromBody] UpdateStockDto stockUpdate)
        {
            // TODO: Implement UpdateStockCommand
            _logger.LogInformation("Updating stock for product ID: {ProductId}", id);
            
            return NotFound(ApiResponse<string>.CreateError("Stock update functionality not implemented yet"));
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        /// <param name="count">Number of featured products to return</param>
        /// <returns>List of featured products</returns>
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSummaryDto>>>> GetFeaturedProducts([FromQuery] int count = 10)
        {
            // TODO: Implement GetFeaturedProductsQuery
            _logger.LogInformation("Getting {Count} featured products", count);
            
            return Ok(ApiResponse<IEnumerable<ProductSummaryDto>>.CreateSuccess(
                new List<ProductSummaryDto>(), 
                "Featured products functionality not implemented yet"));
        }
    }
}