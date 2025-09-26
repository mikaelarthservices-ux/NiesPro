using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using Customer.Application.Common.Models;

namespace Customer.Application.Features.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Command to create a new customer - NiesPro Enterprise Standard
    /// </summary>
    public class CreateCustomerCommand : BaseCommand<ApiResponse<CustomerResponse>>
    {
        /// <summary>
        /// Customer first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Customer last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

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
        public string? PreferredLanguage { get; set; } = "fr-FR";

        /// <summary>
        /// Customer type (Regular, VIP, Corporate)
        /// </summary>
        public string CustomerType { get; set; } = "Regular";

        /// <summary>
        /// Customer addresses
        /// </summary>
        public List<CreateCustomerAddressRequest> Addresses { get; set; } = new();
    }

    /// <summary>
    /// Address request for customer creation
    /// </summary>
    public class CreateCustomerAddressRequest
    {
        /// <summary>
        /// Address type (Billing, Shipping, Main)
        /// </summary>
        public string AddressType { get; set; } = string.Empty; 

        /// <summary>
        /// Street address
        /// </summary>
        public string Street { get; set; } = string.Empty;

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Postal code
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; } = "France";

        /// <summary>
        /// Is default address
        /// </summary>
        public bool IsDefault { get; set; }
    }
}