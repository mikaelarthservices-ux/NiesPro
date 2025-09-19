namespace Restaurant.Domain.Enums;

/// <summary>
/// Statut d'une table de restaurant
/// </summary>
public enum TableStatus
{
    Available = 1,      // Table disponible
    Reserved = 2,       // Table réservée
    Occupied = 3,       // Table occupée
    Cleaning = 4,       // Table en nettoyage
    OutOfService = 5,   // Table hors service
    Maintenance = 6     // Table en maintenance
}

/// <summary>
/// Statut d'une réservation de table
/// </summary>
public enum ReservationStatus
{
    Pending = 1,        // Réservation en attente
    Confirmed = 2,      // Réservation confirmée
    Seated = 3,         // Client installé
    NoShow = 4,         // Client absent
    Cancelled = 5,      // Réservation annulée
    Completed = 6       // Réservation terminée
}

/// <summary>
/// Statut d'une commande cuisine
/// </summary>
public enum KitchenOrderStatus
{
    Pending = 1,        // En attente
    Received = 2,       // Reçue en cuisine
    InPreparation = 3,  // En préparation
    Ready = 4,          // Prête
    Served = 5,         // Servie
    Cancelled = 6       // Annulée
}

/// <summary>
/// Priorité d'une commande cuisine
/// </summary>
public enum OrderPriority
{
    Low = 1,           // Basse priorité
    Normal = 2,        // Priorité normale
    High = 3,          // Haute priorité
    Urgent = 4         // Urgente
}

/// <summary>
/// Catégories de menu
/// </summary>
public enum MenuCategory
{
    Appetizer = 1,      // Entrée
    MainCourse = 2,     // Plat principal
    Dessert = 3,        // Dessert
    Beverage = 4,       // Boisson
    Wine = 5,           // Vin
    Coffee = 6,         // Café
    Cocktail = 7,       // Cocktail
    Salad = 8,          // Salade
    Soup = 9,           // Soupe
    Pasta = 10,         // Pâtes
    Pizza = 11,         // Pizza
    Meat = 12,          // Viande
    Fish = 13,          // Poisson
    Vegetarian = 14,    // Végétarien
    Vegan = 15,         // Végan
    Special = 16        // Spécialité
}

/// <summary>
/// Types de menu
/// </summary>
public enum MenuType
{
    ALaCarte = 1,       // À la carte
    SetMenu = 2,        // Menu fixe
    Buffet = 3,         // Buffet
    TastingMenu = 4,    // Menu dégustation
    KidsMenu = 5,       // Menu enfant
    LunchMenu = 6,      // Menu déjeuner
    DinnerMenu = 7,     // Menu dîner
    SeasonalMenu = 8,   // Menu saisonnier
    SpecialMenu = 9     // Menu spécial
}

/// <summary>
/// Statut d'un élément de menu
/// </summary>
public enum MenuItemStatus
{
    Available = 1,      // Disponible
    OutOfStock = 2,     // Rupture de stock
    Discontinued = 3,   // Arrêté
    Seasonal = 4,       // Saisonnier
    ComingSoon = 5      // Bientôt disponible
}

/// <summary>
/// Types d'allergènes
/// </summary>
public enum AllergenType
{
    Gluten = 1,         // Gluten
    Dairy = 2,          // Produits laitiers
    Eggs = 3,           // Œufs
    Fish = 4,           // Poisson
    Shellfish = 5,      // Crustacés
    Nuts = 6,           // Noix
    Peanuts = 7,        // Arachides
    Soy = 8,            // Soja
    Sesame = 9,         // Sésame
    Sulfites = 10,      // Sulfites
    Celery = 11,        // Céleri
    Mustard = 12,       // Moutarde
    Lupin = 13,         // Lupin
    Mollusks = 14       // Mollusques
}

/// <summary>
/// Types de personnel de restaurant
/// </summary>
public enum StaffType
{
    Manager = 1,        // Gérant
    Chef = 2,           // Chef cuisinier
    SousChef = 3,       // Sous-chef
    Cook = 4,           // Cuisinier
    Waiter = 5,         // Serveur
    Bartender = 6,      // Barman
    Hostess = 7,        // Hôtesse d'accueil
    Busser = 8,         // Commis de salle
    Dishwasher = 9,     // Plongeur
    Sommelier = 10,     // Sommelier
    Cashier = 11        // Caissier
}

/// <summary>
/// Statut du personnel
/// </summary>
public enum StaffStatus
{
    Active = 1,         // Actif
    OnBreak = 2,        // En pause
    OffDuty = 3,        // Hors service
    Sick = 4,           // Malade
    Vacation = 5,       // En congé
    Terminated = 6      // Licencié
}

/// <summary>
/// Types de zones de restaurant
/// </summary>
public enum RestaurantZone
{
    DiningRoom = 1,     // Salle à manger
    Bar = 2,            // Bar
    Terrace = 3,        // Terrasse
    PrivateRoom = 4,    // Salon privé
    Kitchen = 5,        // Cuisine
    Storage = 6,        // Stockage
    Office = 7,         // Bureau
    Restroom = 8,       // Toilettes
    Entry = 9,          // Entrée
    Smoking = 10,       // Zone fumeur
    NonSmoking = 11,    // Zone non-fumeur
    VIP = 12           // Zone VIP
}

/// <summary>
/// Types de service
/// </summary>
public enum ServiceType
{
    DineIn = 1,         // Sur place
    Takeaway = 2,       // À emporter
    Delivery = 3,       // Livraison
    Catering = 4,       // Traiteur
    Buffet = 5,         // Buffet
    FastFood = 6        // Restauration rapide
}

