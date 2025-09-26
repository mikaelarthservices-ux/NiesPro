using MediatR;
using Microsoft.Extensions.Logging;
using Customer.Application.Common.Models;
using Customer.Domain.Interfaces;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;
using NiesPro.Contracts.Application.CQRS;

namespace Customer.Application.Features.Customers.Queries.GetCustomerById
{
    /// <summary>
    /// Get customer by ID query handler - NiesPro Enterprise Standard with BaseQueryHandler
    /// </summary>
    public class GetCustomerByIdQueryHandler : BaseQueryHandler<GetCustomerByIdQuery, ApiResponse<CustomerResponse>>, IRequestHandler<GetCustomerByIdQuery, ApiResponse<CustomerResponse>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogsServiceClient _logsService;

        public GetCustomerByIdQueryHandler(
            ICustomerRepository customerRepository,
            ILogsServiceClient logsService,
            ILogger<GetCustomerByIdQueryHandler> logger) : base(logger)
        {
            _customerRepository = customerRepository;
            _logsService = logsService;
        }

        public async Task<ApiResponse<CustomerResponse>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
        {
            return await HandleAsync(query, cancellationToken);
        }

        protected override async Task<ApiResponse<CustomerResponse>> ExecuteAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // Validate customer ID
                if (query.CustomerId == Guid.Empty)
                {
                    return ApiResponse<CustomerResponse>.CreateError(
                        "Invalid customer ID", 
                        400
                    );
                }

                await _logsService.LogInformationAsync(
                    $"Retrieving customer by ID: {query.CustomerId}",
                    new Dictionary<string, object>
                    {
                        ["QueryId"] = query.QueryId,
                        ["CustomerId"] = query.CustomerId
                    }
                );

                var customer = await _customerRepository.GetByIdAsync(query.CustomerId);
                
                if (customer == null)
                {
                    await _logsService.LogWarningAsync(
                        $"Customer not found with ID: {query.CustomerId}",
                        new Dictionary<string, object>
                        {
                            ["QueryId"] = query.QueryId,
                            ["CustomerId"] = query.CustomerId
                        }
                    );

                    return ApiResponse<CustomerResponse>.CreateError(
                        "Customer not found", 
                        404
                    );
                }

                // Map to response
                var response = MapToResponse(customer);

                await _logsService.LogInformationAsync(
                    $"Customer retrieved successfully: {customer.Id}",
                    new Dictionary<string, object>
                    {
                        ["CustomerId"] = customer.Id,
                        ["Email"] = customer.Email
                    }
                );

                return ApiResponse<CustomerResponse>.CreateSuccess(
                    response, 
                    "Customer retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", query.CustomerId);

                await _logsService.LogErrorAsync(
                    ex,
                    $"Customer retrieval failed: {ex.Message}",
                    new Dictionary<string, object>
                    {
                        ["QueryId"] = query.QueryId,
                        ["CustomerId"] = query.CustomerId,
                        ["Error"] = ex.Message
                    }
                );

                return ApiResponse<CustomerResponse>.CreateError(
                    "An error occurred while retrieving customer", 
                    500
                );
            }
        }

        private static CustomerResponse MapToResponse(Customer.Domain.Aggregates.CustomerAggregate.Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                MobilePhone = customer.MobilePhone,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                Notes = customer.Notes,
                PreferredLanguage = customer.PreferredLanguage,
                CustomerType = customer.CustomerType,
                IsActive = customer.IsActive,
                LoyaltyPoints = customer.LoyaltyPoints,
                TotalSpent = customer.TotalSpent,
                Addresses = customer.Addresses.Select(a => new CustomerAddressResponse
                {
                    Id = a.Id,
                    AddressType = a.Type,
                    Street = a.AddressLine1,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    IsDefault = a.IsDefault,
                    CreatedAt = a.CreatedAt
                }).ToList(),
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            };
        }
    }
}