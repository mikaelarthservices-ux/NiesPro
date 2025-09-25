using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Common;

namespace Catalog.Application.Features.Products.Commands.DeleteProduct
{
    /// <summary>
    /// Handler for DeleteProductCommand
    /// </summary>
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponse<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(
            IProductRepository productRepository,
            ILogger<DeleteProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}", request.Id);

                // Get existing product
                var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                    return ApiResponse<bool>.CreateError("Product not found");
                }

                // Soft delete - just deactivate the product instead of permanently deleting
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product, cancellationToken);

                _logger.LogInformation("Successfully deleted (deactivated) product: {ProductName} (ID: {ProductId})", 
                    product.Name, product.Id);

                return ApiResponse<bool>.CreateSuccess(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", request.Id);
                return ApiResponse<bool>.CreateError("An error occurred while deleting the product");
            }
        }
    }
}