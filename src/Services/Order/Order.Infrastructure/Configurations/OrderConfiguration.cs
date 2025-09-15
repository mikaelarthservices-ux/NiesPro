using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("IX_Orders_CustomerId");

        // Value Object: CustomerInfo
        builder.OwnsOne(o => o.CustomerInfo, customerInfo =>
        {
            customerInfo.Property(ci => ci.FirstName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("CustomerFirstName");

            customerInfo.Property(ci => ci.LastName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("CustomerLastName");

            customerInfo.Property(ci => ci.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("CustomerEmail");

            customerInfo.Property(ci => ci.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("CustomerPhoneNumber");
        });

        // Value Object: ShippingAddress
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("ShippingStreet");

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("ShippingCity");

            address.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("ShippingPostalCode");

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("ShippingCountry");

            address.Property(a => a.AddressLine2)
                .HasMaxLength(100)
                .HasColumnName("ShippingAddressLine2");
        });

        // Value Object: BillingAddress (optionnel)
        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasMaxLength(100)
                .HasColumnName("BillingStreet");

            address.Property(a => a.City)
                .HasMaxLength(50)
                .HasColumnName("BillingCity");

            address.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("BillingPostalCode");

            address.Property(a => a.Country)
                .HasMaxLength(50)
                .HasColumnName("BillingCountry");

            address.Property(a => a.AddressLine2)
                .HasMaxLength(100)
                .HasColumnName("BillingAddressLine2");
        });

        // Enum
        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        // Value Objects: Money
        builder.OwnsOne(o => o.TaxAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("TaxAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("TaxCurrency");
        });

        builder.OwnsOne(o => o.ShippingCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("ShippingCostAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("ShippingCostCurrency");
        });

        builder.OwnsOne(o => o.DiscountAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("DiscountAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("DiscountCurrency");
        });

        // Propriétés de suivi
        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(o => o.ConfirmedAt);
        builder.Property(o => o.ShippedAt);
        builder.Property(o => o.DeliveredAt);
        builder.Property(o => o.CancelledAt);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Relations
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        // Contraintes de performance
        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        builder.HasIndex(o => new { o.CustomerId, o.Status })
            .HasDatabaseName("IX_Orders_Customer_Status");

        // Propriétés calculées ignorées
        builder.Ignore(o => o.SubTotal);
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.DomainEvents);
    }
}