/// <summary>
/// Extensions pour les enums Restaurant
/// </summary>
public static class RestaurantEnumExtensions
{
    /// <summary>
    /// Vérifier si le statut de table permet les réservations
    /// </summary>
    public static bool AllowsReservation(this TableStatus status)
    {
        return status == TableStatus.Available;
    }

    /// <summary>
    /// Vérifier si le statut de table nécessite un nettoyage
    /// </summary>
    public static bool RequiresCleaning(this TableStatus status)
    {
        return status == TableStatus.Cleaning;
    }

    /// <summary>
    /// Vérifier si la réservation est active
    /// </summary>
    public static bool IsActive(this ReservationStatus status)
    {
        return status == ReservationStatus.Confirmed || 
               status == ReservationStatus.Seated;
    }

    /// <summary>
    /// Vérifier si la commande peut être modifiée
    /// </summary>
    public static bool CanBeModified(this KitchenOrderStatus status)
    {
        return status == KitchenOrderStatus.Pending || 
               status == KitchenOrderStatus.Received;
    }

    /// <summary>
    /// Vérifier si la commande est en cours
    /// </summary>
    public static bool IsInProgress(this KitchenOrderStatus status)
    {
        return status == KitchenOrderStatus.Received || 
               status == KitchenOrderStatus.InPreparation;
    }

    /// <summary>
    /// Vérifier si l'élément de menu est disponible
    /// </summary>
    public static bool IsAvailable(this MenuItemStatus status)
    {
        return status == MenuItemStatus.Available || 
               status == MenuItemStatus.Seasonal;
    }

    /// <summary>
    /// Vérifier si le personnel est en service
    /// </summary>
    public static bool IsOnDuty(this StaffStatus status)
    {
        return status == StaffStatus.Active || 
               status == StaffStatus.OnBreak;
    }

    /// <summary>
    /// Vérifier si le personnel peut prendre des commandes
    /// </summary>
    public static bool CanTakeOrders(this StaffType staffType)
    {
        return staffType == StaffType.Waiter || 
               staffType == StaffType.Manager || 
               staffType == StaffType.Hostess;
    }

    /// <summary>
    /// Vérifier si le personnel travaille en cuisine
    /// </summary>
    public static bool IsKitchenStaff(this StaffType staffType)
    {
        return staffType == StaffType.Chef || 
               staffType == StaffType.SousChef || 
               staffType == StaffType.Cook;
    }

    /// <summary>
    /// Obtenir la couleur associée à la priorité
    /// </summary>
    public static string GetColor(this OrderPriority priority)
    {
        return priority switch
        {
            OrderPriority.Low => "#28a745",      // Vert
            OrderPriority.Normal => "#007bff",   // Bleu
            OrderPriority.High => "#ffc107",     // Jaune
            OrderPriority.Urgent => "#dc3545",  // Rouge
            _ => "#6c757d"                       // Gris par défaut
        };
    }

    /// <summary>
    /// Obtenir l'icône associée à la catégorie de menu
    /// </summary>
    public static string GetIcon(this MenuCategory category)
    {
        return category switch
        {
            MenuCategory.Appetizer => "🥗",
            MenuCategory.MainCourse => "🍽️",
            MenuCategory.Dessert => "🍰",
            MenuCategory.Beverage => "🥤",
            MenuCategory.Wine => "🍷",
            MenuCategory.Coffee => "☕",
            MenuCategory.Cocktail => "🍸",
            MenuCategory.Salad => "🥙",
            MenuCategory.Soup => "🍲",
            MenuCategory.Pasta => "🍝",
            MenuCategory.Pizza => "🍕",
            MenuCategory.Meat => "🥩",
            MenuCategory.Fish => "🐟",
            MenuCategory.Vegetarian => "🥬",
            MenuCategory.Vegan => "🌱",
            MenuCategory.Special => "⭐",
            _ => "🍴"
        };
    }

    /// <summary>
    /// Obtenir les allergènes courants par catégorie
    /// </summary>
    public static IEnumerable<AllergenType> GetCommonAllergens(this MenuCategory category)
    {
        return category switch
        {
            MenuCategory.Pasta => new[] { AllergenType.Gluten, AllergenType.Eggs },
            MenuCategory.Pizza => new[] { AllergenType.Gluten, AllergenType.Dairy },
            MenuCategory.Fish => new[] { AllergenType.Fish },
            MenuCategory.Dessert => new[] { AllergenType.Gluten, AllergenType.Dairy, AllergenType.Eggs },
            MenuCategory.Wine => new[] { AllergenType.Sulfites },
            _ => Array.Empty<AllergenType>()
        };
    }

    /// <summary>
    /// Vérifier si la zone permet le service
    /// </summary>
    public static bool AllowsService(this RestaurantZone zone)
    {
        return zone == RestaurantZone.DiningRoom || 
               zone == RestaurantZone.Bar || 
               zone == RestaurantZone.Terrace || 
               zone == RestaurantZone.PrivateRoom || 
               zone == RestaurantZone.VIP;
    }

    /// <summary>
    /// Obtenir le temps de préparation estimé par catégorie (en minutes)
    /// </summary>
    public static int GetEstimatedPreparationTime(this MenuCategory category)
    {
        return category switch
        {
            MenuCategory.Beverage => 2,
            MenuCategory.Coffee => 3,
            MenuCategory.Cocktail => 5,
            MenuCategory.Appetizer => 8,
            MenuCategory.Salad => 5,
            MenuCategory.Soup => 3,
            MenuCategory.Pasta => 12,
            MenuCategory.Pizza => 15,
            MenuCategory.MainCourse => 20,
            MenuCategory.Meat => 25,
            MenuCategory.Fish => 18,
            MenuCategory.Dessert => 8,
            MenuCategory.Special => 30,
            _ => 15
        };
    }
}