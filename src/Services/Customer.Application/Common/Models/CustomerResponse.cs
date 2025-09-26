namespace Customer.Application.Common.Models
{
    /// <summary>
    /// Customer response model for API responses
    /// </summary>
    public class CustomerResponse
    {
        /// <summary>
        /// Customer unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Customer first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Customer last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Customer full name
        /// </summary>
        public string FullName => $"{FirstName} {LastName}".Trim();

        /// <summary>
        /// Customer email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Customer phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Customer mobile phone number
        /// </summary>
        public string? MobilePhone { get; set; }

        /// <summary>
        /// Customer date of birth
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Customer gender
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Customer notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Customer preferred language
        /// </summary>
        public string? PreferredLanguage { get; set; }

        /// <summary>
        /// Customer type
        /// </summary>
        public string CustomerType { get; set; } = string.Empty;

        /// <summary>
        /// Customer active status
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Customer loyalty points
        /// </summary>
        public decimal LoyaltyPoints { get; set; }

        /// <summary>
        /// Customer total spent
        /// </summary>
        public decimal TotalSpent { get; set; }

        /// <summary>
        /// Customer addresses
        /// </summary>
        public List<CustomerAddressResponse> Addresses { get; set; } = new();

        /// <summary>
        /// Customer creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Customer last update date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Customer address response model
    /// </summary>
    public class CustomerAddressResponse
    {
        public Guid Id { get; set; }
        public string AddressType { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Paginated customer response
    /// </summary>
    public class GetCustomersResponse
    {
        public List<CustomerResponse> Customers { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}