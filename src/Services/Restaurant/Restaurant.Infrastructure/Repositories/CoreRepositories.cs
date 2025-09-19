using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Data;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Repositories;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les Tables
/// </summary>
public class TableRepository : Repository<Table>, ITableRepository
{
    public TableRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir les tables disponibles à une date donnée
    /// </summary>
    public async Task<IReadOnlyList<Table>> GetAvailableTablesAsync(
        DateTime date, 
        TimeSpan time, 
        int partySize,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await Context.Set<Table>()
            .Where(t => t.IsActive && 
                       t.Capacity.Maximum >= partySize &&
                       t.Capacity.Minimum <= partySize)
            .Where(t => !Context.Set<TableReservation>()
                .Any(r => r.TableId == t.Id &&
                         r.ReservationDate >= startOfDay &&
                         r.ReservationDate < endOfDay &&
                         r.ReservationTime <= time.Add(TimeSpan.FromHours(2)) &&
                         r.ReservationTime.Add(TimeSpan.FromHours(2)) >= time &&
                         r.Status != Domain.Enums.ReservationStatus.Cancelled))
            .OrderBy(t => t.Number)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les tables par section
    /// </summary>
    public async Task<IReadOnlyList<Table>> GetTablesBySectionAsync(
        Domain.Enums.RestaurantSection section,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Table>()
            .Where(t => t.Section == section && t.IsActive)
            .OrderBy(t => t.Number)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les tables par statut
    /// </summary>
    public async Task<IReadOnlyList<Table>> GetTablesByStatusAsync(
        Domain.Enums.TableStatus status,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Table>()
            .Where(t => t.Status == status)
            .OrderBy(t => t.Number)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Rechercher des tables par capacité
    /// </summary>
    public async Task<IReadOnlyList<Table>> SearchByCapacityAsync(
        int minCapacity, 
        int maxCapacity,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Table>()
            .Where(t => t.IsActive &&
                       t.Capacity.Maximum <= maxCapacity &&
                       t.Capacity.Minimum >= minCapacity)
            .OrderBy(t => t.Capacity.Optimal)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir une table par son numéro
    /// </summary>
    public async Task<Table?> GetByNumberAsync(
        int tableNumber,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Table>()
            .FirstOrDefaultAsync(t => t.Number == tableNumber, cancellationToken);
    }
}

/// <summary>
/// Implémentation du repository pour les Réservations de Tables
/// </summary>
public class TableReservationRepository : Repository<TableReservation>, ITableReservationRepository
{
    public TableReservationRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir les réservations pour une date donnée
    /// </summary>
    public async Task<IReadOnlyList<TableReservation>> GetReservationsByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await Context.Set<TableReservation>()
            .Include(r => r.Table)
            .Where(r => r.ReservationDate >= startOfDay && r.ReservationDate < endOfDay)
            .OrderBy(r => r.ReservationTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les réservations pour une table spécifique
    /// </summary>
    public async Task<IReadOnlyList<TableReservation>> GetReservationsByTableAsync(
        Guid tableId,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<TableReservation>()
            .Where(r => r.TableId == tableId);

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReservationDate >= fromDate.Value);
        }

        return await query
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les réservations par statut
    /// </summary>
    public async Task<IReadOnlyList<TableReservation>> GetReservationsByStatusAsync(
        Domain.Enums.ReservationStatus status,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<TableReservation>()
            .Include(r => r.Table)
            .Where(r => r.Status == status)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Rechercher des réservations par client
    /// </summary>
    public async Task<IReadOnlyList<TableReservation>> SearchByCustomerAsync(
        string customerName,
        string? customerEmail = null,
        string? customerPhone = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<TableReservation>()
            .Include(r => r.Table)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(customerName))
        {
            query = query.Where(r => r.CustomerName.Contains(customerName));
        }

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            query = query.Where(r => r.CustomerEmail != null && r.CustomerEmail.Contains(customerEmail));
        }

        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            query = query.Where(r => r.CustomerPhone != null && r.CustomerPhone.Contains(customerPhone));
        }

        return await query
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir une réservation par numéro de confirmation
    /// </summary>
    public async Task<TableReservation?> GetByConfirmationNumberAsync(
        string confirmationNumber,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<TableReservation>()
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == confirmationNumber, cancellationToken);
    }
}

/// <summary>
/// Implémentation du repository pour les Menus
/// </summary>
public class MenuRepository : Repository<Menu>, IMenuRepository
{
    public MenuRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir les menus actifs
    /// </summary>
    public async Task<IReadOnlyList<Menu>> GetActiveMenusAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Menu>()
            .Where(m => m.Status == Domain.Enums.MenuStatus.Active)
            .Include(m => m.MenuItems)
            .OrderBy(m => m.MenuType)
            .ThenBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les menus par type
    /// </summary>
    public async Task<IReadOnlyList<Menu>> GetMenusByTypeAsync(
        Domain.Enums.MenuType menuType,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Menu>()
            .Where(m => m.MenuType == menuType)
            .Include(m => m.MenuItems)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les menus avec leurs items
    /// </summary>
    public async Task<Menu?> GetMenuWithItemsAsync(
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Menu>()
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Variations)
            .Include(m => m.MenuItems)
                .ThenInclude(mi => mi.Promotions)
            .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken);
    }

    /// <summary>
    /// Rechercher des menus par nom
    /// </summary>
    public async Task<IReadOnlyList<Menu>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<Menu>()
            .Where(m => m.Name.Contains(searchTerm) || 
                       (m.Description != null && m.Description.Contains(searchTerm)))
            .Include(m => m.MenuItems)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Implémentation du repository pour les Items de Menu
/// </summary>
public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(RestaurantDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtenir les items disponibles
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> GetAvailableItemsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MenuItem>()
            .Where(mi => mi.IsAvailable && mi.Status == Domain.Enums.MenuItemStatus.Active)
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.Category)
            .ThenBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les items par catégorie
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> GetItemsByCategoryAsync(
        Domain.Enums.MenuCategory category,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MenuItem>()
            .Where(mi => mi.Category == category)
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les items par menu
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> GetItemsByMenuAsync(
        Guid menuId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MenuItem>()
            .Where(mi => mi.MenuId == menuId)
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.DisplayOrder)
            .ThenBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Rechercher des items par allergènes
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> SearchByAllergensAsync(
        List<Domain.Enums.AllergenType> excludedAllergens,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MenuItem>()
            .Where(mi => mi.IsAvailable && 
                        mi.Status == Domain.Enums.MenuItemStatus.Active &&
                        !mi.Allergens.Any(a => excludedAllergens.Contains(a)))
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.Category)
            .ThenBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Rechercher des items par nom
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> SearchByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<MenuItem>()
            .Where(mi => mi.Name.Contains(searchTerm) || 
                        (mi.Description != null && mi.Description.Contains(searchTerm)))
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtenir les items en promotion
    /// </summary>
    public async Task<IReadOnlyList<MenuItem>> GetItemsOnPromotionAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await Context.Set<MenuItem>()
            .Where(mi => mi.IsAvailable && 
                        mi.Status == Domain.Enums.MenuItemStatus.Active &&
                        mi.Promotions.Any(p => p.IsActive && 
                                              p.StartDate <= now && 
                                              p.EndDate >= now))
            .Include(mi => mi.Variations)
            .Include(mi => mi.Promotions)
            .OrderBy(mi => mi.Category)
            .ThenBy(mi => mi.Name)
            .ToListAsync(cancellationToken);
    }
}