using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Customer.Domain.Aggregates.CustomerAggregate;

namespace Customer.Infrastructure.EntityConfigurations
{
    /// <summary>
    /// Entity configuration for Customer entity
    /// </summary>
    public class CustomerEntityConfiguration : IEntityTypeConfiguration<Customer.Domain.Aggregates.CustomerAggregate.Customer>
    {
        public void Configure(EntityTypeBuilder<Customer.Domain.Aggregates.CustomerAggregate.Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(c => c.Email)
                .IsUnique();

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.MobilePhone)
                .HasMaxLength(20);

            builder.Property(c => c.Gender)
                .HasMaxLength(10);

            builder.Property(c => c.Notes)
                .HasMaxLength(500);

            builder.Property(c => c.PreferredLanguage)
                .HasMaxLength(50);

            builder.Property(c => c.CustomerType)
                .HasMaxLength(50)
                .HasDefaultValue("Regular");

            builder.Property(c => c.LoyaltyPoints)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(c => c.TotalSpent)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(c => c.TotalOrders)
                .HasDefaultValue(0);

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Property(c => c.IsVip)
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt);

            // Relationships
            builder.HasMany(c => c.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Preferences)
                .WithOne(p => p.Customer)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => new { c.LastName, c.FirstName });
            builder.HasIndex(c => c.Phone);
            builder.HasIndex(c => c.IsVip);
            builder.HasIndex(c => c.CustomerType);
            builder.HasIndex(c => c.CreatedAt);
        }
    }

    /// <summary>
    /// Entity configuration for CustomerAddress entity
    /// </summary>
    public class CustomerAddressEntityConfiguration : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ToTable("CustomerAddresses");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.CustomerId)
                .IsRequired();

            builder.Property(a => a.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Label)
                .HasMaxLength(100);

            builder.Property(a => a.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.AddressLine2)
                .HasMaxLength(200);

            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.State)
                .HasMaxLength(100);

            builder.Property(a => a.PostalCode)
                .HasMaxLength(20);

            builder.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.IsDefault)
                .HasDefaultValue(false);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.Property(a => a.UpdatedAt);

            // Indexes
            builder.HasIndex(a => a.CustomerId);
            builder.HasIndex(a => new { a.CustomerId, a.IsDefault });
            builder.HasIndex(a => a.Type);
        }
    }

    /// <summary>
    /// Entity configuration for CustomerPreference entity
    /// </summary>
    public class CustomerPreferenceEntityConfiguration : IEntityTypeConfiguration<CustomerPreference>
    {
        public void Configure(EntityTypeBuilder<CustomerPreference> builder)
        {
            builder.ToTable("CustomerPreferences");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.CustomerId)
                .IsRequired();

            builder.Property(p => p.PreferenceKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.PreferenceValue)
                .HasMaxLength(500);

            builder.Property(p => p.Category)
                .HasMaxLength(50);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.UpdatedAt);

            // Indexes
            builder.HasIndex(p => p.CustomerId);
            builder.HasIndex(p => new { p.CustomerId, p.PreferenceKey });
            builder.HasIndex(p => p.Category);

            // Unique constraint to prevent duplicate preference keys per customer
            builder.HasIndex(p => new { p.CustomerId, p.PreferenceKey })
                .IsUnique();
        }
    }
}