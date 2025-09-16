using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.EventStore;
using Order.Infrastructure.Configurations;
using NiesPro.Contracts.Primitives;
using MediatR;

namespace Order.Infrastructure.Data;

public sealed class OrderDbContext : DbContext
{
    private readonly IMediator? _mediator;

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
    
    public OrderDbContext(DbContextOptions<OrderDbContext> options, IMediator mediator) 
        : base(options) 
    {
        _mediator = mediator;
    }

    public DbSet<Domain.Entities.Order> Orders => Set<Domain.Entities.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StoredEvent> Events => Set<StoredEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appliquer toutes les configurations
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new StoredEventConfiguration());

        // Contraintes globales
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Convention pour les noms de tables
            if (entityType.ClrType.Namespace?.Contains("Order.Domain") == true)
            {
                entityType.SetTableName($"Order_{entityType.DisplayName()}");
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Traitement des Domain Events avant sauvegarde
        var domainEvents = GetDomainEvents();
        
        // Marquer les timestamps
        MarkTimestamps();
        
        var result = await base.SaveChangesAsync(cancellationToken);
        
        // Publier les domain events après sauvegarde réussie
        if (_mediator != null)
        {
            await PublishDomainEventsAsync(domainEvents, cancellationToken);
        }
        
        return result;
    }

    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();
        
        var aggregates = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            domainEvents.AddRange(aggregate.DomainEvents);
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }

    private void MarkTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Entity entity)
            {
                // Logique de timestamps si nécessaire
                // Peut être étendue selon les besoins métier
            }
        }
    }

    private async Task PublishDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _mediator!.Publish(domainEvent, cancellationToken);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Configuration par défaut pour le développement
            optionsBuilder.UseMySql("Server=localhost;Port=3306;Database=NiesPro_Order;Uid=root;Pwd=;",
                new MySqlServerVersion(new Version(8, 0, 21)));
        }

        // Configurations de performance
        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableServiceProviderCaching();
        optionsBuilder.EnableDetailedErrors(false);
    }
}