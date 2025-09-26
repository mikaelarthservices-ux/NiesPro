using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using Customer.Application.Common.Models;

namespace Customer.Application.Features.Customers.Queries.GetCustomerById
{
    /// <summary>
    /// Query to get a customer by ID - NiesPro Enterprise Standard
    /// </summary>
    public class GetCustomerByIdQuery : BaseQuery<ApiResponse<CustomerResponse>>
    {
        /// <summary>
        /// Customer ID to retrieve
        /// </summary>
        public Guid CustomerId { get; set; }
    }
}