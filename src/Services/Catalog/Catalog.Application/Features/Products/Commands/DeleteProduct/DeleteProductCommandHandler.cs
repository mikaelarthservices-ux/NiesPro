using Catalog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Catalog.Application.Features.Products.Commands.DeleteProduct
{
    /// <summary>
    /// Handler for DeleteProductCommand - NiesPro Enterprise Implementation
    /// </summary>
    public class DeleteProductCommandHandler : 
        BaseCommandHandler<DeleteProductCommand, ApiResponse<bool>>,
        IRequestHandler<DeleteProductCommand, ApiResponse<bool>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<DeleteProductCommandHandler> _logger;
        private readonly ILogsServiceClient _logsService;

        public DeleteProductCommandHandler(
            IProductRepository productRepository,
            ILogger<DeleteProductCommandHandler> logger,
            ILogsServiceClient logsService) : base(logger)
        {
            _productRepository = productRepository;
            _logger = logger;
            _logsService = logsService;
        }

        /// <summary>
        /// MediatR Handle method - delegates to BaseCommandHandler
        /// </summary>
        public async Task<ApiResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
            => await HandleAsync(request, cancellationToken);

        /// <summary>
        /// Execute delete product command - NiesPro Enterprise Implementation
        /// </summary>
        protected override async Task<ApiResponse<bool>> ExecuteAsync(DeleteProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // NiesPro Enterprise: Enhanced logging with audit trail
                await _logsService.LogInformationAsync($"Deleting product with ID: {command.Id}", new Dictionary<string, object>
                {
                    ["ProductId"] = command.Id,
                    ["CommandId"] = command.CommandId,
                    ["Operation"] = "SoftDelete"
                });

                // 1. Get existing product
                var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
                if (product == null)
                {
                    await _logsService.LogWarningAsync($"Product not found with ID: {command.Id}", new Dictionary<string, object> 
                    { 
                        ["ProductId"] = command.Id,
                        ["CommandId"] = command.CommandId
                    });
                    return ApiResponse<bool>.CreateError("Product not found");
                }

                // 2. Soft delete - deactivate the product instead of permanently deleting
                var productName = product.Name; // Store name for logging before deactivation
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                // 3. Update product in repository
                await _productRepository.UpdateAsync(product, cancellationToken);

                // NiesPro Enterprise: Enhanced audit trail for successful deletion
                await _logsService.LogInformationAsync($"Product deleted (deactivated) successfully: {productName}", new Dictionary<string, object>
                {
                    ["ProductId"] = product.Id,
                    ["ProductName"] = productName,
                    ["CommandId"] = command.CommandId,
                    ["Operation"] = "SoftDelete",
                    ["DeactivatedAt"] = product.UpdatedAt
                });

                return ApiResponse<bool>.CreateSuccess(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                await _logsService.LogErrorAsync(ex, "Error occurred while deleting product", new Dictionary<string, object>
                {
                    ["ProductId"] = command.Id,
                    ["CommandId"] = command.CommandId,
                    ["Operation"] = "SoftDelete"
                });

                return ApiResponse<bool>.CreateError("An error occurred while deleting the product");
            }
        }
    }
}