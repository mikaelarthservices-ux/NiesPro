using Microsoft.EntityFrameworkCore;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Common;

namespace Customer.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Customer entity
    /// </summary>
    public class CustomerRepository : BaseRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer>, ICustomerRepository
    {
        public CustomerRepository(CustomerContext context) : base(context)
        {
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Addresses.Where(a => a.IsActive))
                .Include(c => c.Preferences.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower() && c.IsActive, cancellationToken);
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetVipCustomersAsync(int? topCount = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Customers
                .Where(c => c.IsVip && c.IsActive)
                .OrderByDescending(c => c.TotalSpent)
                .ThenByDescending(c => c.LoyaltyPoints);

            if (topCount.HasValue)
                return await query.Take(topCount.Value).ToListAsync(cancellationToken);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> SearchCustomersAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default)
        {
            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Customers
                .Where(c => c.IsActive &&
                    (c.FirstName.ToLower().Contains(lowerSearchTerm) ||
                     c.LastName.ToLower().Contains(lowerSearchTerm) ||
                     c.Email.ToLower().Contains(lowerSearchTerm) ||
                     (c.Phone != null && c.Phone.Contains(searchTerm)) ||
                     (c.MobilePhone != null && c.MobilePhone.Contains(searchTerm))))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetWithAddressesAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Addresses.Where(a => a.IsActive))
                .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetWithPreferencesAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Preferences.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetCompleteAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Addresses.Where(a => a.IsActive))
                .Include(c => c.Preferences.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, Guid? excludeCustomerId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Customers
                .Where(c => c.Email.ToLower() == email.ToLower() && c.IsActive);

            if (excludeCustomerId.HasValue)
                query = query.Where(c => c.Id != excludeCustomerId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetCustomersByTypeAsync(string customerType, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Where(c => c.CustomerType == customerType && c.IsActive)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetRecentCustomersAsync(int days = 30, CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow.AddDays(-days);
            
            return await _context.Customers
                .Where(c => c.CreatedAt >= since && c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public override async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Addresses.Where(a => a.IsActive))
                .Include(c => c.Preferences.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public override async Task<IEnumerable<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Addresses.Where(a => a.IsActive))
                .Include(c => c.Preferences.Where(p => p.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Interface for Customer repository with specific methods
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer>
    {
        Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetVipCustomersAsync(int? topCount = null, CancellationToken cancellationToken = default);
        Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> SearchCustomersAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default);
        Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetWithAddressesAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetWithPreferencesAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> GetCompleteAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithEmailAsync(string email, Guid? excludeCustomerId = null, CancellationToken cancellationToken = default);
        Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetCustomersByTypeAsync(string customerType, CancellationToken cancellationToken = default);
        Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> GetRecentCustomersAsync(int days = 30, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository implementation for CustomerAddress entity
    /// </summary>
    public class CustomerAddressRepository : BaseRepository<CustomerAddress>, ICustomerAddressRepository
    {
        public CustomerAddressRepository(CustomerContext context) : base(context)
        {
        }

        public async Task<List<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && a.IsActive)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Type)
                .ToListAsync(cancellationToken);
        }

        public async Task<CustomerAddress?> GetPrimaryAddressAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerAddresses
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.IsDefault && a.IsActive, cancellationToken);
        }

        public async Task<List<CustomerAddress>> GetByTypeAsync(Guid customerId, string addressType, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && a.Type == addressType && a.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task SetPrimaryAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken = default)
        {
            // Remove default from all other addresses
            var addresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == customerId && a.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var address in addresses)
            {
                address.IsDefault = address.Id == addressId;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Interface for CustomerAddress repository
    /// </summary>
    public interface ICustomerAddressRepository : IRepository<CustomerAddress>
    {
        Task<List<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<CustomerAddress?> GetPrimaryAddressAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<List<CustomerAddress>> GetByTypeAsync(Guid customerId, string addressType, CancellationToken cancellationToken = default);
        Task SetPrimaryAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Repository implementation for CustomerPreference entity
    /// </summary>
    public class CustomerPreferenceRepository : BaseRepository<CustomerPreference>, ICustomerPreferenceRepository
    {
        public CustomerPreferenceRepository(CustomerContext context) : base(context)
        {
        }

        public async Task<List<CustomerPreference>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerPreferences
                .Where(p => p.CustomerId == customerId && p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.PreferenceKey)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CustomerPreference>> GetByCategoryAsync(Guid customerId, string category, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerPreferences
                .Where(p => p.CustomerId == customerId && p.Category == category && p.IsActive)
                .OrderBy(p => p.PreferenceKey)
                .ToListAsync(cancellationToken);
        }

        public async Task<CustomerPreference?> GetPreferenceAsync(Guid customerId, string preferenceKey, CancellationToken cancellationToken = default)
        {
            return await _context.CustomerPreferences
                .FirstOrDefaultAsync(p => p.CustomerId == customerId && 
                                        p.PreferenceKey == preferenceKey && 
                                        p.IsActive, cancellationToken);
        }

        public async Task<bool> SetPreferenceAsync(Guid customerId, string preferenceKey, string? preferenceValue, string? category = null, CancellationToken cancellationToken = default)
        {
            var existing = await GetPreferenceAsync(customerId, preferenceKey, cancellationToken);

            if (existing != null)
            {
                existing.PreferenceValue = preferenceValue;
                existing.Category = category ?? existing.Category;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            else
            {
                var newPreference = new CustomerPreference
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    PreferenceKey = preferenceKey,
                    PreferenceValue = preferenceValue,
                    Category = category,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CustomerPreferences.Add(newPreference);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }

    /// <summary>
    /// Interface for CustomerPreference repository
    /// </summary>
    public interface ICustomerPreferenceRepository : IRepository<CustomerPreference>
    {
        Task<List<CustomerPreference>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        Task<List<CustomerPreference>> GetByCategoryAsync(Guid customerId, string category, CancellationToken cancellationToken = default);
        Task<CustomerPreference?> GetPreferenceAsync(Guid customerId, string preferenceKey, CancellationToken cancellationToken = default);
        Task<bool> SetPreferenceAsync(Guid customerId, string preferenceKey, string? preferenceValue, string? category = null, CancellationToken cancellationToken = default);
    }
}