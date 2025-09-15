using Microsoft.EntityFrameworkCore;
using Payment.Domain.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories;

/// <summary>
/// Extension des repositories Payment avec méthodes manquantes pour l'interface
/// </summary>
public partial class PaymentRepository
{
    // Méthodes manquantes pour l'interface IPaymentRepository

    public async Task<Payment.Domain.Entities.Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber, cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> SearchAsync(
        string? searchTerm, 
        Guid? merchantId = null, 
        Guid? customerId = null, 
        List<PaymentStatus>? statuses = null,
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        int pageNumber = 1, 
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.AsQueryable();

        // Filtres
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.PaymentNumber.Contains(searchTerm));
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

        // Pagination
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<List<Payment.Domain.Entities.Payment>> GetByMerchantIdAndDateRangeAsync(
        Guid merchantId, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.MerchantId == merchantId && 
                       p.CreatedAt >= fromDate && 
                       p.CreatedAt <= toDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Extension des repositories Transaction avec méthodes manquantes
/// </summary>
public partial class TransactionRepository
{
    public async Task<Transaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.TransactionNumber == transactionNumber, cancellationToken);
    }

    public async Task<List<Transaction>> GetByMerchantIdAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.MerchantId == merchantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByStatusAsync(TransactionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.CreatedAt >= from && t.CreatedAt <= to)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Extension des repositories PaymentMethod avec méthodes manquantes
/// </summary>
public partial class PaymentMethodRepository
{
    public async Task<List<PaymentMethod>> GetActiveMethodsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Token == token, cancellationToken);
    }
}

/// <summary>
/// Extension des repositories Card avec méthodes manquantes
/// </summary>
public partial class CardRepository
{
    public async Task<Card?> GetByMaskedNumberAsync(string maskedNumber, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .FirstOrDefaultAsync(c => c.MaskedNumber == maskedNumber && c.CustomerId == customerId, cancellationToken);
    }
}

/// <summary>
/// Extension des repositories ThreeDSecure avec méthodes manquantes
/// </summary>
public partial class ThreeDSecureRepository
{
    public async Task<ThreeDSecureAuthentication?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .FirstOrDefaultAsync(tds => tds.TransactionId == transactionId, cancellationToken);
    }

    public async Task<List<ThreeDSecureAuthentication>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreeDSecureAuthentications
            .Where(tds => tds.CustomerId == customerId)
            .OrderByDescending(tds => tds.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Extension des repositories Merchant avec méthodes manquantes
/// </summary>
public partial class MerchantRepository
{
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