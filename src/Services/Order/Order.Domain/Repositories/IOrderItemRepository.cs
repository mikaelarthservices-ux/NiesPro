using NiesPro.Contracts.Common;
using Order.Domain.Entities;

namespace Order.Domain.Repositories;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<OrderItem?> GetByOrderAndProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default);
    Task<int> GetTotalQuantityByProductAsync(Guid productId, DateTime? fromDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueByProductAsync(Guid productId, DateTime? fromDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<(Guid ProductId, int TotalQuantity, decimal TotalRevenue)>> GetProductSalesStatsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        int top = 10, 
        CancellationToken cancellationToken = default);
}