using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Entities;

namespace Order.Infrastructure.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        
        builder.HasKey(oi => oi.Id);
        
        builder.Property(oi => oi.Id)
            .ValueGeneratedNever();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.ProductSku)
            .HasMaxLength(50);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // Value Object: UnitPrice (Money)
        builder.OwnsOne(oi => oi.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("UnitPriceAmount")
                .IsRequired();

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("UnitPriceCurrency");
        });

        // Propriétés de suivi
        builder.Property(oi => oi.AddedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.Property(oi => oi.LastModifiedAt);

        // Index pour performance
        builder.HasIndex(oi => oi.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");

        builder.HasIndex(oi => oi.AddedAt)
            .HasDatabaseName("IX_OrderItems_AddedAt");

        // Propriétés calculées ignorées
        builder.Ignore(oi => oi.TotalPrice);

        // TODO: Contraintes Check non supportées sur Value Objects MySQL
        // builder.ToTable(t => 
        // {
        //     t.HasCheckConstraint("CK_OrderItems_Quantity", "Quantity > 0");
        //     t.HasCheckConstraint("CK_OrderItems_UnitPrice", "UnitPriceAmount > 0");
        // });
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // Enums
        builder.Property(p => p.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        // Value Objects: Money
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Amount")
                .IsRequired();

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("Currency");
        });

        builder.OwnsOne(p => p.RefundedAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("RefundedAmount")
                .IsRequired();

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("RefundedCurrency");
        });

        // Propriétés optionnelles
        builder.Property(p => p.TransactionId)
            .HasMaxLength(100);

        builder.Property(p => p.ProviderReference)
            .HasMaxLength(100);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        // Propriétés de suivi
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.Property(p => p.ProcessedAt);
        builder.Property(p => p.FailedAt);

        // Index
        builder.HasIndex(p => p.TransactionId)
            .IsUnique()
            .HasDatabaseName("IX_Payments_TransactionId")
            .HasFilter("TransactionId IS NOT NULL");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payments_Status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Payments_CreatedAt");

        // TODO: Contraintes Check non supportées sur Value Objects MySQL
        // builder.ToTable(t => 
        // {
        //     t.HasCheckConstraint("CK_Payments_Amount", "Amount > 0");
        //     t.HasCheckConstraint("CK_Payments_RefundedAmount", "RefundedAmount >= 0");
        // });
    }
}