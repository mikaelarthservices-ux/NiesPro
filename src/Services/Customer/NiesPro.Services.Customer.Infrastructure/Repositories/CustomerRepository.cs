using Microsoft.EntityFrameworkCore;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.Repositories;
using NiesPro.Services.Customer.Domain.ValueObjects;
using NiesPro.Services.Customer.Infrastructure.Data;
using System.Linq.Expressions;

namespace NiesPro.Services.Customer.Infrastructure.Repositories;

/// <summary>
/// Implémentation sophistiquée du repository Customer avec optimisations avancées
/// Support : Caching, Bulk Operations, Analytics, Performance optimisée
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;
    private readonly CustomerReadDbContext _readContext;

    public CustomerRepository(
        CustomerDbContext context, 
        CustomerReadDbContext readContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _readContext = readContext ?? throw new ArgumentNullException(nameof(readContext));
    }

    // ===== CORE OPERATIONS =====

    public async Task<Domain.Entities.Customer?> GetByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.PersonalInfo)
            .Include(c => c.ContactInfo)
            .Include(c => c.LoyaltyStats)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Domain.Entities.Customer?> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _context.Customers
            .Include(c => c.PersonalInfo)
            .Include(c => c.ContactInfo)
            .Include(c => c.LoyaltyStats)
            .FirstOrDefaultAsync(c => c.ContactInfo.Email == email.ToLowerInvariant(), 
                cancellationToken);
    }

    public async Task<Domain.Entities.Customer?> GetByCustomerNumberAsync(
        string customerNumber, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerNumber))
            return null;

        return await _context.Customers
            .Include(c => c.PersonalInfo)
            .Include(c => c.ContactInfo)
            .Include(c => c.LoyaltyStats)
            .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber, 
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(
        string email, 
        Guid? excludeCustomerId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var query = _context.Customers
            .Where(c => c.ContactInfo.Email == email.ToLowerInvariant());

        if (excludeCustomerId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCustomerId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(
        Domain.Entities.Customer customer, 
        CancellationToken cancellationToken = default)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Domain.Entities.Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        _context.Customers.Update(customer);
    }

    public void Remove(Domain.Entities.Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        // Soft delete via propriété IsDeleted
        customer.Deactivate();
        _context.Customers.Update(customer);
    }

    // ===== ADVANCED SEARCH & FILTERING =====

    public async Task<(List<Domain.Entities.Customer> Items, int TotalCount)> SearchAsync(
        string? searchTerm = null,
        CustomerStatus? status = null,
        string? country = null,
        string? city = null,
        LoyaltyTier? loyaltyTier = null,
        DateTime? registrationFromDate = null,
        DateTime? registrationToDate = null,
        DateTime? lastLoginFromDate = null,
        DateTime? lastLoginToDate = null,
        bool? isEmailVerified = null,
        bool? isVip = null,
        decimal? minSpending = null,
        decimal? maxSpending = null,
        int? minOrders = null,
        int? maxOrders = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "RegistrationDate",
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _readContext.Customers.AsQueryable();

        // ===== FILTRES DE RECHERCHE =====
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(c => 
                c.CustomerNumber.Contains(searchLower) ||
                c.PersonalInfo.FirstName.ToLower().Contains(searchLower) ||
                c.PersonalInfo.LastName.ToLower().Contains(searchLower) ||
                c.ContactInfo.Email.ToLower().Contains(searchLower) ||
                c.ContactInfo.Phone.Contains(searchTerm));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(c => c.ContactInfo.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(c => c.ContactInfo.City == city);
        }

        if (loyaltyTier.HasValue)
        {
            query = query.Where(c => c.LoyaltyStats.CurrentTier == loyaltyTier.Value);
        }

        if (registrationFromDate.HasValue)
        {
            query = query.Where(c => c.RegistrationDate >= registrationFromDate.Value);
        }

        if (registrationToDate.HasValue)
        {
            query = query.Where(c => c.RegistrationDate <= registrationToDate.Value);
        }

        if (lastLoginFromDate.HasValue)
        {
            query = query.Where(c => c.LastLoginDate >= lastLoginFromDate.Value);
        }

        if (lastLoginToDate.HasValue)
        {
            query = query.Where(c => c.LastLoginDate <= lastLoginToDate.Value);
        }

        if (isEmailVerified.HasValue)
        {
            query = query.Where(c => c.IsEmailVerified == isEmailVerified.Value);
        }

        if (isVip.HasValue)
        {
            // Joindre avec CustomerProfile pour IsVip
            query = query.Where(c => _readContext.CustomerProfiles
                .Any(cp => cp.CustomerId == c.Id && cp.IsVip == isVip.Value));
        }

        // Filtres de dépenses et commandes nécessitent une jointure avec CustomerProfile
        if (minSpending.HasValue || maxSpending.HasValue || minOrders.HasValue || maxOrders.HasValue)
        {
            query = from c in query
                    join cp in _readContext.CustomerProfiles on c.Id equals cp.CustomerId
                    where (!minSpending.HasValue || cp.TotalSpent >= minSpending.Value) &&
                          (!maxSpending.HasValue || cp.TotalSpent <= maxSpending.Value) &&
                          (!minOrders.HasValue || cp.TotalOrders >= minOrders.Value) &&
                          (!maxOrders.HasValue || cp.TotalOrders <= maxOrders.Value)
                    select c;
        }

        // ===== COMPTAGE TOTAL =====
        var totalCount = await query.CountAsync(cancellationToken);

        // ===== TRI =====
        query = ApplySorting(query, sortBy, sortDescending);

        // ===== PAGINATION =====
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Domain.Entities.Customer>> GetBirthdayCustomersAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var month = date.Month;
        var day = date.Day;

        return await _readContext.Customers
            .Where(c => c.PersonalInfo.DateOfBirth.HasValue &&
                       c.PersonalInfo.DateOfBirth.Value.Month == month &&
                       c.PersonalInfo.DateOfBirth.Value.Day == day &&
                       c.Status == CustomerStatus.Active)
            .OrderBy(c => c.PersonalInfo.FirstName)
            .ThenBy(c => c.PersonalInfo.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Customer>> GetInactiveCustomersAsync(
        int daysSinceLastLogin,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastLogin);

        return await _readContext.Customers
            .Where(c => c.Status == CustomerStatus.Active &&
                       (c.LastLoginDate == null || c.LastLoginDate < cutoffDate))
            .OrderBy(c => c.LastLoginDate ?? c.RegistrationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Customer>> GetVipCustomersAsync(
        CancellationToken cancellationToken = default)
    {
        return await (from c in _readContext.Customers
                     join cp in _readContext.CustomerProfiles on c.Id equals cp.CustomerId
                     where cp.IsVip && c.Status == CustomerStatus.Active
                     orderby cp.CustomerLifetimeValue descending
                     select c).ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Customer>> GetCustomersByLoyaltyTierAsync(
        LoyaltyTier tier,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Customers
            .Where(c => c.LoyaltyStats.CurrentTier == tier && c.Status == CustomerStatus.Active)
            .OrderByDescending(c => c.LoyaltyStats.TotalPoints)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Customer>> GetCustomersBySegmentAsync(
        Guid segmentId,
        CancellationToken cancellationToken = default)
    {
        return await (from c in _readContext.Customers
                     join csm in _readContext.Set<Dictionary<string, object>>("CustomerSegmentMemberships") 
                         on c.Id equals csm["CustomerId"]
                     where (Guid)csm["SegmentId"] == segmentId && 
                           (bool)csm["IsActive"] == true &&
                           c.Status == CustomerStatus.Active
                     select c).ToListAsync(cancellationToken);
    }

    // ===== BULK OPERATIONS =====

    public async Task<int> BulkUpdateStatusAsync(
        List<Guid> customerIds,
        CustomerStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        if (!customerIds.Any())
            return 0;

        return await _context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.Status, newStatus), 
                cancellationToken);
    }

    public async Task<int> BulkUpdateLoyaltyTierAsync(
        Dictionary<Guid, LoyaltyTier> customerTierUpdates,
        CancellationToken cancellationToken = default)
    {
        if (!customerTierUpdates.Any())
            return 0;

        var affectedCount = 0;
        
        foreach (var update in customerTierUpdates)
        {
            var count = await _context.Customers
                .Where(c => c.Id == update.Key)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.LoyaltyStats.CurrentTier, update.Value), 
                    cancellationToken);
            affectedCount += count;
        }

        return affectedCount;
    }

    public async Task<List<Domain.Entities.Customer>> GetCustomersForBulkOperationAsync(
        Expression<Func<Domain.Entities.Customer, bool>> predicate,
        int batchSize = 1000,
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Customers
            .Where(predicate)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    // ===== ANALYTICS & STATISTICS =====

    public async Task<CustomerStatsResult> GetCustomerStatsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddYears(-1);
        var to = toDate ?? DateTime.UtcNow;

        var stats = await _readContext.Customers
            .Where(c => c.RegistrationDate >= from && c.RegistrationDate <= to)
            .GroupBy(c => 1)
            .Select(g => new CustomerStatsResult
            {
                TotalCustomers = g.Count(),
                ActiveCustomers = g.Count(c => c.Status == CustomerStatus.Active),
                InactiveCustomers = g.Count(c => c.Status == CustomerStatus.Inactive),
                NewCustomersThisMonth = g.Count(c => c.RegistrationDate >= DateTime.UtcNow.AddDays(-30)),
                VerifiedEmails = g.Count(c => c.IsEmailVerified),
                VerifiedPhones = g.Count(c => c.IsPhoneVerified),
                AverageRegistrationDays = g.Average(c => (DateTime.UtcNow - c.RegistrationDate).Days)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new CustomerStatsResult();
    }

    public async Task<List<CountryDistributionResult>> GetCustomerDistributionByCountryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Customers
            .Where(c => c.Status == CustomerStatus.Active && !string.IsNullOrEmpty(c.ContactInfo.Country))
            .GroupBy(c => c.ContactInfo.Country)
            .Select(g => new CountryDistributionResult
            {
                Country = g.Key,
                CustomerCount = g.Count(),
                Percentage = (decimal)g.Count() * 100 / _readContext.Customers.Count(c => c.Status == CustomerStatus.Active)
            })
            .OrderByDescending(r => r.CustomerCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyTierDistributionResult>> GetLoyaltyTierDistributionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _readContext.Customers
            .Where(c => c.Status == CustomerStatus.Active)
            .GroupBy(c => c.LoyaltyStats.CurrentTier)
            .Select(g => new LoyaltyTierDistributionResult
            {
                Tier = g.Key.ToString(),
                CustomerCount = g.Count(),
                AveragePoints = g.Average(c => c.LoyaltyStats.TotalPoints),
                TotalPoints = g.Sum(c => c.LoyaltyStats.TotalPoints)
            })
            .OrderBy(r => r.Tier)
            .ToListAsync(cancellationToken);
    }

    // ===== HELPER METHODS =====

    private static IQueryable<Domain.Entities.Customer> ApplySorting(
        IQueryable<Domain.Entities.Customer> query,
        string sortBy,
        bool sortDescending)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "customernumber" => sortDescending ? query.OrderByDescending(c => c.CustomerNumber) : query.OrderBy(c => c.CustomerNumber),
            "firstname" => sortDescending ? query.OrderByDescending(c => c.PersonalInfo.FirstName) : query.OrderBy(c => c.PersonalInfo.FirstName),
            "lastname" => sortDescending ? query.OrderByDescending(c => c.PersonalInfo.LastName) : query.OrderBy(c => c.PersonalInfo.LastName),
            "email" => sortDescending ? query.OrderByDescending(c => c.ContactInfo.Email) : query.OrderBy(c => c.ContactInfo.Email),
            "status" => sortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "registrationdate" => sortDescending ? query.OrderByDescending(c => c.RegistrationDate) : query.OrderBy(c => c.RegistrationDate),
            "lastlogindate" => sortDescending ? query.OrderByDescending(c => c.LastLoginDate) : query.OrderBy(c => c.LastLoginDate),
            "loyaltypoints" => sortDescending ? query.OrderByDescending(c => c.LoyaltyStats.TotalPoints) : query.OrderBy(c => c.LoyaltyStats.TotalPoints),
            "loyaltytier" => sortDescending ? query.OrderByDescending(c => c.LoyaltyStats.CurrentTier) : query.OrderBy(c => c.LoyaltyStats.CurrentTier),
            _ => sortDescending ? query.OrderByDescending(c => c.RegistrationDate) : query.OrderBy(c => c.RegistrationDate)
        };
    }
}

// ===== RESULT MODELS =====

public class CustomerStatsResult
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int VerifiedEmails { get; set; }
    public int VerifiedPhones { get; set; }
    public double AverageRegistrationDays { get; set; }
}

public class CountryDistributionResult
{
    public string Country { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal Percentage { get; set; }
}

public class LoyaltyTierDistributionResult
{
    public string Tier { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal AveragePoints { get; set; }
    public decimal TotalPoints { get; set; }
}