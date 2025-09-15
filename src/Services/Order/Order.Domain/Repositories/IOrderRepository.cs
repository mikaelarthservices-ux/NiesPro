using NiesPro.Contracts.Common;
using Order.Domain.Enums;

namespace Order.Domain.Repositories;

public interface IOrderRepository : IRepository<Entities.Order>
{
    Task<Entities.Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Entities.Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Entities.Order?> GetWithPaymentsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Entities.Order?> GetFullOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Entities.Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Entities.Order>> GetPaginatedAsync(int page, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}