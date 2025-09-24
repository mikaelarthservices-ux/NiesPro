using System.ComponentModel.DataAnnotations;
using NiesPro.Contracts.Infrastructure;

namespace Customer.Domain.Aggregates.CustomerAggregate
{
    /// <summary>
    /// Customer entity - Core entity representing a customer in the system
    /// </summary>
    public class Customer : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(20)]
        public string? MobilePhone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVip { get; set; } = false;

        public decimal LoyaltyPoints { get; set; } = 0;

        public DateTime? LastVisit { get; set; }

        public int TotalOrders { get; set; } = 0;

        public decimal TotalSpent { get; set; } = 0;

        [StringLength(50)]
        public string? PreferredLanguage { get; set; }

        [StringLength(50)]
        public string? CustomerType { get; set; } = "Regular";

        // Navigation properties
        public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public virtual ICollection<CustomerPreference> Preferences { get; set; } = new List<CustomerPreference>();

        // Computed properties
        public string FullName => $"{FirstName} {LastName}";

        // Domain methods
        public void AddLoyaltyPoints(decimal points)
        {
            LoyaltyPoints += points;
        }

        public void RedeemLoyaltyPoints(decimal points)
        {
            if (LoyaltyPoints >= points)
            {
                LoyaltyPoints -= points;
            }
            else
            {
                throw new InvalidOperationException("Insufficient loyalty points");
            }
        }

        public void RecordVisit()
        {
            LastVisit = DateTime.UtcNow;
        }

        public void RecordOrder(decimal orderAmount)
        {
            TotalOrders++;
            TotalSpent += orderAmount;
            RecordVisit();
        }

        public void PromoteToVip()
        {
            IsVip = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Customer address entity for multiple addresses per customer
    /// </summary>
    public class CustomerAddress : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Home, Work, Billing, Shipping

        [StringLength(100)]
        public string? Label { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Customer Customer { get; set; } = null!;
    }

    /// <summary>
    /// Customer preferences entity for storing customer preferences
    /// </summary>
    public class CustomerPreference : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string PreferenceKey { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PreferenceValue { get; set; }

        [StringLength(50)]
        public string? Category { get; set; } // Dietary, Communication, Service, etc.

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Customer Customer { get; set; } = null!;
    }
}