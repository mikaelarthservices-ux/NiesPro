using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'entité Payment
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Payment> builder)
    {
        // Table et clé primaire
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        // Propriétés requises
        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.MerchantId)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.Reference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configuration Money (Value Object)
        builder.OwnsOne(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnName("Amount")
                .HasPrecision(18, 4);

            money.Property(m => m.Currency)
                .IsRequired()
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        // Propriétés de métadonnées
        builder.Property(p => p.Metadata)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                metadata => System.Text.Json.JsonSerializer.Serialize(metadata, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            );

        // URLs
        builder.Property(p => p.ReturnUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.CancelUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.WebhookUrl)
            .HasMaxLength(2000);

        // Tracking temporel
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.UpdatedAt);

        builder.Property(p => p.ExpiresAt);

        builder.Property(p => p.ConfirmedAt);

        // Propriétés de session et contexte
        builder.Property(p => p.SessionId)
            .HasMaxLength(100);

        builder.Property(p => p.IpAddress)
            .HasMaxLength(45); // Support IPv6

        builder.Property(p => p.UserAgent)
            .HasMaxLength(2000);

        // Relations
        builder.HasOne(p => p.Merchant)
            .WithMany()
            .HasForeignKey(p => p.MerchantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Transactions)
            .WithOne(t => t.Payment)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance
        builder.HasIndex(p => p.Reference)
            .IsUnique()
            .HasDatabaseName("IX_Payments_Reference");

        builder.HasIndex(p => new { p.CustomerId, p.Status })
            .HasDatabaseName("IX_Payments_Customer_Status");

        builder.HasIndex(p => new { p.MerchantId, p.CreatedAt })
            .HasDatabaseName("IX_Payments_Merchant_Date");

        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("IX_Payments_Order");

        // Configuration du soft delete
        builder.HasQueryFilter(p => p.DeletedAt == null);
        builder.Property(p => p.DeletedAt);
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité Transaction
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Table et clé primaire
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        // Propriétés requises
        builder.Property(t => t.Id)
            .IsRequired();

        builder.Property(t => t.PaymentId)
            .IsRequired();

        builder.Property(t => t.PaymentMethodId)
            .IsRequired();

        builder.Property(t => t.CustomerId)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configuration Money (Value Object)
        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnName("Amount")
                .HasPrecision(18, 4);

            money.Property(m => m.Currency)
                .IsRequired()
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(t => t.Fee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("FeeAmount")
                .HasPrecision(18, 4);

            money.Property(m => m.Currency)
                .HasColumnName("FeeCurrency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        // Identifiants externes
        builder.Property(t => t.ProcessorTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.GatewayTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.AuthorizationCode)
            .HasMaxLength(50);

        // Détails de la transaction
        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.ProcessorResponse)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.FailureReason)
            .HasMaxLength(1000);

        // Données de géolocalisation et contexte
        builder.Property(t => t.IpAddress)
            .HasMaxLength(45);

        builder.Property(t => t.GeoLocation)
            .HasMaxLength(200);

        builder.Property(t => t.DeviceFingerprint)
            .HasMaxLength(100);

        // Score de fraude
        builder.Property(t => t.FraudScore);

        // Tracking temporel
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedAt);

        builder.Property(t => t.ProcessedAt);

        // Relations
        builder.HasOne(t => t.Payment)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.PaymentMethod)
            .WithMany()
            .HasForeignKey(t => t.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index pour performance et sécurité
        builder.HasIndex(t => t.ProcessorTransactionId)
            .HasDatabaseName("IX_Transactions_ProcessorId");

        builder.HasIndex(t => new { t.CustomerId, t.CreatedAt })
            .HasDatabaseName("IX_Transactions_Customer_Date");

        builder.HasIndex(t => new { t.PaymentId, t.Type })
            .HasDatabaseName("IX_Transactions_Payment_Type");

        builder.HasIndex(t => new { t.IpAddress, t.CreatedAt })
            .HasDatabaseName("IX_Transactions_IP_Date");

        // Index pour détection de fraude
        builder.HasIndex(t => new { t.CustomerId, t.FraudScore, t.CreatedAt })
            .HasDatabaseName("IX_Transactions_Fraud");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité PaymentMethod
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        // Table et clé primaire
        builder.ToTable("PaymentMethods");
        builder.HasKey(pm => pm.Id);

        // Propriétés requises
        builder.Property(pm => pm.Id)
            .IsRequired();

        builder.Property(pm => pm.CustomerId)
            .IsRequired();

        builder.Property(pm => pm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pm => pm.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pm => pm.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pm => pm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Tracking temporel
        builder.Property(pm => pm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(pm => pm.UpdatedAt);

        builder.Property(pm => pm.LastUsedAt);

        // Index pour performance
        builder.HasIndex(pm => new { pm.CustomerId, pm.IsDefault })
            .HasDatabaseName("IX_PaymentMethods_Customer_Default");

        builder.HasIndex(pm => new { pm.CustomerId, pm.Type })
            .HasDatabaseName("IX_PaymentMethods_Customer_Type");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité Card avec conformité PCI-DSS
/// </summary>
public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        // Table et clé primaire
        builder.ToTable("Cards");
        builder.HasKey(c => c.Id);

        // Propriétés requises
        builder.Property(c => c.Id)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .IsRequired();

        builder.Property(c => c.Token)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Brand)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.LastFourDigits)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(c => c.ExpiryMonth)
            .IsRequired();

        builder.Property(c => c.ExpiryYear)
            .IsRequired();

        builder.Property(c => c.CardholderName)
            .HasMaxLength(100);

        // Données sensibles chiffrées
        builder.Property(c => c.EncryptedBillingAddress)
            .HasMaxLength(2000);

        builder.Property(c => c.Fingerprint)
            .IsRequired()
            .HasMaxLength(255);

        // État de la carte
        builder.Property(c => c.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Tracking temporel
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.LastUsedAt);

        // Relation avec PaymentMethod
        builder.HasOne<PaymentMethod>()
            .WithMany()
            .HasForeignKey("PaymentMethodId")
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour sécurité et performance
        builder.HasIndex(c => c.Token)
            .IsUnique()
            .HasDatabaseName("IX_Cards_Token");

        builder.HasIndex(c => new { c.Fingerprint, c.CustomerId })
            .IsUnique()
            .HasDatabaseName("IX_Cards_Fingerprint_Customer");

        builder.HasIndex(c => new { c.CustomerId, c.IsDefault })
            .HasDatabaseName("IX_Cards_Customer_Default");

        // Conformité PCI-DSS : exclusion des données sensibles
        builder.Ignore(c => c.CardNumber);
        builder.Ignore(c => c.Cvv);
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité ThreeDSecureAuthentication
/// </summary>
public class ThreeDSecureAuthenticationConfiguration : IEntityTypeConfiguration<ThreeDSecureAuthentication>
{
    public void Configure(EntityTypeBuilder<ThreeDSecureAuthentication> builder)
    {
        // Table et clé primaire
        builder.ToTable("ThreeDSecureAuthentications");
        builder.HasKey(tds => tds.Id);

        // Propriétés requises
        builder.Property(tds => tds.Id)
            .IsRequired();

        builder.Property(tds => tds.TransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tds => tds.CardId)
            .IsRequired();

        builder.Property(tds => tds.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(tds => tds.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Données 3D Secure
        builder.Property(tds => tds.AcsUrl)
            .HasMaxLength(2000);

        builder.Property(tds => tds.PaReq)
            .HasColumnType("nvarchar(max)");

        builder.Property(tds => tds.Md)
            .HasMaxLength(500);

        builder.Property(tds => tds.Cavv)
            .HasMaxLength(100);

        builder.Property(tds => tds.Eci)
            .HasMaxLength(10);

        builder.Property(tds => tds.Xid)
            .HasMaxLength(100);

        builder.Property(tds => tds.DirectoryServerTransactionId)
            .HasMaxLength(100);

        builder.Property(tds => tds.AuthenticationValue)
            .HasMaxLength(100);

        // Gestion des erreurs
        builder.Property(tds => tds.ErrorMessage)
            .HasMaxLength(1000);

        // Tracking temporel
        builder.Property(tds => tds.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(tds => tds.UpdatedAt);

        builder.Property(tds => tds.CompletedAt);

        // Relation avec Card
        builder.HasOne(tds => tds.Card)
            .WithMany()
            .HasForeignKey(tds => tds.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index pour performance
        builder.HasIndex(tds => tds.TransactionId)
            .IsUnique()
            .HasDatabaseName("IX_3DS_Transaction");

        builder.HasIndex(tds => new { tds.CardId, tds.Status })
            .HasDatabaseName("IX_3DS_Card_Status");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité PaymentRefund
/// </summary>
public class PaymentRefundConfiguration : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        // Table et clé primaire
        builder.ToTable("PaymentRefunds");
        builder.HasKey(r => r.Id);

        // Propriétés requises
        builder.Property(r => r.Id)
            .IsRequired();

        builder.Property(r => r.PaymentId)
            .IsRequired();

        builder.Property(r => r.Reference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Reason)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configuration Money (Value Object)
        builder.OwnsOne(r => r.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasColumnName("Amount")
                .HasPrecision(18, 4);

            money.Property(m => m.Currency)
                .IsRequired()
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        // Identifiants externes
        builder.Property(r => r.ProcessorRefundId)
            .HasMaxLength(100);

        builder.Property(r => r.ProcessorResponse)
            .HasColumnType("nvarchar(max)");

        // Tracking temporel
        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.ProcessedAt);

        // Relation avec Payment
        builder.HasOne(r => r.Payment)
            .WithMany(p => p.Refunds)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance
        builder.HasIndex(r => r.Reference)
            .IsUnique()
            .HasDatabaseName("IX_Refunds_Reference");

        builder.HasIndex(r => new { r.PaymentId, r.Status })
            .HasDatabaseName("IX_Refunds_Payment_Status");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité Merchant
/// </summary>
public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        // Table et clé primaire
        builder.ToTable("Merchants");
        builder.HasKey(m => m.Id);

        // Propriétés requises
        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Propriétés optionnelles
        builder.Property(m => m.Description)
            .HasMaxLength(1000);

        builder.Property(m => m.Website)
            .HasMaxLength(500);

        builder.Property(m => m.SupportEmail)
            .HasMaxLength(255);

        builder.Property(m => m.SupportPhone)
            .HasMaxLength(50);

        // Configuration de l'adresse (Value Object)
        builder.OwnsOne(m => m.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("AddressStreet")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .HasMaxLength(100);
        });

        // Métadonnées et configuration
        builder.Property(m => m.Metadata)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                metadata => System.Text.Json.JsonSerializer.Serialize(metadata, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            );

        // Tracking temporel
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.UpdatedAt);

        // Configuration des relations
        builder.HasMany<MerchantConfiguration>()
            .WithOne(mc => mc.Merchant)
            .HasForeignKey(mc => mc.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance
        builder.HasIndex(m => m.Code)
            .IsUnique()
            .HasDatabaseName("IX_Merchants_Code");

        builder.HasIndex(m => m.Email)
            .HasDatabaseName("IX_Merchants_Email");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_Merchants_Status");
    }
}