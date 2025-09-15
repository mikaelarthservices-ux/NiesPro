using Microsoft.EntityFrameworkCore;
using Order.Domain.Repositories;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly OrderDbContext _context;

    public PaymentRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Failed && p.FailedAt < olderThan)
            .OrderBy(p => p.FailedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < olderThan)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaidAmountAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Completed)
            .ToListAsync(cancellationToken);

        return payments.Sum(p => p.GetRemainingAmount().Amount);
    }

    public async Task<decimal> GetTotalRefundedAmountAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var payments = await _context.Payments
            .Where(p => p.OrderId == orderId)
            .ToListAsync(cancellationToken);

        return payments.Sum(p => p.RefundedAmount.Amount);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Payment entity, CancellationToken cancellationToken = default)
    {
        _context.Payments.Remove(entity);
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
        return await _context.Payments
            .AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments.CountAsync(cancellationToken);
    }
}

public sealed class OrderItemRepository : IOrderItemRepository
{
    private readonly OrderDbContext _context;

    public OrderItemRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<OrderItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<OrderItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .Where(oi => oi.ProductId == productId)
            .OrderByDescending(oi => oi.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderItem?> GetByOrderAndProductAsync(Guid orderId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId, cancellationToken);
    }

    public async Task<int> GetTotalQuantityByProductAsync(Guid productId, DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.OrderItems.Where(oi => oi.ProductId == productId);
        
        if (fromDate.HasValue)
        {
            query = query.Where(oi => oi.AddedAt >= fromDate.Value);
        }

        return await query.SumAsync(oi => oi.Quantity, cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueByProductAsync(Guid productId, DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.OrderItems.Where(oi => oi.ProductId == productId);
        
        if (fromDate.HasValue)
        {
            query = query.Where(oi => oi.AddedAt >= fromDate.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        return items.Sum(oi => oi.TotalPrice.Amount);
    }

    public async Task<IEnumerable<(Guid ProductId, int TotalQuantity, decimal TotalRevenue)>> GetProductSalesStatsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        int top = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.OrderItems.AsQueryable();
        
        if (fromDate.HasValue)
        {
            query = query.Where(oi => oi.AddedAt >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            query = query.Where(oi => oi.AddedAt <= toDate.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        
        return items
            .GroupBy(oi => oi.ProductId)
            .Select(g => (
                ProductId: g.Key,
                TotalQuantity: g.Sum(oi => oi.Quantity),
                TotalRevenue: g.Sum(oi => oi.TotalPrice.Amount)
            ))
            .OrderByDescending(x => x.TotalRevenue)
            .Take(top);
    }

    public async Task<IEnumerable<OrderItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems
            .OrderByDescending(oi => oi.AddedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OrderItem entity, CancellationToken cancellationToken = default)
    {
        await _context.OrderItems.AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(OrderItem entity, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(OrderItem entity, CancellationToken cancellationToken = default)
    {
        _context.OrderItems.Remove(entity);
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
        return await _context.OrderItems
            .AnyAsync(oi => oi.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrderItems.CountAsync(cancellationToken);
    }
}