using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BuildingBlocks.Domain;
using BuildingBlocks.Infrastructure.Data;
using Restaurant.Domain.Entities;
using Restaurant.Domain.ValueObjects;
using Restaurant.Infrastructure.Data.Configurations;
using System.Reflection;

namespace Restaurant.Infrastructure.Data;

/// <summary>
/// Contexte de base de données pour le service Restaurant
/// </summary>
public class RestaurantDbContext : DbContext, IUnitOfWork
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public RestaurantDbContext(
        DbContextOptions<RestaurantDbContext> options,
        IDomainEventDispatcher domainEventDispatcher) : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
    }

    // Tables principales
    public DbSet<Table> Tables { get; set; } = null!;
    public DbSet<TableReservation> TableReservations { get; set; } = null!;
    public DbSet<Menu> Menus { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<KitchenOrder> KitchenOrders { get; set; } = null!;
    public DbSet<Staff> Staff { get; set; } = null!;

    // Entités liées
    public DbSet<MenuItemVariation> MenuItemVariations { get; set; } = null!;
    public DbSet<MenuItemPromotion> MenuItemPromotions { get; set; } = null!;
    public DbSet<KitchenOrderItem> KitchenOrderItems { get; set; } = null!;
    public DbSet<KitchenOrderLog> KitchenOrderLogs { get; set; } = null!;
    public DbSet<KitchenOrderModification> KitchenOrderModifications { get; set; } = null!;
    public DbSet<StaffSchedule> StaffSchedules { get; set; } = null!;
    public DbSet<StaffPerformance> StaffPerformances { get; set; } = null!;
    public DbSet<StaffTraining> StaffTrainings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Application des configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configuration des Value Objects
        ConfigureValueObjects(modelBuilder);

        // Configuration des enums
        ConfigureEnums(modelBuilder);

        // Index et contraintes
        ConfigureIndexesAndConstraints(modelBuilder);

        // Données de seed (si nécessaire)
        ConfigureSeedData(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Configuration du logging et de la performance
        optionsBuilder
            .EnableSensitiveDataLogging(false)
            .EnableServiceProviderCaching()
            .EnableDetailedErrors();

        // Configuration des intercepteurs
        // optionsBuilder.AddInterceptors(new AuditInterceptor(), new PerformanceInterceptor());
    }

    /// <summary>
    /// Sauvegarder les changements avec dispatch des événements de domaine
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Mise à jour des timestamps
        UpdateTimestamps();

        // Collecte des événements de domaine avant sauvegarde
        var domainEvents = CollectDomainEvents();

        // Sauvegarde
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch des événements après succès de la sauvegarde
        await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    /// <summary>
    /// Sauvegarder sans dispatch d'événements (pour les tests ou cas spéciaux)
    /// </summary>
    public async Task<int> SaveChangesWithoutEventsAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Commencer une transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is null)
        {
            await Database.BeginTransactionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Confirmer la transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CommitTransactionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Annuler la transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.RollbackTransactionAsync(cancellationToken);
        }
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (Entity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
                // Empêcher la modification de CreatedAt
                entityEntry.Property(nameof(Entity.CreatedAt)).IsModified = false;
            }
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Nettoyer les événements des entités
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }

    private static void ConfigureValueObjects(ModelBuilder modelBuilder)
    {
        // Configuration des Value Objects pour Table
        modelBuilder.Entity<Table>()
            .OwnsOne(t => t.Capacity, builder =>
            {
                builder.Property(c => c.Minimum).HasColumnName("MinCapacity");
                builder.Property(c => c.Maximum).HasColumnName("MaxCapacity");
                builder.Property(c => c.Optimal).HasColumnName("OptimalCapacity");
            });

        modelBuilder.Entity<Table>()
            .OwnsOne(t => t.Position, builder =>
            {
                builder.Property(p => p.X).HasColumnName("XCoordinate").HasPrecision(10, 2);
                builder.Property(p => p.Y).HasColumnName("YCoordinate").HasPrecision(10, 2);
                builder.Property(p => p.Zone).HasColumnName("Zone").HasMaxLength(50);
            });

        // Configuration des Value Objects pour MenuItem
        modelBuilder.Entity<MenuItem>()
            .OwnsOne(m => m.Price, builder =>
            {
                builder.Property(p => p.Amount).HasColumnName("Price").HasPrecision(10, 2);
                builder.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
            });

        modelBuilder.Entity<MenuItem>()
            .OwnsOne(m => m.PreparationTime, builder =>
            {
                builder.Property(p => p.TotalTime).HasColumnName("PreparationTimeTotal");
                builder.Property(p => p.ActiveTime).HasColumnName("PreparationTimeActive");
            });

        modelBuilder.Entity<MenuItem>()
            .OwnsOne(m => m.NutritionalInfo, builder =>
            {
                builder.Property(n => n.Calories).HasColumnName("Calories");
                builder.Property(n => n.Protein).HasColumnName("Protein").HasPrecision(8, 2);
                builder.Property(n => n.Carbohydrates).HasColumnName("Carbohydrates").HasPrecision(8, 2);
                builder.Property(n => n.Fat).HasColumnName("Fat").HasPrecision(8, 2);
                builder.Property(n => n.Fiber).HasColumnName("Fiber").HasPrecision(8, 2);
                builder.Property(n => n.Sugar).HasColumnName("Sugar").HasPrecision(8, 2);
                builder.Property(n => n.Sodium).HasColumnName("Sodium").HasPrecision(8, 2);
                builder.Property(n => n.Unit).HasColumnName("NutritionalUnit").HasMaxLength(10);
            });

        // Configuration des Value Objects pour KitchenOrder
        // TODO: Ajouter ces configurations quand les Value Objects seront créés
        /*
        modelBuilder.Entity<KitchenOrder>()
            .OwnsOne(k => k.EstimatedPreparationTime, builder =>
            {
                builder.Property(p => p.TotalTime).HasColumnName("EstimatedTotalTime");
                builder.Property(p => p.ActiveTime).HasColumnName("EstimatedActiveTime");
            });
        */

        // Configuration des Value Objects pour Staff
        // TODO: Ajouter ces configurations quand les Value Objects seront créés
        /*
        modelBuilder.Entity<Staff>()
            .OwnsOne(s => s.Address, builder =>
            {
                builder.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                builder.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
                builder.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
                builder.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
                builder.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
                builder.Property(a => a.AdditionalInfo).HasColumnName("AddressAdditionalInfo").HasMaxLength(500);
            });

        modelBuilder.Entity<Staff>()
            .OwnsOne(s => s.WorkSchedule, builder =>
            {
                builder.Property(w => w.TotalHoursPerWeek).HasColumnName("TotalHoursPerWeek");
                builder.Property(w => w.IsFlexible).HasColumnName("IsFlexibleSchedule");
                builder.OwnsMany(w => w.WorkDays, wd =>
                {
                    wd.Property(d => d.DayOfWeek).HasColumnName("DayOfWeek");
                    wd.Property(d => d.IsWorkingDay).HasColumnName("IsWorkingDay");
                    wd.Property(d => d.StartTime).HasColumnName("StartTime");
                    wd.Property(d => d.EndTime).HasColumnName("EndTime");
                    wd.Property(d => d.BreakStartTime).HasColumnName("BreakStartTime");
                    wd.Property(d => d.BreakEndTime).HasColumnName("BreakEndTime");
                    wd.ToTable("StaffWorkDays");
                });
            });
        */
    }

    private static void ConfigureEnums(ModelBuilder modelBuilder)
    {
        // Configuration des enums comme strings pour lisibilité
        modelBuilder.Entity<Table>()
            .Property(t => t.TableType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Table>()
            .Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Table>()
            .Property(t => t.Section)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<TableReservation>()
            .Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<TableReservation>()
            .Property(r => r.ReservationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Menu>()
            .Property(m => m.MenuType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Menu>()
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<KitchenOrder>()
            .Property(k => k.OrderType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<KitchenOrder>()
            .Property(k => k.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<KitchenOrder>()
            .Property(k => k.Priority)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<KitchenOrder>()
            .Property(k => k.KitchenSection)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Staff>()
            .Property(s => s.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Staff>()
            .Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Staff>()
            .Property(s => s.EmploymentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<Staff>()
            .Property(s => s.Department)
            .HasConversion<string>()
            .HasMaxLength(50);
    }

    private static void ConfigureIndexesAndConstraints(ModelBuilder modelBuilder)
    {
        // Index pour Table
        modelBuilder.Entity<Table>()
            .HasIndex(t => t.Number)
            .IsUnique()
            .HasDatabaseName("IX_Tables_Number");

        modelBuilder.Entity<Table>()
            .HasIndex(t => t.Section)
            .HasDatabaseName("IX_Tables_Section");

        modelBuilder.Entity<Table>()
            .HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tables_Status");

        // Index pour TableReservation
        modelBuilder.Entity<TableReservation>()
            .HasIndex(r => r.ReservationNumber)
            .IsUnique()
            .HasDatabaseName("IX_TableReservations_ReservationNumber");

        modelBuilder.Entity<TableReservation>()
            .HasIndex(r => new { r.TableId, r.ReservationDateTime })
            .HasDatabaseName("IX_TableReservations_Table_DateTime");

        modelBuilder.Entity<TableReservation>()
            .HasIndex(r => r.CustomerPhone)
            .HasDatabaseName("IX_TableReservations_CustomerPhone");

        modelBuilder.Entity<TableReservation>()
            .HasIndex(r => r.Status)
            .HasDatabaseName("IX_TableReservations_Status");

        // Index pour Menu
        modelBuilder.Entity<Menu>()
            .HasIndex(m => m.MenuType)
            .HasDatabaseName("IX_Menus_MenuType");

        modelBuilder.Entity<Menu>()
            .HasIndex(m => m.Status)
            .HasDatabaseName("IX_Menus_Status");

        // Index pour MenuItem
        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => new { m.MenuId, m.Category })
            .HasDatabaseName("IX_MenuItems_Menu_Category");

        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.Status)
            .HasDatabaseName("IX_MenuItems_Status");

        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.IsAvailable)
            .HasDatabaseName("IX_MenuItems_IsAvailable");

        // Index pour KitchenOrder
        modelBuilder.Entity<KitchenOrder>()
            .HasIndex(k => k.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_KitchenOrders_OrderNumber");

        modelBuilder.Entity<KitchenOrder>()
            .HasIndex(k => new { k.Status, k.OrderedAt })
            .HasDatabaseName("IX_KitchenOrders_Status_OrderedAt");

        modelBuilder.Entity<KitchenOrder>()
            .HasIndex(k => k.TableId)
            .HasDatabaseName("IX_KitchenOrders_TableId");

        modelBuilder.Entity<KitchenOrder>()
            .HasIndex(k => k.KitchenSection)
            .HasDatabaseName("IX_KitchenOrders_KitchenSection");

        // Index pour Staff
        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.EmployeeId)
            .IsUnique()
            .HasDatabaseName("IX_Staff_EmployeeId");

        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.Email)
            .IsUnique()
            .HasDatabaseName("IX_Staff_Email");

        modelBuilder.Entity<Staff>()
            .HasIndex(s => new { s.Position, s.WorkStatus })
            .HasDatabaseName("IX_Staff_Position_WorkStatus");
    }

    private static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        // Données de seed pour les sections de restaurant par défaut
        // Peut être étendu selon les besoins
    }
}

/// <summary>
/// Interface pour l'Unit of Work
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesWithoutEventsAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}