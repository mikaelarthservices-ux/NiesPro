using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.ValueObjects;
using NiesPro.Services.Customer.Domain.Events;
using NiesPro.Services.Customer.Infrastructure.Data.Configurations;
using System.Reflection;

namespace NiesPro.Services.Customer.Infrastructure.Data;

/// <summary>
/// DbContext principal pour le service Customer avec configuration avancée
/// Support : CQRS, Domain Events, Audit Trail, Performance optimisée
/// </summary>
public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    // ===== ENTITIES =====
    public DbSet<Domain.Entities.Customer> Customers { get; set; } = null!;
    public DbSet<CustomerProfile> CustomerProfiles { get; set; } = null!;
    public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; } = null!;
    public DbSet<LoyaltyReward> LoyaltyRewards { get; set; } = null!;
    public DbSet<CustomerSegment> CustomerSegments { get; set; } = null!;
    public DbSet<CustomerInteraction> CustomerInteractions { get; set; } = null!;
    public DbSet<CustomerPreference> CustomerPreferences { get; set; } = null!;

    // ===== AUDIT ET EVENTS =====
    public DbSet<DomainEventOutbox> DomainEvents { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Configuration avancée du modèle avec patterns Enterprise
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== SCHEMA CONFIGURATION =====
        modelBuilder.HasDefaultSchema("customer");

        // ===== AUTO-DISCOVERY DES CONFIGURATIONS =====
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ===== GLOBAL FILTERS =====
        ConfigureGlobalQueryFilters(modelBuilder);

        // ===== VALUE CONVERTERS =====
        ConfigureValueConverters(modelBuilder);

        // ===== SHADOW PROPERTIES =====
        ConfigureShadowProperties(modelBuilder);

        // ===== INDEXES ET CONTRAINTES =====
        ConfigureIndexesAndConstraints(modelBuilder);

        // ===== STORED PROCEDURES =====
        ConfigureStoredProcedures(modelBuilder);
    }

    /// <summary>
    /// Configuration des filtres globaux pour soft delete et tenant isolation
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Soft Delete pour Customer
        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasQueryFilter(c => !c.IsDeleted);

        // Filtres pour les autres entités si nécessaire
        modelBuilder.Entity<CustomerProfile>()
            .HasQueryFilter(cp => !EF.Property<bool>(cp, "IsDeleted"));

        modelBuilder.Entity<CustomerSegment>()
            .HasQueryFilter(cs => cs.IsActive);
    }

    /// <summary>
    /// Configuration des Value Converters pour les Value Objects
    /// </summary>
    private void ConfigureValueConverters(ModelBuilder modelBuilder)
    {
        // PersonalInfo Value Object
        modelBuilder.Entity<Domain.Entities.Customer>()
            .OwnsOne(c => c.PersonalInfo, pi =>
            {
                pi.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
                pi.Property(p => p.LastName).HasMaxLength(100).IsRequired();
                pi.Property(p => p.DateOfBirth).HasColumnType("date");
                pi.Property(p => p.Gender)
                  .HasConversion<string>()
                  .HasMaxLength(20);
                pi.HasIndex(p => new { p.FirstName, p.LastName })
                  .HasDatabaseName("IX_Customer_PersonalInfo_Names");
            });

        // ContactInfo Value Object
        modelBuilder.Entity<Domain.Entities.Customer>()
            .OwnsOne(c => c.ContactInfo, ci =>
            {
                ci.Property(c => c.Email).HasMaxLength(255).IsRequired();
                ci.Property(c => c.Phone).HasMaxLength(20);
                ci.Property(c => c.Address).HasMaxLength(500);
                ci.Property(c => c.City).HasMaxLength(100);
                ci.Property(c => c.PostalCode).HasMaxLength(20);
                ci.Property(c => c.Country).HasMaxLength(100);
                ci.HasIndex(c => c.Email)
                  .IsUnique()
                  .HasDatabaseName("IX_Customer_ContactInfo_Email_Unique");
                ci.HasIndex(c => c.Phone)
                  .HasDatabaseName("IX_Customer_ContactInfo_Phone");
            });

        // CommunicationSettings Value Object
        modelBuilder.Entity<CustomerProfile>()
            .OwnsOne(cp => cp.CommunicationSettings, cs =>
            {
                cs.Property(c => c.PreferredChannel)
                  .HasConversion<string>()
                  .HasMaxLength(20);
                cs.Property(c => c.EmailNotifications).HasDefaultValue(true);
                cs.Property(c => c.SmsNotifications).HasDefaultValue(false);
                cs.Property(c => c.PushNotifications).HasDefaultValue(true);
                cs.Property(c => c.NewsletterSubscription).HasDefaultValue(false);
                cs.Property(c => c.PromotionalOffers).HasDefaultValue(true);
            });

        // LoyaltyStats Value Object
        modelBuilder.Entity<Domain.Entities.Customer>()
            .OwnsOne(c => c.LoyaltyStats, ls =>
            {
                ls.Property(l => l.TotalPoints).HasDefaultValue(0);
                ls.Property(l => l.LifetimePoints).HasDefaultValue(0);
                ls.Property(l => l.CurrentTier)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue("Bronze");
                ls.Property(l => l.PointsToNextTier).HasDefaultValue(0);
                ls.Property(l => l.TierExpiryDate).HasColumnType("date");
                ls.HasIndex(l => l.CurrentTier)
                  .HasDatabaseName("IX_Customer_LoyaltyStats_CurrentTier");
                ls.HasIndex(l => l.TotalPoints)
                  .HasDatabaseName("IX_Customer_LoyaltyStats_TotalPoints");
            });
    }

    /// <summary>
    /// Configuration des Shadow Properties pour audit et performance
    /// </summary>
    private void ConfigureShadowProperties(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Audit automatique
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("UpdatedAt")
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAddOrUpdate();

                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("CreatedBy")
                    .HasMaxLength(255);

                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("UpdatedBy")
                    .HasMaxLength(255);
            }

            // Soft Delete
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<bool>("IsDeleted")
                    .HasDefaultValue(false);

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime?>("DeletedAt");

                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("DeletedBy")
                    .HasMaxLength(255);
            }

            // Versioning optimiste
            if (typeof(IVersionedEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<byte[]>("RowVersion")
                    .IsRowVersion()
                    .ValueGeneratedOnAddOrUpdate();
            }
        }
    }

    /// <summary>
    /// Configuration des index et contraintes pour performance optimale
    /// </summary>
    private void ConfigureIndexesAndConstraints(ModelBuilder modelBuilder)
    {
        // ===== CUSTOMER INDEXES =====
        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => c.Status)
            .HasDatabaseName("IX_Customer_Status");

        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => c.RegistrationDate)
            .HasDatabaseName("IX_Customer_RegistrationDate");

        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => c.LastLoginDate)
            .HasDatabaseName("IX_Customer_LastLoginDate");

        // ===== LOYALTY INDEXES =====
        modelBuilder.Entity<LoyaltyProgram>()
            .HasIndex(lp => new { lp.ProgramType, lp.IsActive })
            .HasDatabaseName("IX_LoyaltyProgram_Type_Active");

        modelBuilder.Entity<LoyaltyReward>()
            .HasIndex(lr => new { lr.ProgramId, lr.IsActive })
            .HasDatabaseName("IX_LoyaltyReward_Program_Active");

        // ===== INTERACTION INDEXES =====
        modelBuilder.Entity<CustomerInteraction>()
            .HasIndex(ci => new { ci.CustomerId, ci.InteractionDate })
            .HasDatabaseName("IX_CustomerInteraction_Customer_Date");

        modelBuilder.Entity<CustomerInteraction>()
            .HasIndex(ci => ci.InteractionType)
            .HasDatabaseName("IX_CustomerInteraction_Type");

        // ===== SEGMENT INDEXES =====
        modelBuilder.Entity<CustomerSegment>()
            .HasIndex(cs => new { cs.SegmentType, cs.IsActive })
            .HasDatabaseName("IX_CustomerSegment_Type_Active");

        // ===== PREFERENCE INDEXES =====
        modelBuilder.Entity<CustomerPreference>()
            .HasIndex(cp => new { cp.CustomerId, cp.PreferenceType })
            .HasDatabaseName("IX_CustomerPreference_Customer_Type");

        // ===== COMPOSITE INDEXES POUR RECHERCHE =====
        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => new { c.Status, c.RegistrationDate, c.LastLoginDate })
            .HasDatabaseName("IX_Customer_Status_Dates_Composite");
    }

    /// <summary>
    /// Configuration des stored procedures pour opérations complexes
    /// </summary>
    private void ConfigureStoredProcedures(ModelBuilder modelBuilder)
    {
        // Configuration pour les procédures stockées d'analytics
        modelBuilder.Entity<CustomerAnalyticsResult>()
            .HasNoKey()
            .ToView(null);

        modelBuilder.Entity<LoyaltyAnalyticsResult>()
            .HasNoKey()
            .ToView(null);

        modelBuilder.Entity<SegmentAnalyticsResult>()
            .HasNoKey()
            .ToView(null);
    }

    /// <summary>
    /// Override SaveChanges pour Domain Events et Audit automatique
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collecte des Domain Events avant sauvegarde
        var domainEvents = ExtractDomainEvents();

        // Audit automatique
        SetAuditProperties();

        // Sauvegarde principale
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publication des Domain Events après sauvegarde
        await PublishDomainEvents(domainEvents);

        return result;
    }

    /// <summary>
    /// Extraction des Domain Events des entités
    /// </summary>
    private List<IDomainEvent> ExtractDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        var domainEntities = ChangeTracker.Entries()
            .Where(x => x.Entity is IHasDomainEvents)
            .Select(x => x.Entity as IHasDomainEvents)
            .Where(x => x!.DomainEvents.Any())
            .ToList();

        foreach (var entity in domainEntities)
        {
            domainEvents.AddRange(entity!.DomainEvents);
            entity.ClearDomainEvents();
        }

        return domainEvents;
    }

    /// <summary>
    /// Configuration automatique des propriétés d'audit
    /// </summary>
    private void SetAuditProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId(); // À implémenter via IHttpContextAccessor

            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = now;
                entry.Property("CreatedBy").CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
                entry.Property("UpdatedBy").CurrentValue = userId;
            }
        }
    }

    /// <summary>
    /// Publication des Domain Events (via MediatR)
    /// </summary>
    private async Task PublishDomainEvents(List<IDomainEvent> domainEvents)
    {
        // TODO: Intégration avec MediatR pour publication des events
        foreach (var domainEvent in domainEvents)
        {
            // await _mediator.Publish(domainEvent);
            
            // Pour l'instant, stockage dans outbox pour processing ultérieur
            DomainEvents.Add(new DomainEventOutbox
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().Name,
                EventData = System.Text.Json.JsonSerializer.Serialize(domainEvent),
                OccurredOn = domainEvent.OccurredOn,
                ProcessedOn = null
            });
        }
    }

    /// <summary>
    /// Récupération de l'utilisateur courant (à implémenter)
    /// </summary>
    private string GetCurrentUserId()
    {
        // TODO: Intégration avec IHttpContextAccessor
        return "system"; // Valeur par défaut
    }
}

// ===== INTERFACES POUR SHADOW PROPERTIES =====
public interface IAuditableEntity
{
    // Marqueur pour les entités auditables
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public interface IVersionedEntity
{
    // Marqueur pour les entités versionnées
}

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

// ===== ENTITÉS POUR OUTBOX PATTERN =====
public class DomainEventOutbox
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// ===== RÉSULTATS POUR STORED PROCEDURES =====
public class CustomerAnalyticsResult
{
    public string MetricName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class LoyaltyAnalyticsResult
{
    public string ProgramName { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public decimal AveragePoints { get; set; }
    public decimal RedemptionRate { get; set; }
}

public class SegmentAnalyticsResult
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal AverageValue { get; set; }
    public decimal GrowthRate { get; set; }
}