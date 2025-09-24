using NiesPro.Contracts.Primitives;

namespace Customer.Domain.Events
{
    /// <summary>
    /// Event fired when a new customer is created
    /// </summary>
    public class CustomerCreatedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when customer information is updated
    /// </summary>
    public class CustomerUpdatedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Dictionary<string, object> ChangedFields { get; set; } = new();
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when a customer is promoted to VIP status
    /// </summary>
    public class CustomerPromotedToVipEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when loyalty points are added
    /// </summary>
    public class LoyaltyPointsAddedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public decimal PointsAdded { get; set; }
        public decimal TotalPoints { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when loyalty points are redeemed
    /// </summary>
    public class LoyaltyPointsRedeemedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public decimal PointsRedeemed { get; set; }
        public decimal RemainingPoints { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when customer records an order
    /// </summary>
    public class CustomerOrderRecordedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public Guid OrderId { get; set; }
        public decimal OrderAmount { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when customer is deactivated
    /// </summary>
    public class CustomerDeactivatedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event fired when customer address is added
    /// </summary>
    public class CustomerAddressAddedEvent : IDomainEvent
    {
        public Guid CustomerId { get; set; }
        public Guid AddressId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    }
}