using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Stock.Domain.Enums;
using Stock.Domain.Events;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Entité représentant un emplacement de stockage
/// </summary>
public sealed class Location : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public LocationType LocationType { get; private set; }
    public string? Description { get; private set; }
    public LocationAddress? Address { get; private set; }
    public bool IsActive { get; private set; }
    public string? DeactivationReason { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    // Propriétés spécifiques au type d'emplacement
    public decimal? Capacity { get; private set; } // Capacité de stockage
    public string? CapacityUnit { get; private set; }
    public decimal? Temperature { get; private set; } // Température de stockage
    public decimal? Humidity { get; private set; } // Humidité relative
    public bool? IsTemperatureControlled { get; private set; }
    public bool? IsHumidityControlled { get; private set; }

    // Hiérarchie des emplacements
    public Guid? ParentLocationId { get; private set; }
    public string? Zone { get; private set; } // Zone dans l'entrepôt
    public string? Aisle { get; private set; } // Allée
    public string? Shelf { get; private set; } // Étagère
    public string? Bin { get; private set; } // Bac/casier

    // Restrictions d'accès
    public bool RequiresAuthorization { get; private set; }
    public string? AccessLevel { get; private set; } // PUBLIC, RESTRICTED, SECURED
    public bool IsPickingLocation { get; private set; }
    public bool IsReceivingLocation { get; private set; }
    public bool IsShippingLocation { get; private set; }

    // Métriques et informations
    public int Priority { get; private set; } // Priorité pour le picking (1 = haute priorité)
    public decimal? CurrentUtilization { get; private set; } // % d'utilisation actuelle
    public DateTime? LastInventoryDate { get; private set; }
    public Guid? ResponsibleUserId { get; private set; }

    private readonly List<LocationStockLevel> _stockLevels = new();
    public IReadOnlyList<LocationStockLevel> StockLevels => _stockLevels.AsReadOnly();

    private Location() { } // EF Constructor

    public Location(
        string name,
        string code,
        LocationType locationType,
        string? description = null,
        LocationAddress? address = null,
        Guid? parentLocationId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Location code cannot be null or empty", nameof(code));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Code = code.Trim().ToUpperInvariant();
        LocationType = locationType;
        Description = description?.Trim();
        Address = address;
        ParentLocationId = parentLocationId;
        IsActive = true;
        Priority = 100; // Priorité moyenne par défaut
        RequiresAuthorization = false;
        AccessLevel = "PUBLIC";
        CreatedAt = DateTime.UtcNow;

        // Configuration par défaut selon le type
        ConfigureByType(locationType);

        AddDomainEvent(new LocationCreatedEvent(
            Id,
            Name,
            LocationType,
            Code,
            CreatedAt));
    }

    /// <summary>
    /// Mettre à jour les informations de base
    /// </summary>
    public void UpdateInformation(string name, string? description = null, LocationAddress? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Location name cannot be null or empty", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Address = address;
        UpdateTimestamp();
    }

    /// <summary>
    /// Configurer les propriétés physiques
    /// </summary>
    public void ConfigurePhysicalProperties(
        decimal? capacity = null,
        string? capacityUnit = null,
        decimal? temperature = null,
        decimal? humidity = null,
        bool? isTemperatureControlled = null,
        bool? isHumidityControlled = null)
    {
        if (capacity.HasValue && capacity.Value <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));

        if (temperature.HasValue && (temperature.Value < -50 || temperature.Value > 100))
            throw new ArgumentException("Temperature must be between -50°C and 100°C", nameof(temperature));

        if (humidity.HasValue && (humidity.Value < 0 || humidity.Value > 100))
            throw new ArgumentException("Humidity must be between 0% and 100%", nameof(humidity));

        Capacity = capacity;
        CapacityUnit = capacityUnit?.Trim().ToUpperInvariant();
        Temperature = temperature;
        Humidity = humidity;
        IsTemperatureControlled = isTemperatureControlled;
        IsHumidityControlled = isHumidityControlled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer la hiérarchie d'emplacement
    /// </summary>
    public void ConfigureHierarchy(
        Guid? parentLocationId = null,
        string? zone = null,
        string? aisle = null,
        string? shelf = null,
        string? bin = null)
    {
        if (parentLocationId == Id)
            throw new ArgumentException("Location cannot be its own parent", nameof(parentLocationId));

        ParentLocationId = parentLocationId;
        Zone = zone?.Trim();
        Aisle = aisle?.Trim();
        Shelf = shelf?.Trim();
        Bin = bin?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les restrictions d'accès
    /// </summary>
    public void ConfigureAccess(
        bool requiresAuthorization = false,
        string accessLevel = "PUBLIC",
        bool isPickingLocation = false,
        bool isReceivingLocation = false,
        bool isShippingLocation = false,
        int priority = 100,
        Guid? responsibleUserId = null)
    {
        var validAccessLevels = new[] { "PUBLIC", "RESTRICTED", "SECURED" };
        if (!validAccessLevels.Contains(accessLevel.ToUpperInvariant()))
            throw new ArgumentException($"Access level must be one of: {string.Join(", ", validAccessLevels)}", nameof(accessLevel));

        if (priority <= 0)
            throw new ArgumentException("Priority must be positive (1 = highest priority)", nameof(priority));

        RequiresAuthorization = requiresAuthorization;
        AccessLevel = accessLevel.ToUpperInvariant();
        IsPickingLocation = isPickingLocation;
        IsReceivingLocation = isReceivingLocation;
        IsShippingLocation = isShippingLocation;
        Priority = priority;
        ResponsibleUserId = responsibleUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir les seuils de stock pour un produit
    /// </summary>
    public void SetStockThresholds(
        Guid productId,
        StockQuantity? minimumLevel = null,
        StockQuantity? maximumLevel = null,
        StockQuantity? reorderLevel = null,
        StockQuantity? safetyStockLevel = null)
    {
        var existingLevel = _stockLevels.FirstOrDefault(sl => sl.ProductId == productId);
        if (existingLevel != null)
        {
            existingLevel.UpdateThresholds(minimumLevel, maximumLevel, reorderLevel, safetyStockLevel);
        }
        else
        {
            var stockLevel = new LocationStockLevel(Id, productId, minimumLevel, maximumLevel, reorderLevel, safetyStockLevel);
            _stockLevels.Add(stockLevel);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer les seuils de stock pour un produit
    /// </summary>
    public void RemoveStockThresholds(Guid productId)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl => sl.ProductId == productId);
        if (stockLevel != null)
        {
            _stockLevels.Remove(stockLevel);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mettre à jour l'utilisation actuelle
    /// </summary>
    public void UpdateUtilization(decimal utilizationPercentage)
    {
        if (utilizationPercentage < 0 || utilizationPercentage > 100)
            throw new ArgumentException("Utilization must be between 0% and 100%", nameof(utilizationPercentage));

        CurrentUtilization = utilizationPercentage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enregistrer un inventaire
    /// </summary>
    public void RecordInventory()
    {
        LastInventoryDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Désactiver l'emplacement
    /// </summary>
    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw new InvalidOperationException("Location is already deactivated");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Deactivation reason is required", nameof(reason));

        IsActive = false;
        DeactivationReason = reason.Trim();
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new LocationDeactivatedEvent(
            Id,
            Name,
            DeactivationReason,
            DeactivatedAt.Value));
    }

    /// <summary>
    /// Réactiver l'emplacement
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            throw new InvalidOperationException("Location is already active");

        IsActive = true;
        DeactivationReason = null;
        DeactivatedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtenir l'adresse complète de l'emplacement (avec hiérarchie)
    /// </summary>
    public string GetFullLocationPath()
    {
        var parts = new List<string> { Code };
        
        if (!string.IsNullOrEmpty(Zone)) parts.Add($"Zone:{Zone}");
        if (!string.IsNullOrEmpty(Aisle)) parts.Add($"Allée:{Aisle}");
        if (!string.IsNullOrEmpty(Shelf)) parts.Add($"Étagère:{Shelf}");
        if (!string.IsNullOrEmpty(Bin)) parts.Add($"Bac:{Bin}");
        
        return string.Join(" / ", parts);
    }

    /// <summary>
    /// Vérifier si l'emplacement nécessite des conditions spéciales
    /// </summary>
    public bool RequiresSpecialConditions => IsTemperatureControlled == true || IsHumidityControlled == true;

    /// <summary>
    /// Vérifier si l'emplacement est adapté pour un produit donné
    /// </summary>
    public bool IsSuitableForProduct(bool requiresTemperatureControl, bool requiresHumidityControl)
    {
        if (requiresTemperatureControl && IsTemperatureControlled != true)
            return false;
        
        if (requiresHumidityControl && IsHumidityControlled != true)
            return false;
        
        return true;
    }

    private void ConfigureByType(LocationType locationType)
    {
        switch (locationType)
        {
            case LocationType.Warehouse:
                IsReceivingLocation = true;
                IsShippingLocation = true;
                IsPickingLocation = true;
                CapacityUnit = "M3";
                break;
            
            case LocationType.Store:
                IsPickingLocation = true;
                Priority = 1; // Haute priorité pour les magasins
                break;
            
            case LocationType.FrozenStorage:
                IsTemperatureControlled = true;
                Temperature = -18;
                RequiresAuthorization = true;
                AccessLevel = "RESTRICTED";
                break;
            
            case LocationType.ColdStorage:
                IsTemperatureControlled = true;
                Temperature = 4;
                RequiresAuthorization = true;
                AccessLevel = "RESTRICTED";
                break;
            
            case LocationType.DryStorage:
                IsHumidityControlled = true;
                Humidity = 50;
                break;
            
            case LocationType.Quarantine:
                RequiresAuthorization = true;
                AccessLevel = "SECURED";
                IsPickingLocation = false;
                Priority = 999; // Basse priorité
                break;
            
            case LocationType.Production:
                RequiresAuthorization = true;
                AccessLevel = "RESTRICTED";
                IsReceivingLocation = true;
                break;
            
            case LocationType.Damaged:
                RequiresAuthorization = true;
                AccessLevel = "RESTRICTED";
                IsPickingLocation = false;
                Priority = 999;
                break;
        }
    }
}

/// <summary>
/// Seuils de stock pour un produit dans un emplacement
/// </summary>
public sealed class LocationStockLevel : Entity
{
    public Guid LocationId { get; private set; }
    public Guid ProductId { get; private set; }
    public StockQuantity? MinimumLevel { get; private set; }
    public StockQuantity? MaximumLevel { get; private set; }
    public StockQuantity? ReorderLevel { get; private set; }
    public StockQuantity? SafetyStockLevel { get; private set; }

    private LocationStockLevel() { } // EF Constructor

    public LocationStockLevel(
        Guid locationId,
        Guid productId,
        StockQuantity? minimumLevel = null,
        StockQuantity? maximumLevel = null,
        StockQuantity? reorderLevel = null,
        StockQuantity? safetyStockLevel = null)
    {
        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        ValidateThresholds(minimumLevel, maximumLevel, reorderLevel, safetyStockLevel);

        Id = Guid.NewGuid();
        LocationId = locationId;
        ProductId = productId;
        MinimumLevel = minimumLevel;
        MaximumLevel = maximumLevel;
        ReorderLevel = reorderLevel;
        SafetyStockLevel = safetyStockLevel;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateThresholds(
        StockQuantity? minimumLevel = null,
        StockQuantity? maximumLevel = null,
        StockQuantity? reorderLevel = null,
        StockQuantity? safetyStockLevel = null)
    {
        ValidateThresholds(minimumLevel, maximumLevel, reorderLevel, safetyStockLevel);

        MinimumLevel = minimumLevel;
        MaximumLevel = maximumLevel;
        ReorderLevel = reorderLevel;
        SafetyStockLevel = safetyStockLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si un niveau de stock déclenche une alerte
    /// </summary>
    public AlertType? CheckAlertLevel(StockQuantity currentStock)
    {
        if (currentStock.IsZero)
            return AlertType.StockOut;

        if (MinimumLevel != null && currentStock <= MinimumLevel)
            return AlertType.LowStock;

        if (MaximumLevel != null && currentStock >= MaximumLevel)
            return AlertType.Overstock;

        if (ReorderLevel != null && currentStock <= ReorderLevel)
            return AlertType.ReorderPoint;

        if (SafetyStockLevel != null && currentStock <= SafetyStockLevel)
            return AlertType.SafetyStock;

        return null;
    }

    private static void ValidateThresholds(
        StockQuantity? minimumLevel,
        StockQuantity? maximumLevel,
        StockQuantity? reorderLevel,
        StockQuantity? safetyStockLevel)
    {
        // Valider que tous les seuils utilisent la même unité
        var units = new[] { minimumLevel?.Unit, maximumLevel?.Unit, reorderLevel?.Unit, safetyStockLevel?.Unit }
            .Where(u => u != null)
            .Distinct()
            .ToList();

        if (units.Count > 1)
            throw new ArgumentException("All threshold quantities must use the same unit");

        // Valider la hiérarchie des seuils
        if (minimumLevel != null && maximumLevel != null && minimumLevel >= maximumLevel)
            throw new ArgumentException("Minimum level must be less than maximum level");

        if (minimumLevel != null && reorderLevel != null && reorderLevel < minimumLevel)
            throw new ArgumentException("Reorder level must be greater than or equal to minimum level");

        if (maximumLevel != null && reorderLevel != null && reorderLevel > maximumLevel)
            throw new ArgumentException("Reorder level must be less than or equal to maximum level");

        if (minimumLevel != null && safetyStockLevel != null && safetyStockLevel < minimumLevel)
            throw new ArgumentException("Safety stock level must be greater than or equal to minimum level");
    }
}