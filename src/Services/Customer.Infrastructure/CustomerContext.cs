using Microsoft.EntityFrameworkCore;
using Customer.Domain.Aggregates.CustomerAggregate;
using NiesPro.Contracts.Infrastructure;

namespace Customer.Infrastructure
{
    /// <summary>
    /// Customer database context
    /// </summary>
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
        {
        }

        public DbSet<Customer.Domain.Aggregates.CustomerAggregate.Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerPreference> CustomerPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships with explicit foreign key mapping
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(a => a.Id);
                
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_CustomerAddress_Customer");
                      
                entity.Property(a => a.CustomerId)
                      .IsRequired();
            });

            modelBuilder.Entity<CustomerPreference>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.HasOne(p => p.Customer)
                      .WithMany(c => c.Preferences)
                      .HasForeignKey(p => p.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_CustomerPreference_Customer");
                      
                entity.Property(p => p.CustomerId)
                      .IsRequired();
            });

            // Configure entity properties to limit key length
            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>(entity =>
            {
                entity.Property(c => c.Email)
                      .HasMaxLength(255)
                      .IsRequired();
                      
                entity.Property(c => c.FirstName)
                      .HasMaxLength(100)
                      .IsRequired();
                      
                entity.Property(c => c.LastName)
                      .HasMaxLength(100)
                      .IsRequired();
                      
                entity.Property(c => c.Gender)
                      .HasMaxLength(50);
                      
                entity.Property(c => c.CustomerType)
                      .HasMaxLength(50);
            });

            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.Property(a => a.Type)
                      .HasMaxLength(50)
                      .IsRequired();
                      
                entity.Property(a => a.Label)
                      .HasMaxLength(100);
                      
                entity.Property(a => a.AddressLine1)
                      .HasMaxLength(200)
                      .IsRequired();
                      
                entity.Property(a => a.AddressLine2)
                      .HasMaxLength(200);
                      
                entity.Property(a => a.City)
                      .HasMaxLength(100)
                      .IsRequired();
                      
                entity.Property(a => a.State)
                      .HasMaxLength(100);
                      
                entity.Property(a => a.Country)
                      .HasMaxLength(100)
                      .IsRequired();
                      
                entity.Property(a => a.PostalCode)
                      .HasMaxLength(20);
            });

            modelBuilder.Entity<CustomerPreference>(entity =>
            {
                entity.Property(p => p.PreferenceKey)
                      .HasMaxLength(100)
                      .IsRequired();
                      
                entity.Property(p => p.Category)
                      .HasMaxLength(100);
                      
                entity.Property(p => p.PreferenceValue)
                      .HasMaxLength(500);
            });

            // Ensure only one default address per customer
            modelBuilder.Entity<CustomerAddress>()
                .HasIndex(a => a.CustomerId)
                .HasFilter("`IsDefault` = 1")
                .IsUnique();

            // Ensure email uniqueness across active customers
            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .HasIndex(c => c.Email)
                .HasFilter("`IsActive` = 1")
                .IsUnique();

            // Add check constraints
            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .ToTable(t => t.HasCheckConstraint("CK_Customer_LoyaltyPoints", "`LoyaltyPoints` >= 0"));

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .ToTable(t => t.HasCheckConstraint("CK_Customer_TotalSpent", "`TotalSpent` >= 0"));

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .ToTable(t => t.HasCheckConstraint("CK_Customer_TotalOrders", "`TotalOrders` >= 0"));

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .ToTable(t => t.HasCheckConstraint("CK_Customer_Gender", "`Gender` IN ('Male', 'Female', 'Other', 'Prefer not to say') OR `Gender` IS NULL"));

            modelBuilder.Entity<CustomerAddress>()
                .ToTable(t => t.HasCheckConstraint("CK_CustomerAddress_Type", "`Type` IN ('Home', 'Work', 'Billing', 'Shipping', 'Other')"));

            // Set default values
            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .Property(c => c.IsVip)
                .HasDefaultValue(false);

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .Property(c => c.LoyaltyPoints)
                .HasDefaultValue(0);

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .Property(c => c.TotalSpent)
                .HasDefaultValue(0m);

            modelBuilder.Entity<Customer.Domain.Aggregates.CustomerAggregate.Customer>()
                .Property(c => c.TotalOrders)
                .HasDefaultValue(0);

            modelBuilder.Entity<CustomerAddress>()
                .Property(a => a.IsDefault)
                .HasDefaultValue(false);

            modelBuilder.Entity<CustomerAddress>()
                .Property(a => a.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<CustomerPreference>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}