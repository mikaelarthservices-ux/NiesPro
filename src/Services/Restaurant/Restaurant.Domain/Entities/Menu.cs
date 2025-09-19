using BuildingBlocks.Domain;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant un menu de restaurant
/// </summary>
public sealed class Menu : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MenuType MenuType { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Currency { get; private set; } = "EUR";

    // Période de validité
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    
    // Horaires de service
    public TimeOnly? AvailableFromTime { get; private set; }
    public TimeOnly? AvailableUntilTime { get; private set; }
    public List<DayOfWeek> AvailableDays { get; private set; } = new();

    // Métriques
    public int TotalItems { get; private set; }
    public decimal? AverageItemPrice { get; private set; }
    public int PopularityScore { get; private set; }
    public DateTime? LastModified { get; private set; }

    // Configuration
    public bool AllowsCustomization { get; private set; }
    public bool ShowNutritionalInfo { get; private set; }
    public bool ShowAllergenInfo { get; private set; }
    public string? SpecialInstructions { get; private set; }

    private readonly List<MenuSection> _sections = new();
    public IReadOnlyList<MenuSection> Sections => _sections.AsReadOnly();

    private readonly List<MenuItem> _items = new();
    public IReadOnlyList<MenuItem> Items => _items.AsReadOnly();

    private Menu() { } // EF Constructor

