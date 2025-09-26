using MediatR;
using Microsoft.Extensions.Logging;
using Customer.Application.Common.Models;
using Customer.Domain.Aggregates.CustomerAggregate;
using Customer.Domain.Interfaces;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;
using NiesPro.Contracts.Application.CQRS;

namespace Customer.Application.Features.Customers.Commands.CreateCustomer
{
    /// <summary>
    /// Create customer command handler - NiesPro Enterprise Standard with BaseCommandHandler
    /// </summary>
    public class CreateCustomerCommandHandler : BaseCommandHandler<CreateCustomerCommand, ApiResponse<CustomerResponse>>, IRequestHandler<CreateCustomerCommand, ApiResponse<CustomerResponse>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogsServiceClient _logsService;
        private readonly IAuditServiceClient _auditService;

        public CreateCustomerCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ILogsServiceClient logsService,
            IAuditServiceClient auditService,
            ILogger<CreateCustomerCommandHandler> logger) : base(logger)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _logsService = logsService;
            _auditService = auditService;
        }

        public async Task<ApiResponse<CustomerResponse>> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
        {
            return await HandleAsync(command, cancellationToken);
        }

        protected override async Task<ApiResponse<CustomerResponse>> ExecuteAsync(CreateCustomerCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Creating new customer with email: {Email}", command.Email);

                // Log command details for audit
                await _logsService.LogInformationAsync(
                    $"Creating customer: {command.FirstName} {command.LastName}",
                    new Dictionary<string, object>
                    {
                        ["CommandId"] = command.CommandId,
                        ["Email"] = command.Email,
                        ["CustomerType"] = command.CustomerType,
                        ["AddressCount"] = command.Addresses.Count
                    }
                );

                // Check if customer with email already exists
                var existingCustomer = await _customerRepository.GetByEmailAsync(command.Email);
                if (existingCustomer != null)
                {
                    await _logsService.LogWarningAsync(
                        $"Customer creation failed - email already exists: {command.Email}",
                        new Dictionary<string, object>
                        {
                            ["CommandId"] = command.CommandId,
                            ["ExistingCustomerId"] = existingCustomer.Id
                        }
                    );

                    return ApiResponse<CustomerResponse>.CreateError(
                        "Customer with this email already exists", 
                        400
                    );
                }

                // Create customer entity
                var customer = new Customer.Domain.Aggregates.CustomerAggregate.Customer
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    Email = command.Email,
                    Phone = command.Phone,
                    MobilePhone = command.MobilePhone,
                    DateOfBirth = command.DateOfBirth,
                    Gender = command.Gender,
                    Notes = command.Notes,
                    PreferredLanguage = command.PreferredLanguage,
                    CustomerType = command.CustomerType
                };

                // Add addresses if provided
                foreach (var addressRequest in command.Addresses)
                {
                    var address = new CustomerAddress
                    {
                        Type = addressRequest.AddressType,
                        AddressLine1 = addressRequest.Street,
                        City = addressRequest.City,
                        PostalCode = addressRequest.PostalCode,
                        Country = addressRequest.Country,
                        IsDefault = addressRequest.IsDefault,
                        CustomerId = customer.Id
                    };
                    customer.Addresses.Add(address);
                }

                // Save to repository
                await _customerRepository.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Audit trail
                await _auditService.AuditCreateAsync(
                    userId: "System", // TODO: Get from context
                    userName: "System",
                    entityName: "Customer",
                    entityId: customer.Id.ToString(),
                    metadata: new Dictionary<string, object>
                    {
                        ["Email"] = customer.Email,
                        ["CustomerType"] = customer.CustomerType,
                        ["AddressCount"] = customer.Addresses.Count
                    }
                );

                // Map to response
                var response = MapToResponse(customer);

                await _logsService.LogInformationAsync(
                    $"Customer created successfully: {customer.Id}",
                    new Dictionary<string, object>
                    {
                        ["CustomerId"] = customer.Id,
                        ["Email"] = customer.Email
                    }
                );

                return ApiResponse<CustomerResponse>.CreateSuccess(
                    response, 
                    "Customer created successfully"
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating customer with email: {Email}", command.Email);

                await _logsService.LogErrorAsync(
                    ex,
                    $"Customer creation failed: {ex.Message}",
                    new Dictionary<string, object>
                    {
                        ["CommandId"] = command.CommandId,
                        ["Email"] = command.Email,
                        ["Error"] = ex.Message
                    }
                );

                return ApiResponse<CustomerResponse>.CreateError(
                    "An error occurred while creating customer", 
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