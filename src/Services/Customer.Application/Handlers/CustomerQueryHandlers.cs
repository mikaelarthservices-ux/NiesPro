using MediatR;
using Customer.Application.Queries;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Common;

namespace Customer.Application.Handlers
{
    /// <summary>
    /// Handler for GetCustomerByIdQuery
    /// </summary>
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Customer.Domain.Aggregates.CustomerAggregate.Customer?>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public GetCustomerByIdQueryHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _customerRepository.GetByIdAsync(request.CustomerId);
        }
    }

    /// <summary>
    /// Handler for GetCustomerByEmailQuery
    /// </summary>
    public class GetCustomerByEmailQueryHandler : IRequestHandler<GetCustomerByEmailQuery, Customer.Domain.Aggregates.CustomerAggregate.Customer?>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public GetCustomerByEmailQueryHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer.Domain.Aggregates.CustomerAggregate.Customer?> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.FirstOrDefault(c => c.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Handler for GetCustomersQuery
    /// </summary>
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public GetCustomersQueryHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<PagedResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var allCustomers = await _customerRepository.GetAllAsync();
            var customersList = allCustomers.ToList();
            var query = customersList.AsQueryable();

            // Apply filters
            if (request.IsActive.HasValue)
                query = query.Where(c => c.IsActive == request.IsActive.Value);

            if (request.IsVip.HasValue)
                query = query.Where(c => c.IsVip == request.IsVip.Value);

            if (!string.IsNullOrEmpty(request.CustomerType))
                query = query.Where(c => c.CustomerType == request.CustomerType);

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm) ||
                    (c.Phone != null && c.Phone.Contains(searchTerm)));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "firstname":
                        query = request.SortDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName);
                        break;
                    case "lastname":
                        query = request.SortDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName);
                        break;
                    case "email":
                        query = request.SortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email);
                        break;
                    case "createdat":
                        query = request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt);
                        break;
                    case "totalspent":
                        query = request.SortDescending ? query.OrderByDescending(c => c.TotalSpent) : query.OrderBy(c => c.TotalSpent);
                        break;
                    default:
                        query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
            }

            var totalCount = query.Count();
            var items = query.Skip((request.Page - 1) * request.PageSize)
                            .Take(request.PageSize)
                            .ToList();

            return new PagedResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }

    /// <summary>
    /// Handler for GetVipCustomersQuery
    /// </summary>
    public class GetVipCustomersQueryHandler : IRequestHandler<GetVipCustomersQuery, List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public GetVipCustomersQueryHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> Handle(GetVipCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllAsync();
            IEnumerable<Customer.Domain.Aggregates.CustomerAggregate.Customer> vipCustomers = customers.Where(c => c.IsVip && c.IsActive)
                                      .OrderByDescending(c => c.TotalSpent)
                                      .ThenByDescending(c => c.LoyaltyPoints);

            if (request.TopCount.HasValue)
                vipCustomers = vipCustomers.Take(request.TopCount.Value);

            return vipCustomers.ToList();
        }
    }

    /// <summary>
    /// Handler for GetCustomerAddressesQuery
    /// </summary>
    public class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, List<CustomerAddress>>
    {
        private readonly IRepository<CustomerAddress> _addressRepository;

        public GetCustomerAddressesQueryHandler(IRepository<CustomerAddress> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<List<CustomerAddress>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAllAsync();
            var filteredAddresses = addresses.Where(a => a.CustomerId == request.CustomerId);

            if (request.IsActive.HasValue)
                filteredAddresses = filteredAddresses.Where(a => a.IsActive == request.IsActive.Value);

            return filteredAddresses.OrderByDescending(a => a.IsDefault)
                       .ThenBy(a => a.Type)
                       .ToList();
        }
    }

    /// <summary>
    /// Handler for GetCustomerAddressByIdQuery
    /// </summary>
    public class GetCustomerAddressByIdQueryHandler : IRequestHandler<GetCustomerAddressByIdQuery, CustomerAddress?>
    {
        private readonly IRepository<CustomerAddress> _addressRepository;

        public GetCustomerAddressByIdQueryHandler(IRepository<CustomerAddress> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<CustomerAddress?> Handle(GetCustomerAddressByIdQuery request, CancellationToken cancellationToken)
        {
            return await _addressRepository.GetByIdAsync(request.AddressId);
        }
    }

    /// <summary>
    /// Handler for GetCustomerPreferencesQuery
    /// </summary>
    public class GetCustomerPreferencesQueryHandler : IRequestHandler<GetCustomerPreferencesQuery, List<CustomerPreference>>
    {
        private readonly IRepository<CustomerPreference> _preferenceRepository;

        public GetCustomerPreferencesQueryHandler(IRepository<CustomerPreference> preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<List<CustomerPreference>> Handle(GetCustomerPreferencesQuery request, CancellationToken cancellationToken)
        {
            var preferences = await _preferenceRepository.GetAllAsync();
            var query = preferences.Where(p => p.CustomerId == request.CustomerId && p.IsActive);

            if (!string.IsNullOrEmpty(request.Category))
                query = query.Where(p => p.Category == request.Category);

            return query.OrderBy(p => p.Category)
                       .ThenBy(p => p.PreferenceKey)
                       .ToList();
        }
    }

    /// <summary>
    /// Handler for GetCustomerStatsQuery
    /// </summary>
    public class GetCustomerStatsQueryHandler : IRequestHandler<GetCustomerStatsQuery, CustomerStatsDto>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;
        private readonly IRepository<CustomerAddress> _addressRepository;
        private readonly IRepository<CustomerPreference> _preferenceRepository;

        public GetCustomerStatsQueryHandler(
            IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository,
            IRepository<CustomerAddress> addressRepository,
            IRepository<CustomerPreference> preferenceRepository)
        {
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
            _preferenceRepository = preferenceRepository;
        }

        public async Task<CustomerStatsDto> Handle(GetCustomerStatsQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new ArgumentException($"Customer with ID {request.CustomerId} not found");

            var addresses = await _addressRepository.GetAllAsync();
            var preferences = await _preferenceRepository.GetAllAsync();

            var addressCount = addresses.Count(a => a.CustomerId == request.CustomerId && a.IsActive);
            var preferenceCount = preferences.Count(p => p.CustomerId == request.CustomerId && p.IsActive);

            return new CustomerStatsDto
            {
                CustomerId = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                TotalOrders = customer.TotalOrders,
                TotalSpent = customer.TotalSpent,
                LoyaltyPoints = customer.LoyaltyPoints,
                LastVisit = customer.LastVisit,
                CustomerSince = customer.CreatedAt,
                IsVip = customer.IsVip,
                CustomerType = customer.CustomerType ?? string.Empty,
                AddressCount = addressCount,
                PreferenceCount = preferenceCount
            };
        }
    }

    /// <summary>
    /// Handler for SearchCustomersQuery
    /// </summary>
    public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        private readonly IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> _customerRepository;

        public SearchCustomersQueryHandler(IRepository<Customer.Domain.Aggregates.CustomerAggregate.Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetAllAsync();
            var searchTerm = request.SearchTerm.ToLower();

            var results = customers.Where(c => c.IsActive &&
                (c.FirstName.ToLower().Contains(searchTerm) ||
                 c.LastName.ToLower().Contains(searchTerm) ||
                 c.Email.ToLower().Contains(searchTerm) ||
                 (c.Phone != null && c.Phone.Contains(searchTerm)) ||
                 (c.MobilePhone != null && c.MobilePhone.Contains(searchTerm))))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Take(request.MaxResults)
                .ToList();

            return results;
        }
    }
}