using MediatR;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.Application.Queries
{
    /// <summary>
    /// Query to get customer by ID
    /// </summary>
    public class GetCustomerByIdQuery : IRequest<Customer.Domain.Aggregates.CustomerAggregate.Customer?>
    {
        public Guid CustomerId { get; set; }
    }

    /// <summary>
    /// Query to get customer by email
    /// </summary>
    public class GetCustomerByEmailQuery : IRequest<Customer.Domain.Aggregates.CustomerAggregate.Customer?>
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Query to get customers with pagination
    /// </summary>
    public class GetCustomersQuery : IRequest<PagedResult<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVip { get; set; }
        public string? CustomerType { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// Query to get VIP customers
    /// </summary>
    public class GetVipCustomersQuery : IRequest<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        public int? TopCount { get; set; }
    }

    /// <summary>
    /// Query to get customer addresses
    /// </summary>
    public class GetCustomerAddressesQuery : IRequest<List<CustomerAddress>>
    {
        public Guid CustomerId { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Query to get customer address by ID
    /// </summary>
    public class GetCustomerAddressByIdQuery : IRequest<CustomerAddress?>
    {
        public Guid AddressId { get; set; }
    }

    /// <summary>
    /// Query to get customer preferences
    /// </summary>
    public class GetCustomerPreferencesQuery : IRequest<List<CustomerPreference>>
    {
        public Guid CustomerId { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// Query to get customer statistics
    /// </summary>
    public class GetCustomerStatsQuery : IRequest<CustomerStatsDto>
    {
        public Guid CustomerId { get; set; }
    }

    /// <summary>
    /// Query to search customers
    /// </summary>
    public class SearchCustomersQuery : IRequest<List<Customer.Domain.Aggregates.CustomerAggregate.Customer>>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int MaxResults { get; set; } = 50;
    }

    /// <summary>
    /// Paged result wrapper
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// Customer statistics DTO
    /// </summary>
    public class CustomerStatsDto
    {
        public Guid CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal LoyaltyPoints { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime CustomerSince { get; set; }
        public bool IsVip { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public int AddressCount { get; set; }
        public int PreferenceCount { get; set; }
    }
}