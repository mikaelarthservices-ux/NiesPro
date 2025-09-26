using Microsoft.EntityFrameworkCore;
using Payment.Domain.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories;

/// <summary>
/// Repository pour les paiements avec optimisations de performance
/// </summary>
public partial class PaymentRepository : BaseRepository<Domain.Entities.Payment>, IPaymentRepository
{
    public PaymentRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.Reference == reference, cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetByMerchantIdAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .Where(p => p.MerchantId == merchantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetRecentByCustomerIdAsync(Guid customerId, TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(timeSpan);
        
        return await _context.Payments
            .Where(p => p.CustomerId == customerId && p.CreatedAt >= cutoffDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Where(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> GetExpiredPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.Payments
            .Where(p => p.ExpiresAt.HasValue && p.ExpiresAt.Value < now && 
                       (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Created))
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Payment?> GetByNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber, cancellationToken);
    }

    public async Task<(List<Domain.Entities.Payment> payments, int totalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        
        var payments = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }

    public async Task<(List<Domain.Entities.Payment> payments, int totalCount)> SearchAsync(
        string? searchTerm, Guid? merchantId = null, Guid? customerId = null, List<PaymentStatus>? statuses = null,
        DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => p.PaymentNumber.Contains(searchTerm) || 
                                   p.Reference.Contains(searchTerm));
        }

        if (merchantId.HasValue)
        {
            query = query.Where(p => p.MerchantId == merchantId.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == customerId.Value);
        }

        if (statuses != null && statuses.Any())
        {
            query = query.Where(p => statuses.Contains(p.Status));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= toDate.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        
        var payments = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }

    public async Task<decimal> GetTotalAmountByMerchantAsync(Guid merchantId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.MerchantId == merchantId && 
                       p.CreatedAt >= from && 
                       p.CreatedAt <= to &&
                       p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount.Amount, cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(PaymentStatus status, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .CountAsync(p => p.Status == status && 
                           p.CreatedAt >= from && 
                           p.CreatedAt <= to, cancellationToken);
    }

    public async Task<List<Domain.Entities.Payment>> SearchPaymentsAsync(
        string? reference = null,
        Guid? customerId = null,
        Guid? merchantId = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(reference))
            query = query.Where(p => p.Reference.Contains(reference));

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (merchantId.HasValue)
            query = query.Where(p => p.MerchantId == merchantId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        return await query
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment.Domain.Entities.Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber, cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> GetByMerchantIdAndDateRangeAsync(
        Guid merchantId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.MerchantId == merchantId && p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // ✅ NiesPro Enterprise: Méthodes additionnelles pour les nouvelles fonctionnalités
    public async Task<List<Payment.Domain.Entities.Payment>> GetByCustomerIdAsync(
        Guid customerId, int page, int pageSize, PaymentStatus? status = null, 
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .Where(p => p.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetCountByCustomerIdAsync(
        Guid customerId, PaymentStatus? status = null, 
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Where(p => p.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<PaymentRefund>> GetRefundsByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRefunds
            .Include(r => r.Payment)
            .Where(r => r.PaymentId == paymentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRefundAsync(PaymentRefund refund, CancellationToken cancellationToken = default)
    {
        await _context.PaymentRefunds.AddAsync(refund, cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> SearchAsync(
        string? searchTerm = null,
        Guid? customerId = null,
        Guid? merchantId = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Include(p => p.Merchant)
            .Include(p => p.Transactions)
            .Include(p => p.Refunds)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.PaymentNumber.Contains(searchTerm) || 
                                   p.Reference.Contains(searchTerm) ||
                                   (p.Description != null && p.Description.Contains(searchTerm)));
        }

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (merchantId.HasValue)
            query = query.Where(p => p.MerchantId == merchantId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        string? searchTerm = null,
        Guid? customerId = null,
        Guid? merchantId = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.PaymentNumber.Contains(searchTerm) || 
                                   p.Reference.Contains(searchTerm) ||
                                   (p.Description != null && p.Description.Contains(searchTerm)));
        }

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (merchantId.HasValue)
            query = query.Where(p => p.MerchantId == merchantId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}

/// <summary>
/// Repository pour les transactions avec optimisations de performance
/// </summary>
public partial class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<Transaction?> GetByProcessorTransactionIdAsync(string processorTransactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .FirstOrDefaultAsync(t => t.ProcessorTransactionId == processorTransactionId, cancellationToken);
    }

    public async Task<Transaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber, cancellationToken);
    }

    public async Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .ThenInclude(p => p.Merchant)
            .Include(t => t.PaymentMethod)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);
    }

    public async Task<List<Transaction>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.PaymentMethod)
            .Where(t => t.PaymentId == paymentId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetRecentByCustomerAsync(Guid customerId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.CustomerId == customerId && t.CreatedAt >= since)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByPaymentMethodIdAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Where(t => t.PaymentMethodId == paymentMethodId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByTypeAndStatusAsync(TransactionType type, TransactionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .Where(t => t.Type == type && t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .Where(t => t.Status == TransactionStatus.Pending || t.Status == TransactionStatus.Processing)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetHighRiskTransactionsAsync(int minimumFraudScore, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Payment)
            .Include(t => t.PaymentMethod)
            .Where(t => t.FraudScore >= minimumFraudScore)
            .OrderByDescending(t => t.FraudScore)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalAmountByCustomerAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.CustomerId == customerId && 
                       t.CreatedAt >= from && 
                       t.CreatedAt <= to &&
                       t.Status == TransactionStatus.Completed)
            .SumAsync(t => t.Amount.Amount, cancellationToken);
    }

    public async Task<int> GetTransactionCountByIpAsync(string ipAddress, TimeSpan timeWindow, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        return await _context.Transactions
            .CountAsync(t => t.IpAddress == ipAddress && t.CreatedAt >= cutoffTime, cancellationToken);
    }

    public async Task<List<Transaction>> GetSuspiciousVelocityTransactionsAsync(Guid customerId, TimeSpan timeWindow, int minimumCount, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
        
        var transactionCount = await _context.Transactions
            .CountAsync(t => t.CustomerId == customerId && t.CreatedAt >= cutoffTime, cancellationToken);

        if (transactionCount >= minimumCount)
        {
            return await _context.Transactions
                .Where(t => t.CustomerId == customerId && t.CreatedAt >= cutoffTime)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        return new List<Transaction>();
    }

    public async Task<List<Transaction>> GetByMerchantIdAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.MerchantId == merchantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByStatusAsync(TransactionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repository pour les moyens de paiement
/// </summary>
public partial class PaymentMethodRepository : BaseRepository<PaymentMethod>, IPaymentMethodRepository
{
    public PaymentMethodRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<List<PaymentMethod>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.CustomerId == customerId && pm.IsActive)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod?> GetDefaultByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.CustomerId == customerId && pm.IsDefault && pm.IsActive, cancellationToken);
    }

    public async Task<List<PaymentMethod>> GetByTypeAsync(PaymentMethodType type, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.Type == type && pm.IsActive)
            .OrderByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetDefaultAsync(Guid paymentMethodId, Guid customerId, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Désactiver l'ancien défaut
            await _context.PaymentMethods
                .Where(pm => pm.CustomerId == customerId && pm.IsDefault)
                .ExecuteUpdateAsync(pm => pm.SetProperty(p => p.IsDefault, false), cancellationToken);

            // Activer le nouveau défaut
            var rowsAffected = await _context.PaymentMethods
                .Where(pm => pm.Id == paymentMethodId && pm.CustomerId == customerId)
                .ExecuteUpdateAsync(pm => pm.SetProperty(p => p.IsDefault, true), cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateLastUsedAsync(Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        await _context.PaymentMethods
            .Where(pm => pm.Id == paymentMethodId)
            .ExecuteUpdateAsync(pm => pm.SetProperty(p => p.LastUsedAt, DateTime.UtcNow), cancellationToken);
    }

    public async Task<List<PaymentMethod>> GetActiveMethodsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(pm => pm.IsActive)
            .OrderByDescending(pm => pm.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pm => pm.Token == token, cancellationToken);
    }
}

/// <summary>
/// Repository pour les cartes avec sécurité PCI-DSS
/// </summary>
public partial class CardRepository : BaseRepository<Card>, ICardRepository
{
    public CardRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<Card?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .FirstOrDefaultAsync(c => c.Token == token && c.IsActive, cancellationToken);
    }

    public async Task<Card?> GetByFingerprintAndCustomerAsync(string fingerprint, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .FirstOrDefaultAsync(c => c.Fingerprint == fingerprint && c.CustomerId == customerId && c.IsActive, cancellationToken);
    }

    public async Task<List<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .Where(c => c.CustomerId == customerId && c.IsActive)
            .OrderByDescending(c => c.IsDefault)
            .ThenByDescending(c => c.LastUsedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Card>> GetExpiringCardsAsync(int daysUntilExpiry, CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        var expiryThreshold = currentDate.AddDays(daysUntilExpiry);

        return await _context.Cards
            .Where(c => c.IsActive && 
                       new DateTime(c.ExpiryYear, c.ExpiryMonth, 1).AddMonths(1).AddDays(-1) <= expiryThreshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetDefaultAsync(Guid cardId, Guid customerId, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Désactiver l'ancienne carte par défaut
            await _context.Cards
                .Where(c => c.CustomerId == customerId && c.IsDefault)
                .ExecuteUpdateAsync(c => c.SetProperty(card => card.IsDefault, false), cancellationToken);

            // Activer la nouvelle carte par défaut
            var rowsAffected = await _context.Cards
                .Where(c => c.Id == cardId && c.CustomerId == customerId)
                .ExecuteUpdateAsync(c => c.SetProperty(card => card.IsDefault, true), cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateLastUsedAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        await _context.Cards
            .Where(c => c.Id == cardId)
            .ExecuteUpdateAsync(c => c.SetProperty(card => card.LastUsedAt, DateTime.UtcNow), cancellationToken);
    }

    public async Task DeactivateAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        await _context.Cards
            .Where(c => c.Id == cardId)
            .ExecuteUpdateAsync(c => c.SetProperty(card => card.IsActive, false), cancellationToken);
    }

    public async Task<Card?> GetByMaskedNumberAsync(string maskedNumber, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.MaskedNumber == maskedNumber && c.CustomerId == customerId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

/// <summary>
/// Repository pour les authentifications 3D Secure
/// </summary>
public partial class ThreeDSecureRepository : BaseRepository<ThreeDSecureAuthentication>, IThreeDSecureRepository
{
    public ThreeDSecureRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<ThreeDSecureAuthentication?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .Include(tds => tds.Card)
            .FirstOrDefaultAsync(tds => tds.TransactionId == transactionId, cancellationToken);
    }

    public async Task<List<ThreeDSecureAuthentication>> GetByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .Include(tds => tds.Card)
            .Where(tds => tds.CardId == cardId)
            .OrderByDescending(tds => tds.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ThreeDSecureAuthentication>> GetPendingAuthenticationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .Include(tds => tds.Card)
            .Where(tds => tds.Status == ThreeDSecureStatus.Pending)
            .OrderBy(tds => tds.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ThreeDSecureAuthentication>> GetExpiredAuthenticationsAsync(TimeSpan expiryWindow, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(expiryWindow);

        return await _context.ThreeDSecureAuthentications
            .Where(tds => tds.Status == ThreeDSecureStatus.Pending && tds.CreatedAt < cutoffTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ThreeDSecureAuthentication>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .Include(tds => tds.Card)
            .Where(tds => tds.Card.CustomerId == customerId)
            .OrderByDescending(tds => tds.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repository pour les marchands
/// </summary>
public partial class MerchantRepository : BaseRepository<Merchant>, IMerchantRepository
{
    public MerchantRepository(PaymentDbContext context) : base(context)
    {
    }

    public async Task<Merchant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .FirstOrDefaultAsync(m => m.Code == code, cancellationToken);
    }

    public async Task<Merchant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
    }

    public async Task<List<Merchant>> GetByStatusAsync(MerchantStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .Where(m => m.Status == status)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Merchant>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .Where(m => m.Name.Contains(searchTerm) || m.Code.Contains(searchTerm))
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Merchant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .FirstOrDefaultAsync(m => m.ApiKey == apiKey, cancellationToken);
    }

    public async Task<bool> ApiKeyExistsAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .AnyAsync(m => m.ApiKey == apiKey, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Merchants
            .AnyAsync(m => m.Email == email, cancellationToken);
    }
}