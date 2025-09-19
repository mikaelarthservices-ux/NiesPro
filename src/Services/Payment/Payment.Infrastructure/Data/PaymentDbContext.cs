using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Entities;
using Payment.Domain.Events;
using Payment.Infrastructure.Data.Configurations;
using System.Linq.Expressions;
using System.Reflection;

namespace Payment.Infrastructure.Data;

/// <summary>
/// Contexte Entity Framework pour le microservice Payment avec configuration enterprise
/// </summary>
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    // Entities principales
    public DbSet<Domain.Entities.Payment> Payments { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<ThreeDSecureAuthentication> ThreeDSecureAuthentications { get; set; } = null!;
    public DbSet<PaymentRefund> PaymentRefunds { get; set; } = null!;
    public DbSet<Merchant> Merchants { get; set; } = null!;

    // Event Store pour Event Sourcing
    public DbSet<StoredEvent> StoredEvents { get; set; } = null!;
    public DbSet<EventSnapshot> EventSnapshots { get; set; } = null!;

    // Tables de sécurité et audit
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<SecurityLog> SecurityLogs { get; set; } = null!;
    public DbSet<FraudAlertLog> FraudAlertLogs { get; set; } = null!;

    // Tables de configuration
    public DbSet<PaymentProcessorConfiguration> PaymentProcessorConfigurations { get; set; } = null!;
    public DbSet<MerchantConfiguration> MerchantConfigurations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appliquer toutes les configurations depuis les fichiers séparés
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new ThreeDSecureAuthenticationConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentRefundConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MerchantEntityConfiguration());

        // Event Store configurations
        modelBuilder.ApplyConfiguration(new StoredEventConfiguration());
        modelBuilder.ApplyConfiguration(new EventSnapshotConfiguration());

        // Audit et sécurité configurations
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new SecurityLogConfiguration());
        modelBuilder.ApplyConfiguration(new FraudAlertLogConfiguration());

        // Configuration système
        modelBuilder.ApplyConfiguration(new PaymentProcessorConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new MerchantConfigurationEntityConfiguration());

        // Index de performance pour les requêtes fréquentes
        ConfigurePerformanceIndexes(modelBuilder);

        // Configuration des conventions globales
        ConfigureGlobalConventions(modelBuilder);

        // Configuration de la sécurité PCI-DSS
        ConfigurePCICompliance(modelBuilder);
    }

    private static void ConfigurePerformanceIndexes(ModelBuilder modelBuilder)
    {
        // Index pour les recherches de paiements
        modelBuilder.Entity<Domain.Entities.Payment>()
            .HasIndex(p => new { p.CustomerId, p.Status, p.CreatedAt })
            .HasDatabaseName("IX_Payments_Customer_Status_Date");

        modelBuilder.Entity<Domain.Entities.Payment>()
            .HasIndex(p => new { p.MerchantId, p.Status, p.CreatedAt })
            .HasDatabaseName("IX_Payments_Merchant_Status_Date");

        modelBuilder.Entity<Domain.Entities.Payment>()
            .HasIndex(p => p.Reference)
            .IsUnique()
            .HasDatabaseName("IX_Payments_Reference_Unique");

        // Index pour les transactions
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.PaymentId, t.Type, t.Status })
            .HasDatabaseName("IX_Transactions_Payment_Type_Status");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.CustomerId, t.CreatedAt })
            .HasDatabaseName("IX_Transactions_Customer_Date");

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.ProcessorTransactionId)
            .HasDatabaseName("IX_Transactions_ProcessorId");

        // Index pour les cartes (sécurisé)
        modelBuilder.Entity<Card>()
            .HasIndex(c => new { c.CustomerId, c.IsDefault })
            .HasDatabaseName("IX_Cards_Customer_Default");

        modelBuilder.Entity<Card>()
            .HasIndex(c => c.Token)
            .IsUnique()
            .HasDatabaseName("IX_Cards_Token_Unique");

        modelBuilder.Entity<Card>()
            .HasIndex(c => new { c.Fingerprint, c.CustomerId })
            .IsUnique()
            .HasDatabaseName("IX_Cards_Fingerprint_Customer_Unique");

        // Index pour la détection de fraude
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.CustomerId, t.CreatedAt, t.FraudScore })
            .HasDatabaseName("IX_Transactions_Fraud_Analysis");

        // Index pour l'audit et la sécurité
        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityType, a.EntityId, a.Timestamp })
            .HasDatabaseName("IX_AuditLogs_Entity_Date");

        modelBuilder.Entity<SecurityLog>()
            .HasIndex(s => new { s.IpAddress, s.Timestamp })
            .HasDatabaseName("IX_SecurityLogs_IP_Date");

        modelBuilder.Entity<FraudAlertLog>()
            .HasIndex(f => new { f.CustomerId, f.FraudScore, f.Timestamp })
            .HasDatabaseName("IX_FraudAlerts_Customer_Score_Date");

        // Index pour Event Store
        modelBuilder.Entity<StoredEvent>()
            .HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique()
            .HasDatabaseName("IX_StoredEvents_Aggregate_Version_Unique");

        modelBuilder.Entity<StoredEvent>()
            .HasIndex(e => new { e.EventType, e.Timestamp })
            .HasDatabaseName("IX_StoredEvents_Type_Date");
    }

    private static void ConfigureGlobalConventions(ModelBuilder modelBuilder)
    {
        // Convention pour les clés primaires UUID
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey?.Properties.Count == 1 && 
                primaryKey.Properties[0].ClrType == typeof(Guid))
            {
                primaryKey.Properties[0].SetDefaultValueSql("NEWID()");
            }
        }

        // Convention pour les timestamps automatiques
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var createdAtProperty = entityType.FindProperty("CreatedAt");
            if (createdAtProperty != null && createdAtProperty.ClrType == typeof(DateTime))
            {
                createdAtProperty.SetDefaultValueSql("GETUTCDATE()");
            }

            var updatedAtProperty = entityType.FindProperty("UpdatedAt");
            if (updatedAtProperty != null && updatedAtProperty.ClrType == typeof(DateTime?))
            {
                // UpdatedAt sera géré par les triggers ou l'application
            }
        }

        // Convention pour les propriétés de suppression logique
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var deletedAtProperty = entityType.FindProperty("DeletedAt");
            if (deletedAtProperty != null)
            {
                // Configuration du soft delete avec expression lambda typée
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(DateTime?) }, parameter, Expression.Constant("DeletedAt"));
                var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));
                var lambda = Expression.Lambda(condition, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // Convention pour les propriétés monétaires
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.Name.Contains("Amount") && property.ClrType == typeof(decimal))
                {
                    property.SetPrecision(18);
                    property.SetScale(4);
                }
            }
        }
    }

    private static void ConfigurePCICompliance(ModelBuilder modelBuilder)
    {
        // Configuration pour la conformité PCI-DSS
        
        // Les données sensibles des cartes doivent être chiffrées
        modelBuilder.Entity<Card>(entity =>
        {
            // Jamais stocker le numéro de carte complet
            entity.Ignore(c => c.CardNumber);
            
            // Jamais stocker le CVV
            entity.Ignore(c => c.Cvv);
            
            // L'adresse de facturation doit être chiffrée
            entity.Property(c => c.EncryptedBillingAddress)
                .HasColumnName("BillingAddress_Encrypted")
                .HasMaxLength(2000);
        });

        // Configuration pour les logs de sécurité (requis pour PCI)
        modelBuilder.Entity<SecurityLog>(entity =>
        {
            entity.Property(s => s.Details)
                .HasColumnType("nvarchar(max)");
            
            entity.Property(s => s.IpAddress)
                .HasMaxLength(45); // Support IPv6
        });

        // Configuration pour les alertes de fraude
        modelBuilder.Entity<FraudAlertLog>(entity =>
        {
            entity.Property(f => f.RiskFactors)
                .HasColumnType("nvarchar(max)")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                );
        });

        // Configuration pour la rétention des données (conformité réglementaire)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.GetProperties().Any(p => p.Name == "CreatedAt"))
            {
                // Ajouter des index pour la purge automatique des anciennes données
                var tableName = entityType.GetTableName();
                
                // Politique de rétention : 7 ans pour les transactions financières
                if (tableName == "Payments" || tableName == "Transactions")
                {
                    // Configuration sera gérée via les politiques de base de données
                }
            }
        }
    }

    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInfo()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && 
                       e.Entity is not SecurityLog && 
                       e.Entity is not StoredEvent &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            // Créer des logs d'audit pour toutes les modifications
            var auditLog = new AuditLog
            {
                EntityType = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State.ToString(),
                Changes = GetChanges(entry),
                Timestamp = DateTime.UtcNow,
                UserId = GetCurrentUserId() // À implémenter selon le système d'auth
            };

            AuditLogs.Add(auditLog);

            // Mise à jour automatique des timestamps
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }

    private static string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return keyProperty?.CurrentValue?.ToString() ?? "Unknown";
    }

    private static string GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.IsModified)
            {
                changes.Add(property.Metadata.Name, new
                {
                    Original = property.OriginalValue,
                    Current = property.CurrentValue
                });
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(changes);
    }

    private static string? GetCurrentUserId()
    {
        // À implémenter : récupérer l'ID de l'utilisateur actuel depuis le contexte
        // Par exemple via HttpContextAccessor ou ClaimsPrincipal
        return "system"; // Placeholder
    }
}

/// <summary>
/// Factory pour la création du contexte lors des migrations
/// </summary>
public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        var connectionString = configuration.GetConnectionString("PaymentDatabase") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=NiesPro_Payment;Trusted_Connection=true;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableSensitiveDataLogging(false); // Sécurité PCI
        optionsBuilder.EnableDetailedErrors(false); // Performance

        return new PaymentDbContext(optionsBuilder.Options);
    }
}

// Entités pour Event Store
public class StoredEvent
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
    public string? CorrelationId { get; set; }
    public string? CausationId { get; set; }
    public string? UserId { get; set; }
}

public class EventSnapshot
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public string SnapshotData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
}

// Entités pour l'audit et la sécurité
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class SecurityLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty; // Info, Warning, Error, Critical
}

public class FraudAlertLog
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public int FraudScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsBlocked { get; set; }
    public string? ReviewNotes { get; set; }
}

// Entités de configuration
public class PaymentProcessorConfiguration
{
    public Guid Id { get; set; }
    public string ProcessorName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string ConfigurationJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}