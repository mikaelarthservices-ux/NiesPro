using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Payment.Domain.Entities;
using System.Text.Json;

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
            )
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)
            ));

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

        // Ignorer les propriétés calculées
        builder.Ignore(p => p.PaymentNumber);
        builder.Ignore(p => p.ProcessingFees);
        builder.Ignore(p => p.FeeMode);
        builder.Ignore(p => p.MinimumPartialAmount);
        builder.Ignore(p => p.LastPaymentMethod);
        builder.Ignore(p => p.Transactions);
        builder.Ignore(p => p.SessionData);
        builder.Ignore(p => p.AllowPartialPayments);

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

        builder.OwnsOne(t => t.Fees, money =>
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

        // Ignorer la propriété calculée Fee
        builder.Ignore(t => t.Fee);
        
        // Ignorer la propriété calculée IpAddress (alias vers ClientIpAddress)
        builder.Ignore(t => t.IpAddress);

        // Ignorer la propriété calculée DomainEvents
        builder.Ignore(t => t.DomainEvents);

        // Identifiants externes
        builder.Property(t => t.ProcessorTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.GatewayTransactionId)
            .HasMaxLength(100);

        builder.Property(t => t.AuthorizationCode)
            .HasMaxLength(50);

        builder.Property(t => t.ExternalReference)
            .HasMaxLength(100);

        builder.Property(t => t.TransactionNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Propriétés de refus et erreur
        builder.Property(t => t.DeclineReason)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.ErrorMessage)
            .HasMaxLength(1000);

        // Propriétés de contexte client
        builder.Property(t => t.ClientIpAddress)
            .HasMaxLength(45);

        builder.Property(t => t.ClientUserAgent)
            .HasMaxLength(2000);

        // Propriétés de conversion de devise
        builder.Property(t => t.ExchangeRate)
            .HasPrecision(18, 8);

        builder.Property(t => t.OriginalCurrency)
            .HasMaxLength(3);

        // Propriétés temporelles supplémentaires
        builder.Property(t => t.AuthorizationExpiresAt);

        // Détails de la transaction
        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.ProcessorResponse)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.FailureReason)
            .HasMaxLength(1000);

        // Données de géolocalisation et contexte
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

        // Configuration de la propriété Metadata (Dictionary<string, string>)
        builder.Property(t => t.Metadata)
            .HasConversion(
                metadata => JsonSerializer.Serialize(metadata, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            )
            .HasColumnName("Metadata")
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)
            ));

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

        builder.HasIndex(t => new { t.ClientIpAddress, t.CreatedAt })
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

        builder.Property(pm => pm.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        // Ignorer la propriété calculée Name
        builder.Ignore(pm => pm.Name);

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

        // Configuration du Value Object CreditCard
        builder.OwnsOne(pm => pm.CreditCard, creditCard =>
        {
            creditCard.Property(cc => cc.MaskedNumber)
                .HasColumnName("CreditCard_MaskedNumber")
                .HasMaxLength(20);

            creditCard.Property(cc => cc.Last4Digits)
                .HasColumnName("CreditCard_Last4Digits")
                .HasMaxLength(4);

            creditCard.Property(cc => cc.CardholderName)
                .HasColumnName("CreditCard_CardholderName")
                .HasMaxLength(100);

            creditCard.Property(cc => cc.ExpiryMonth)
                .HasColumnName("CreditCard_ExpiryMonth");

            creditCard.Property(cc => cc.ExpiryYear)
                .HasColumnName("CreditCard_ExpiryYear");

            creditCard.Property(cc => cc.CardType)
                .HasColumnName("CreditCard_CardType")
                .HasMaxLength(20);

            creditCard.Property(cc => cc.Brand)
                .HasColumnName("CreditCard_Brand")
                .HasConversion<string>()
                .HasMaxLength(20);

            creditCard.Property(cc => cc.Token)
                .HasColumnName("CreditCard_Token")
                .HasMaxLength(100);
        });

        // Configuration des limites Money (Value Objects optionnels)
        builder.OwnsOne(pm => pm.DailyLimit, dailyLimit =>
        {
            dailyLimit.Property(dl => dl.Amount)
                .HasColumnName("DailyLimit_Amount")
                .HasPrecision(18, 4);

            dailyLimit.Property(dl => dl.Currency)
                .HasColumnName("DailyLimit_Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        builder.OwnsOne(pm => pm.TransactionLimit, transactionLimit =>
        {
            transactionLimit.Property(tl => tl.Amount)
                .HasColumnName("TransactionLimit_Amount")
                .HasPrecision(18, 4);

            transactionLimit.Property(tl => tl.Currency)
                .HasColumnName("TransactionLimit_Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Domain.ValueObjects.Currency.FromCode(code))
                .HasMaxLength(3);
        });

        // Configuration de la propriété Metadata (Dictionary<string, string>)
        builder.Property(pm => pm.Metadata)
            .HasConversion(
                metadata => System.Text.Json.JsonSerializer.Serialize(metadata, (JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            )
            .HasColumnName("Metadata")
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)
            ));

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

        builder.Property(c => c.Last4Digits)
            .IsRequired()
            .HasMaxLength(4);

        // Ignorer la propriété calculée LastFourDigits (alias vers Last4Digits)
        builder.Ignore(c => c.LastFourDigits);

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

        builder.Property(r => r.RefundNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Ignorer la propriété calculée Reference
        builder.Ignore(r => r.Reference);

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
        builder.HasIndex(r => r.RefundNumber)
            .IsUnique()
            .HasDatabaseName("IX_Refunds_RefundNumber");

        builder.HasIndex(r => new { r.PaymentId, r.Status })
            .HasDatabaseName("IX_Refunds_Payment_Status");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité Merchant
/// </summary>
public class MerchantEntityConfiguration : IEntityTypeConfiguration<Merchant>
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

        // Configuration de l'adresse (Value Object)
        // Configuration de l'adresse comme propriété simple
        builder.Property(m => m.Address)
            .HasColumnName("Address")
            .HasMaxLength(500);

        // Métadonnées et configuration
        builder.Property(m => m.Metadata)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                metadata => System.Text.Json.JsonSerializer.Serialize(metadata, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            )
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)
            ));

        // Tracking temporel
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.UpdatedAt);

        // Configuration des relations
        builder.HasMany<Payment.Domain.Entities.MerchantConfiguration>()
            .WithOne(mc => mc.Merchant)
            .HasForeignKey(mc => mc.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance
        builder.HasIndex(m => m.Email)
            .HasDatabaseName("IX_Merchants_Email");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_Merchants_Status");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité MerchantConfiguration
/// </summary>
public class MerchantConfigurationEntityConfiguration : IEntityTypeConfiguration<MerchantConfiguration>
{
    public void Configure(EntityTypeBuilder<MerchantConfiguration> builder)
    {
        // Table et clé primaire
        builder.ToTable("MerchantConfigurations");
        builder.HasKey(mc => mc.Id);

        // Propriétés requises
        builder.Property(mc => mc.Id)
            .IsRequired();

        builder.Property(mc => mc.MerchantId)
            .IsRequired();

        builder.Property(mc => mc.ConfigurationKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(mc => mc.ConfigurationValue)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(mc => mc.EncryptedValue)
            .HasMaxLength(2000);

        builder.Property(mc => mc.IsEncrypted)
            .IsRequired()
            .HasDefaultValue(false);

        // Tracking temporel
        builder.Property(mc => mc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(mc => mc.UpdatedAt);

        // Relation avec Merchant
        builder.HasOne<Merchant>()
            .WithMany()
            .HasForeignKey(mc => mc.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance
        builder.HasIndex(mc => new { mc.MerchantId, mc.ConfigurationKey })
            .IsUnique()
            .HasDatabaseName("IX_MerchantConfigurations_Merchant_Key");

        builder.HasIndex(mc => mc.MerchantId)
            .HasDatabaseName("IX_MerchantConfigurations_Merchant");
    }
}