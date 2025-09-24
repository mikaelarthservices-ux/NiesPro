using MediatR;

namespace Customer.Application.Commands
{
    /// <summary>
    /// Command to create a new customer
    /// </summary>
    public class CreateCustomerCommand : IRequest<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Notes { get; set; }
        public string? PreferredLanguage { get; set; }
        public string CustomerType { get; set; } = "Regular";
    }

    /// <summary>
    /// Command to update customer information
    /// </summary>
    public class UpdateCustomerCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? MobilePhone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Notes { get; set; }
        public string? PreferredLanguage { get; set; }
        public string CustomerType { get; set; } = "Regular";
    }

    /// <summary>
    /// Command to add loyalty points to a customer
    /// </summary>
    public class AddLoyaltyPointsCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public decimal Points { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to redeem loyalty points
    /// </summary>
    public class RedeemLoyaltyPointsCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public decimal Points { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to promote customer to VIP
    /// </summary>
    public class PromoteToVipCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
    }

    /// <summary>
    /// Command to deactivate a customer
    /// </summary>
    public class DeactivateCustomerCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Command to activate a customer
    /// </summary>
    public class ActivateCustomerCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
    }

    /// <summary>
    /// Command to record a customer order
    /// </summary>
    public class RecordCustomerOrderCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public Guid OrderId { get; set; }
        public decimal OrderAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }

    /// <summary>
    /// Command to add customer address
    /// </summary>
    public class AddCustomerAddressCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>
    /// Command to update customer address
    /// </summary>
    public class UpdateCustomerAddressCommand : IRequest<bool>
    {
        public Guid AddressId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Label { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>
    /// Command to delete customer address
    /// </summary>
    public class DeleteCustomerAddressCommand : IRequest<bool>
    {
        public Guid AddressId { get; set; }
    }
}