using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'Event Store
/// </summary>
public class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        // Table et clé primaire
        builder.ToTable("StoredEvents");
        builder.HasKey(e => e.Id);

        // Propriétés requises
        builder.Property(e => e.Id)
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Propriétés optionnelles pour le tracing
        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        builder.Property(e => e.CausationId)
            .HasMaxLength(100);

        builder.Property(e => e.UserId)
            .HasMaxLength(100);

        // Index pour performance Event Sourcing
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique()
            .HasDatabaseName("IX_StoredEvents_Aggregate_Version");

        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_StoredEvents_Timestamp");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_StoredEvents_EventType");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("IX_StoredEvents_CorrelationId");
    }
}

/// <summary>
/// Configuration Entity Framework pour les snapshots d'événements
/// </summary>
public class EventSnapshotConfiguration : IEntityTypeConfiguration<EventSnapshot>
{
    public void Configure(EntityTypeBuilder<EventSnapshot> builder)
    {
        // Table et clé primaire
        builder.ToTable("EventSnapshots");
        builder.HasKey(s => s.Id);

        // Propriétés requises
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.AggregateId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.AggregateType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.SnapshotData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Version)
            .IsRequired();

        builder.Property(s => s.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index pour performance
        builder.HasIndex(s => new { s.AggregateId, s.Version })
            .HasDatabaseName("IX_EventSnapshots_Aggregate_Version");

        builder.HasIndex(s => s.AggregateType)
            .HasDatabaseName("IX_EventSnapshots_AggregateType");
    }
}

/// <summary>
/// Configuration Entity Framework pour les logs d'audit
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table et clé primaire
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        // Propriétés requises
        builder.Property(a => a.Id)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Changes)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Propriétés optionnelles pour le contexte
        builder.Property(a => a.UserId)
            .HasMaxLength(100);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // Support IPv6

        builder.Property(a => a.UserAgent)
            .HasMaxLength(2000);

        // Index pour recherche et performance
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_AuditLogs_Entity");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(a => new { a.EntityType, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_EntityType_Date");
    }
}

/// <summary>
/// Configuration Entity Framework pour les logs de sécurité
/// </summary>
public class SecurityLogConfiguration : IEntityTypeConfiguration<SecurityLog>
{
    public void Configure(EntityTypeBuilder<SecurityLog> builder)
    {
        // Table et clé primaire
        builder.ToTable("SecurityLogs");
        builder.HasKey(s => s.Id);

        // Propriétés requises
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Details)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // Support IPv6

        builder.Property(s => s.Severity)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Propriétés optionnelles
        builder.Property(s => s.UserId)
            .HasMaxLength(100);

        // Index pour surveillance sécurité
        builder.HasIndex(s => new { s.IpAddress, s.Timestamp })
            .HasDatabaseName("IX_SecurityLogs_IP_Date");

        builder.HasIndex(s => new { s.EventType, s.Severity })
            .HasDatabaseName("IX_SecurityLogs_EventType_Severity");

        builder.HasIndex(s => s.Timestamp)
            .HasDatabaseName("IX_SecurityLogs_Timestamp");

        builder.HasIndex(s => new { s.UserId, s.Timestamp })
            .HasDatabaseName("IX_SecurityLogs_User_Date");
    }
}

/// <summary>
/// Configuration Entity Framework pour les alertes de fraude
/// </summary>
public class FraudAlertLogConfiguration : IEntityTypeConfiguration<FraudAlertLog>
{
    public void Configure(EntityTypeBuilder<FraudAlertLog> builder)
    {
        // Table et clé primaire
        builder.ToTable("FraudAlertLogs");
        builder.HasKey(f => f.Id);

        // Propriétés requises
        builder.Property(f => f.Id)
            .IsRequired();

        builder.Property(f => f.CustomerId)
            .IsRequired();

        builder.Property(f => f.TransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FraudScore)
            .IsRequired();

        builder.Property(f => f.RiskLevel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(f => f.RiskFactors)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(f => f.Recommendation)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(f => f.IsBlocked)
            .IsRequired()
            .HasDefaultValue(false);

        // Propriétés optionnelles
        builder.Property(f => f.ReviewNotes)
            .HasMaxLength(2000);

        // Index pour analyse de fraude
        builder.HasIndex(f => new { f.CustomerId, f.FraudScore })
            .HasDatabaseName("IX_FraudAlerts_Customer_Score");

        builder.HasIndex(f => new { f.RiskLevel, f.Timestamp })
            .HasDatabaseName("IX_FraudAlerts_RiskLevel_Date");

        builder.HasIndex(f => f.TransactionId)
            .HasDatabaseName("IX_FraudAlerts_Transaction");

        builder.HasIndex(f => new { f.IsBlocked, f.Timestamp })
            .HasDatabaseName("IX_FraudAlerts_Blocked_Date");
    }
}

/// <summary>
/// Configuration Entity Framework pour la configuration des processeurs de paiement
/// </summary>
public class PaymentProcessorConfigurationConfiguration : IEntityTypeConfiguration<PaymentProcessorConfiguration>
{
    public void Configure(EntityTypeBuilder<PaymentProcessorConfiguration> builder)
    {
        // Table et clé primaire
        builder.ToTable("PaymentProcessorConfigurations");
        builder.HasKey(p => p.Id);

        // Propriétés requises
        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.ProcessorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.ConfigurationJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.UpdatedAt);

        // Index pour performance
        builder.HasIndex(p => p.ProcessorName)
            .IsUnique()
            .HasDatabaseName("IX_PaymentProcessorConfig_Name");

        builder.HasIndex(p => p.IsEnabled)
            .HasDatabaseName("IX_PaymentProcessorConfig_Enabled");
    }
}

/// <summary>
/// Configuration Entity Framework pour la configuration des marchands
/// </summary>
public class MerchantConfigurationConfiguration : IEntityTypeConfiguration<MerchantConfiguration>
{
    public void Configure(EntityTypeBuilder<MerchantConfiguration> builder)
    {
        // Table et clé primaire
        builder.ToTable("MerchantConfigurations");
        builder.HasKey(m => m.Id);

        // Propriétés requises
        builder.Property(m => m.Id)
            .IsRequired();

        builder.Property(m => m.MerchantId)
            .IsRequired();

        builder.Property(m => m.ConfigurationKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ConfigurationValue)
            .HasMaxLength(2000);

        builder.Property(m => m.EncryptedValue)
            .HasMaxLength(4000);

        builder.Property(m => m.IsEncrypted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(m => m.UpdatedAt);

        // Relation avec Merchant
        builder.HasOne(m => m.Merchant)
            .WithMany()
            .HasForeignKey(m => m.MerchantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index pour performance et sécurité
        builder.HasIndex(m => new { m.MerchantId, m.ConfigurationKey })
            .IsUnique()
            .HasDatabaseName("IX_MerchantConfig_Merchant_Key");

        builder.HasIndex(m => m.ConfigurationKey)
            .HasDatabaseName("IX_MerchantConfig_Key");

        builder.HasIndex(m => m.IsEncrypted)
            .HasDatabaseName("IX_MerchantConfig_Encrypted");
    }
}