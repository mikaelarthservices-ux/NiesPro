using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant un élément de menu
/// </summary>
public sealed class MenuItem : Entity, IAggregateRoot
{
    public Guid MenuId { get; private set; }
    public Guid? SectionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MenuCategory Category { get; private set; }
    public MenuPrice Price { get; private set; }
    public MenuItemStatus Status { get; private set; }
    public PreparationTime PreparationTime { get; private set; }
    public NutritionalInfo? NutritionalInfo { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }

    // Propriétés de disponibilité
    public bool IsAvailable { get; private set; }
    public bool IsPopular { get; private set; }
    public bool IsNew { get; private set; }
    public bool IsSpicy { get; private set; }
    public bool IsVegetarian { get; private set; }
    public bool IsVegan { get; private set; }
    public bool IsGlutenFree { get; private set; }
    public bool RequiresAgeVerification { get; private set; } // Pour l'alcool

    // Allergènes et régimes
    public List<AllergenType> Allergens { get; private set; } = new();
    public List<string> DietaryRestrictions { get; private set; } = new();
    public string? ServingSize { get; private set; }
    public string? Ingredients { get; private set; }

    // Horaires de disponibilité
    public TimeOnly? AvailableFromTime { get; private set; }
    public TimeOnly? AvailableUntilTime { get; private set; }
    public List<DayOfWeek> AvailableDays { get; private set; } = new();

    // Métriques et popularité
    public int OrderCount { get; private set; }
    public decimal? AverageRating { get; private set; }
    public int ReviewCount { get; private set; }
    public DateTime? LastOrdered { get; private set; }
    public decimal Revenue { get; private set; }

    // Gestion des stocks et ingrédients
    public List<Guid> RequiredStockItems { get; private set; } = new();
    public bool RequiresStockCheck { get; private set; }
    public int? MinimumStockLevel { get; private set; }

    // Personnalisation
    public bool AllowsCustomization { get; private set; }
    public string? CustomizationInstructions { get; private set; }
    public decimal? CustomizationUpcharge { get; private set; }

    private readonly List<MenuItemVariation> _variations = new();
    public IReadOnlyList<MenuItemVariation> Variations => _variations.AsReadOnly();

    private readonly List<MenuItemPromotion> _promotions = new();
    public IReadOnlyList<MenuItemPromotion> Promotions => _promotions.AsReadOnly();

    private MenuItem() { } // EF Constructor