    public Menu(
        string name,
        MenuType menuType,
        string? description = null,
        string currency = "EUR",
        bool isDefault = false,
        DateTime? validFrom = null,
        DateTime? validUntil = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Menu name cannot be null or empty", nameof(name));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (validFrom.HasValue && validUntil.HasValue && validFrom >= validUntil)
            throw new ArgumentException("Valid from date must be before valid until date");

        Id = Guid.NewGuid();
        Name = name.Trim();
        MenuType = menuType;
        Description = description?.Trim();
        Currency = currency.ToUpperInvariant();
        IsActive = true;
        IsDefault = isDefault;
        DisplayOrder = 0;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        AllowsCustomization = true;
        ShowNutritionalInfo = true;
        ShowAllergenInfo = true;
        AvailableDays = Enum.GetValues<DayOfWeek>().ToList(); // Tous les jours par défaut
        TotalItems = 0;
        PopularityScore = 0;
        CreatedAt = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter une section au menu
    /// </summary>
    public void AddSection(string sectionName, string? description = null, int? displayOrder = null)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Section name cannot be null or empty", nameof(sectionName));

        if (_sections.Any(s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Section '{sectionName}' already exists");

        var order = displayOrder ?? (_sections.Any() ? _sections.Max(s => s.DisplayOrder) + 1 : 1);
        var section = new MenuSection(Id, sectionName, description, order);
        
        _sections.Add(section);
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer une section
    /// </summary>
    public void RemoveSection(Guid sectionId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section == null)
            throw new InvalidOperationException($"Section with ID {sectionId} not found");

        // Vérifier qu'il n'y a pas d'éléments dans cette section
        var itemsInSection = _items.Where(i => i.SectionId == sectionId).ToList();
        if (itemsInSection.Any())
            throw new InvalidOperationException($"Cannot remove section that contains {itemsInSection.Count} items");

        _sections.Remove(section);
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter un élément au menu
    /// </summary>
    public void AddItem(MenuItem menuItem)
    {
        if (menuItem == null)
            throw new ArgumentNullException(nameof(menuItem));

        if (menuItem.MenuId != Id)
            throw new InvalidOperationException("Menu item must belong to this menu");

        if (_items.Any(i => i.Name.Equals(menuItem.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Item '{menuItem.Name}' already exists in this menu");

        _items.Add(menuItem);
        TotalItems = _items.Count;
        RecalculateAveragePrice();
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemAddedEvent(
            menuItem.Id,
            Id,
            menuItem.Name,
            menuItem.Category,
            menuItem.Price,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Supprimer un élément du menu
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException($"Item with ID {itemId} not found");

        _items.Remove(item);
        TotalItems = _items.Count;
        RecalculateAveragePrice();
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour les informations du menu
    /// </summary>
    public void UpdateInformation(
        string? name = null,
        string? description = null,
        string? imageUrl = null,
        string? specialInstructions = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();

        if (description != null)
            Description = description.Trim();

        if (imageUrl != null)
            ImageUrl = imageUrl.Trim();

        if (specialInstructions != null)
            SpecialInstructions = specialInstructions.Trim();

        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer la période de validité
    /// </summary>
    public void SetValidityPeriod(DateTime? validFrom, DateTime? validUntil)
    {
        if (validFrom.HasValue && validUntil.HasValue && validFrom >= validUntil)
            throw new ArgumentException("Valid from date must be before valid until date");

        ValidFrom = validFrom;
        ValidUntil = validUntil;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les horaires de disponibilité
    /// </summary>
    public void SetAvailableHours(TimeOnly? fromTime, TimeOnly? untilTime, List<DayOfWeek>? availableDays = null)
    {
        if (fromTime.HasValue && untilTime.HasValue && fromTime >= untilTime)
            throw new ArgumentException("From time must be before until time");

        AvailableFromTime = fromTime;
        AvailableUntilTime = untilTime;
        
        if (availableDays != null)
            AvailableDays = availableDays.ToList();

        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les options d'affichage
    /// </summary>
    public void ConfigureDisplay(
        bool? allowsCustomization = null,
        bool? showNutritionalInfo = null,
        bool? showAllergenInfo = null,
        int? displayOrder = null)
    {
        if (allowsCustomization.HasValue)
            AllowsCustomization = allowsCustomization.Value;

        if (showNutritionalInfo.HasValue)
            ShowNutritionalInfo = showNutritionalInfo.Value;

        if (showAllergenInfo.HasValue)
            ShowAllergenInfo = showAllergenInfo.Value;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activer le menu
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (MenuType == MenuType.SeasonalMenu)
        {
            AddDomainEvent(new SeasonalMenuActivatedEvent(
                Id,
                Name,
                MenuType,
                _items.Select(i => i.Id).ToList(),
                new List<Guid>(), // Items supprimés (pour les menus saisonniers)
                DateTime.UtcNow));
        }
    }

    /// <summary>
    /// Désactiver le menu
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        IsDefault = false; // Un menu inactif ne peut pas être par défaut
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir comme menu par défaut
    /// </summary>
    public void SetAsDefault()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot set inactive menu as default");

        IsDefault = true;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retirer le statut de menu par défaut
    /// </summary>
    public void RemoveDefaultStatus()
    {
        IsDefault = false;
        LastModified = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrémenter le score de popularité
    /// </summary>
    public void IncrementPopularity(int points = 1)
    {
        PopularityScore += Math.Max(0, points);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si le menu est actuellement disponible
    /// </summary>
    public bool IsCurrentlyAvailable
    {
        get
        {
            if (!IsActive)
                return false;

            var now = DateTime.UtcNow;
            
            // Vérifier la période de validité
            if (ValidFrom.HasValue && now < ValidFrom.Value)
                return false;
            
            if (ValidUntil.HasValue && now > ValidUntil.Value)
                return false;

            // Vérifier le jour de la semaine
            if (!AvailableDays.Contains(now.DayOfWeek))
                return false;

            // Vérifier l'heure
            if (AvailableFromTime.HasValue || AvailableUntilTime.HasValue)
            {
                var currentTime = TimeOnly.FromDateTime(now);
                
                if (AvailableFromTime.HasValue && currentTime < AvailableFromTime.Value)
                    return false;
                
                if (AvailableUntilTime.HasValue && currentTime > AvailableUntilTime.Value)
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Obtenir les éléments disponibles
    /// </summary>
    public IReadOnlyList<MenuItem> GetAvailableItems()
    {
        return _items.Where(item => item.IsCurrentlyAvailable).ToList();
    }

    /// <summary>
    /// Obtenir les éléments par catégorie
    /// </summary>
    public IReadOnlyList<MenuItem> GetItemsByCategory(MenuCategory category)
    {
        return _items.Where(item => item.Category == category).ToList();
    }

    /// <summary>
    /// Obtenir les éléments par section
    /// </summary>
    public IReadOnlyList<MenuItem> GetItemsBySection(Guid sectionId)
    {
        return _items.Where(item => item.SectionId == sectionId).ToList();
    }

    /// <summary>
    /// Rechercher des éléments
    /// </summary>
    public IReadOnlyList<MenuItem> SearchItems(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return _items.ToList();

        var term = searchTerm.ToLowerInvariant();
        return _items.Where(item => 
            item.Name.ToLowerInvariant().Contains(term) ||
            (item.Description != null && item.Description.ToLowerInvariant().Contains(term))
        ).ToList();
    }

    /// <summary>
    /// Obtenir les statistiques du menu
    /// </summary>
    public MenuStatistics GetStatistics()
    {
        var availableItems = GetAvailableItems();
        var outOfStockItems = _items.Count(i => i.Status == MenuItemStatus.OutOfStock);
        
        return new MenuStatistics(
            TotalItems,
            availableItems.Count,
            outOfStockItems,
            AverageItemPrice ?? 0,
            PopularityScore,
            _sections.Count
        );
    }

    private void RecalculateAveragePrice()
    {
        if (!_items.Any())
        {
            AverageItemPrice = null;
            return;
        }

        var availableItems = _items.Where(i => i.Status == MenuItemStatus.Available).ToList();
        if (!availableItems.Any())
        {
            AverageItemPrice = null;
            return;
        }

        AverageItemPrice = availableItems.Average(i => i.Price.GetEffectivePrice());
    }
}

/// <summary>
/// Section d'un menu
/// </summary>
public sealed class MenuSection : Entity
{
    public Guid MenuId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsVisible { get; private set; }

    private MenuSection() { } // EF Constructor

    public MenuSection(Guid menuId, string name, string? description = null, int displayOrder = 0)
    {
        if (menuId == Guid.Empty)
            throw new ArgumentException("Menu ID cannot be empty", nameof(menuId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Section name cannot be null or empty", nameof(name));

        Id = Guid.NewGuid();
        MenuId = menuId;
        Name = name.Trim();
        Description = description?.Trim();
        DisplayOrder = displayOrder;
        IsVisible = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateInformation(string? name = null, string? description = null, string? imageUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();

        if (description != null)
            Description = description.Trim();

        if (imageUrl != null)
            ImageUrl = imageUrl.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Statistiques d'un menu
/// </summary>
public record MenuStatistics(
    int TotalItems,
    int AvailableItems,
    int OutOfStockItems,
    decimal AveragePrice,
    int PopularityScore,
    int SectionsCount
);