using Microsoft.EntityFrameworkCore;
using Order.Domain.Repositories;
using Order.Domain.Enums;
using Order.Infrastructure.Data;
using Order.Infrastructure.EventStore;
using NiesPro.Contracts.Common;

namespace Order.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly IEventStore _eventStore;

    public OrderRepository(OrderDbContext context, IEventStore eventStore)
    {
        _context = context;
        _eventStore = eventStore;
    }

    public async Task<Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<Domain.Entities.Order?> GetWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Domain.Entities.Order?> GetWithPaymentsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Domain.Entities.Order?> GetFullOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<Domain.Entities.Order>> GetPaginatedAsync(int page, int pageSize, OrderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PaginatedResult<Domain.Entities.Order>.Create(orders, totalCount, page, pageSize);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var prefix = $"ORD-{today:yyyyMMdd}";
        
        var lastOrderToday = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastOrderToday == null)
        {
            return $"{prefix}-0001";
        }

        var lastNumberPart = lastOrderToday.OrderNumber.Split('-').Last();
        if (int.TryParse(lastNumberPart, out var lastNumber))
        {
            return $"{prefix}-{(lastNumber + 1):D4}";
        }

        return $"{prefix}-0001";
    }

    public async Task<bool> ExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Order entity, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(entity, cancellationToken);
        
        // Sauvegarder les events dans l'Event Store
        if (entity.DomainEvents.Any())
        {
            await _eventStore.SaveEventsAsync(
                entity.Id,
                nameof(Domain.Entities.Order),
                entity.DomainEvents,
                0, // Version initiale
                cancellationToken);
        }
    }

    public Task UpdateAsync(Domain.Entities.Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Order entity, CancellationToken cancellationToken = default)
    {
        _context.Orders.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AnyAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(cancellationToken);
    }
}