    public MenuItem(
        Guid menuId,
        string name,
        MenuCategory category,
        MenuPrice price,
        PreparationTime preparationTime,
        string? description = null,
        Guid? sectionId = null,
        NutritionalInfo? nutritionalInfo = null,
        List<AllergenType>? allergens = null)
    {
        if (menuId == Guid.Empty)
            throw new ArgumentException("Menu ID cannot be empty", nameof(menuId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name cannot be null or empty", nameof(name));

        Id = Guid.NewGuid();
        MenuId = menuId;
        SectionId = sectionId;
        Name = name.Trim();
        Description = description?.Trim();
        Category = category;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        PreparationTime = preparationTime ?? throw new ArgumentNullException(nameof(preparationTime));
        NutritionalInfo = nutritionalInfo;
        Status = MenuItemStatus.Available;
        IsAvailable = true;
        AllowsCustomization = true;
        RequiresStockCheck = true;
        DisplayOrder = 0;
        OrderCount = 0;
        ReviewCount = 0;
        Revenue = 0;
        Allergens = allergens?.ToList() ?? new List<AllergenType>();
        DietaryRestrictions = new List<string>();
        AvailableDays = Enum.GetValues<DayOfWeek>().ToList();
        RequiredStockItems = new List<Guid>();
        CreatedAt = DateTime.UtcNow;

        // Configuration automatique basée sur la catégorie
        ConfigureByCategory(category);

        AddDomainEvent(new MenuItemAddedEvent(
            Id,
            MenuId,
            Name,
            Category,
            Price,
            CreatedAt));
    }

    /// <summary>
    /// Mettre à jour les informations de base
    /// </summary>
    public void UpdateInformation(
        string? name = null,
        string? description = null,
        string? imageUrl = null,
        string? ingredients = null,
        string? servingSize = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name.Trim();

        if (description != null)
            Description = description.Trim();

        if (imageUrl != null)
            ImageUrl = imageUrl.Trim();

        if (ingredients != null)
            Ingredients = ingredients.Trim();

        if (servingSize != null)
            ServingSize = servingSize.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour le prix
    /// </summary>
    public void UpdatePrice(MenuPrice newPrice, string? reason = null)
    {
        var oldPrice = Price;
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemPriceUpdatedEvent(
            Id,
            oldPrice,
            newPrice,
            reason,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Changer le statut de l'élément
    /// </summary>
    public void ChangeStatus(MenuItemStatus newStatus, string? reason = null)
    {
        if (Status == newStatus)
            return;

        var previousStatus = Status;
        Status = newStatus;
        IsAvailable = newStatus == MenuItemStatus.Available || newStatus == MenuItemStatus.Seasonal;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemStatusChangedEvent(
            Id,
            previousStatus,
            newStatus,
            reason,
            UpdatedAt.Value));

        // Événement spécial pour rupture de stock
        if (newStatus == MenuItemStatus.OutOfStock)
        {
            AddDomainEvent(new MenuItemOutOfStockEvent(
                Id,
                Name,
                Category,
                new List<Guid>(), // Commandes affectées - à remplir depuis le contexte
                UpdatedAt.Value));
        }
    }

    /// <summary>
    /// Configurer les propriétés diététiques
    /// </summary>
    public void ConfigureDietaryProperties(
        bool? isVegetarian = null,
        bool? isVegan = null,
        bool? isGlutenFree = null,
        bool? isSpicy = null,
        List<AllergenType>? allergens = null,
        List<string>? dietaryRestrictions = null)
    {
        if (isVegetarian.HasValue)
            IsVegetarian = isVegetarian.Value;

        if (isVegan.HasValue)
        {
            IsVegan = isVegan.Value;
            if (IsVegan)
                IsVegetarian = true; // Végan implique végétarien
        }

        if (isGlutenFree.HasValue)
            IsGlutenFree = isGlutenFree.Value;

        if (isSpicy.HasValue)
            IsSpicy = isSpicy.Value;

        if (allergens != null)
            Allergens = allergens.ToList();

        if (dietaryRestrictions != null)
            DietaryRestrictions = dietaryRestrictions.ToList();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les horaires de disponibilité
    /// </summary>
    public void SetAvailabilitySchedule(
        TimeOnly? fromTime = null,
        TimeOnly? untilTime = null,
        List<DayOfWeek>? availableDays = null)
    {
        AvailableFromTime = fromTime;
        AvailableUntilTime = untilTime;
        
        if (availableDays != null)
            AvailableDays = availableDays.ToList();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer la gestion des stocks
    /// </summary>
    public void ConfigureStockManagement(
        List<Guid> requiredStockItems,
        bool requiresStockCheck = true,
        int? minimumStockLevel = null)
    {
        RequiredStockItems = requiredStockItems?.ToList() ?? new List<Guid>();
        RequiresStockCheck = requiresStockCheck;
        MinimumStockLevel = minimumStockLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les options de personnalisation
    /// </summary>
    public void ConfigureCustomization(
        bool allowsCustomization = true,
        string? customizationInstructions = null,
        decimal? customizationUpcharge = null)
    {
        AllowsCustomization = allowsCustomization;
        CustomizationInstructions = customizationInstructions?.Trim();
        CustomizationUpcharge = customizationUpcharge;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter une variation de l'élément
    /// </summary>
    public void AddVariation(string name, MenuPrice price, string? description = null, NutritionalInfo? nutritionalInfo = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variation name cannot be null or empty", nameof(name));

        if (_variations.Any(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Variation '{name}' already exists");

        var variation = new MenuItemVariation(Id, name, price, description, nutritionalInfo);
        _variations.Add(variation);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer une variation
    /// </summary>
    public void RemoveVariation(Guid variationId)
    {
        var variation = _variations.FirstOrDefault(v => v.Id == variationId);
        if (variation != null)
        {
            _variations.Remove(variation);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Démarrer une promotion
    /// </summary>
    public void StartPromotion(
        string promotionName,
        MenuPrice promotionalPrice,
        DateTime startDate,
        DateTime endDate,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(promotionName))
            throw new ArgumentException("Promotion name cannot be null or empty", nameof(promotionName));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        var promotion = new MenuItemPromotion(Id, promotionName, promotionalPrice, startDate, endDate, description);
        _promotions.Add(promotion);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemPromotionStartedEvent(
            promotion.Id,
            Id,
            Price,
            promotionalPrice,
            startDate,
            endDate,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Terminer une promotion
    /// </summary>
    public void EndPromotion(Guid promotionId)
    {
        var promotion = _promotions.FirstOrDefault(p => p.Id == promotionId);
        if (promotion != null)
        {
            promotion.End();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Enregistrer une commande
    /// </summary>
    public void RecordOrder(decimal orderValue)
    {
        OrderCount++;
        Revenue += orderValue;
        LastOrdered = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Marquer comme populaire si plus de 50 commandes
        if (OrderCount >= 50 && !IsPopular)
        {
            IsPopular = true;
        }
    }

    /// <summary>
    /// Ajouter une évaluation
    /// </summary>
    public void AddRating(Rating rating)
    {
        if (rating == null)
            throw new ArgumentNullException(nameof(rating));

        // Calcul simple de la moyenne - en production, on utiliserait une base de données
        if (AverageRating.HasValue)
        {
            var totalScore = AverageRating.Value * ReviewCount + rating.Score;
            ReviewCount++;
            AverageRating = totalScore / ReviewCount;
        }
        else
        {
            AverageRating = rating.Score;
            ReviewCount = 1;
        }

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MenuItemRatedEvent(
            Id,
            Guid.Empty, // Customer ID serait passé en paramètre
            rating,
            rating.Comment,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Marquer comme nouveau
    /// </summary>
    public void MarkAsNew(bool isNew = true)
    {
        IsNew = isNew;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si l'élément est actuellement disponible
    /// </summary>
    public bool IsCurrentlyAvailable
    {
        get
        {
            if (!IsAvailable || Status != MenuItemStatus.Available)
                return false;

            var now = DateTime.UtcNow;
            
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
    /// Obtenir le prix effectif (avec promotions actives)
    /// </summary>
    public MenuPrice GetEffectivePrice()
    {
        var activePromotion = _promotions.FirstOrDefault(p => p.IsActive);
        return activePromotion?.PromotionalPrice ?? Price;
    }

    /// <summary>
    /// Vérifier si l'élément a des restrictions diététiques
    /// </summary>
    public bool HasDietaryRestrictions => Allergens.Any() || DietaryRestrictions.Any();

    /// <summary>
    /// Vérifier si l'élément convient à un régime spécifique
    /// </summary>
    public bool IsSuitableForDiet(string diet)
    {
        return diet.ToLowerInvariant() switch
        {
            "vegetarian" => IsVegetarian,
            "vegan" => IsVegan,
            "gluten-free" => IsGlutenFree,
            _ => !DietaryRestrictions.Contains(diet, StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    /// Calculer le score de popularité
    /// </summary>
    public decimal CalculatePopularityScore()
    {
        var baseScore = OrderCount * 0.3m;
        var revenueScore = Revenue * 0.0001m;
        var ratingScore = (AverageRating ?? 0) * ReviewCount * 0.1m;
        var recencyScore = LastOrdered.HasValue && LastOrdered.Value > DateTime.UtcNow.AddDays(-30) ? 10 : 0;

        return baseScore + revenueScore + ratingScore + recencyScore;
    }

    private void ConfigureByCategory(MenuCategory category)
    {
        switch (category)
        {
            case MenuCategory.Wine:
            case MenuCategory.Cocktail:
                RequiresAgeVerification = true;
                RequiresStockCheck = true;
                break;
            
            case MenuCategory.Vegetarian:
                IsVegetarian = true;
                break;
            
            case MenuCategory.Vegan:
                IsVegan = true;
                IsVegetarian = true;
                break;
            
            case MenuCategory.Coffee:
            case MenuCategory.Beverage:
                RequiresStockCheck = false; // Moins critique pour les boissons
                break;
        }

        // Ajouter des allergènes communs par catégorie
        var commonAllergens = category.GetCommonAllergens();
        foreach (var allergen in commonAllergens)
        {
            if (!Allergens.Contains(allergen))
                Allergens.Add(allergen);
        }
    }
}

/// <summary>
/// Variation d'un élément de menu (taille, préparation, etc.)
/// </summary>
public sealed class MenuItemVariation : Entity
{
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MenuPrice Price { get; private set; }
    public NutritionalInfo? NutritionalInfo { get; private set; }
    public bool IsAvailable { get; private set; }
    public int DisplayOrder { get; private set; }

    private MenuItemVariation() { } // EF Constructor

    public MenuItemVariation(
        Guid menuItemId,
        string name,
        MenuPrice price,
        string? description = null,
        NutritionalInfo? nutritionalInfo = null)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        Name = name.Trim();
        Description = description?.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        NutritionalInfo = nutritionalInfo;
        IsAvailable = true;
        DisplayOrder = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(MenuPrice newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Promotion sur un élément de menu
/// </summary>
public sealed class MenuItemPromotion : Entity
{
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public MenuPrice PromotionalPrice { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? EndedAt { get; private set; }

    private MenuItemPromotion() { } // EF Constructor

    public MenuItemPromotion(
        Guid menuItemId,
        string name,
        MenuPrice promotionalPrice,
        DateTime startDate,
        DateTime endDate,
        string? description = null)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        Name = name.Trim();
        Description = description?.Trim();
        PromotionalPrice = promotionalPrice ?? throw new ArgumentNullException(nameof(promotionalPrice));
        StartDate = startDate;
        EndDate = endDate;
        IsActive = DateTime.UtcNow >= startDate && DateTime.UtcNow <= endDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void End()
    {
        IsActive = false;
        EndedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCurrentlyActive => IsActive && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}