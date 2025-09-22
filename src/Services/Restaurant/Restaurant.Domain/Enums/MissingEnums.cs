namespace Restaurant.Domain.Enums;

/// <summary>
/// Types de commande (dine-in, takeout, delivery)
/// </summary>
public enum OrderType
{
    DineIn = 1,         // Sur place
    Takeout = 2,        // À emporter
    TakeAway = 2,       // À emporter (alias)
    Delivery = 3,       // Livraison
    Catering = 4        // Traiteur
}

/// <summary>
/// Statut global d'une commande
/// </summary>
public enum OrderStatus
{
    Draft = 1,          // Brouillon
    Pending = 2,        // En attente
    Submitted = 2,      // Soumise (alias)
    Accepted = 3,       // Acceptée
    InPreparation = 3,  // En préparation (alias)
    Ready = 4,          // Prête
    Served = 5,         // Servie
    Completed = 6,      // Terminée
    Paid = 6,           // Payée (alias)
    Cancelled = 7       // Annulée
}

/// <summary>
/// Sections de cuisine
/// </summary>
public enum KitchenSection
{
    ColdKitchen = 1,    // Cuisine froide
    HotKitchen = 2,     // Cuisine chaude
    Hot = 2,            // Chaud (alias)
    Grill = 3,          // Grill
    Pastry = 4,         // Pâtisserie
    Bar = 5,            // Bar
    SaladStation = 6,   // Station salade
    Fryer = 7,          // Friteuse
    Oven = 8            // Four
}

/// <summary>
/// Niveau de qualité
/// </summary>
public enum QualityLevel
{
    Poor = 1,           // Mauvais
    Fair = 2,           // Moyen
    Good = 3,           // Bon
    VeryGood = 4,       // Très bon
    Excellent = 5       // Excellent
}

/// <summary>
/// Actions sur commande
/// </summary>
public enum OrderAction
{
    Created = 1,        // Créée
    Placed = 2,         // Placée
    Modified = 2,       // Modifiée (alias)
    Accepted = 3,       // Acceptée
    Cancelled = 3,      // Annulée (alias)
    Sent = 4,           // Envoyée
    PreparationStarted = 5, // Préparation commencée
    Received = 5,       // Reçue (alias)
    Started = 6,        // Commencée (alias)
    Ready = 7,          // Prête
    Completed = 7,      // Terminée (alias)
    Served = 8,         // Servie
    Paid = 9,           // Payée
    PriorityChanged = 10, // Priorité changée
    DiscountApplied = 11  // Remise appliquée
}

/// <summary>
/// Statut d'un élément de commande
/// </summary>
public enum ItemStatus
{
    Pending = 1,        // En attente
    InPreparation = 2,  // En préparation
    Ready = 3,          // Prêt
    Served = 4,         // Servi
    Cancelled = 5       // Annulé
}

/// <summary>
/// Rôles du personnel
/// </summary>
public enum StaffRole
{
    Manager = 1,        // Gérant
    Chef = 2,           // Chef cuisinier
    SousChef = 3,       // Sous-chef
    Cook = 4,           // Cuisinier
    Waiter = 5,         // Serveur
    Bartender = 6,      // Barman
    Hostess = 7,        // Hôtesse
    Busser = 8,         // Commis
    Dishwasher = 9,     // Plongeur
    Cashier = 10        // Caissier
}

/// <summary>
/// Types d'emploi
/// </summary>
public enum EmploymentType
{
    FullTime = 1,       // Temps plein
    PartTime = 2,       // Temps partiel
    Temporary = 3,      // Temporaire
    Seasonal = 4,       // Saisonnier
    Contract = 5,       // Contrat
    Intern = 6          // Stagiaire
}

/// <summary>
/// Départements
/// </summary>
public enum Department
{
    Kitchen = 1,        // Cuisine
    Service = 2,        // Service
    Bar = 3,            // Bar
    Management = 4,     // Direction
    Administration = 5, // Administration
    Maintenance = 6     // Maintenance
}

/// <summary>
/// Statut de shift
/// </summary>
public enum ShiftStatus
{
    Scheduled = 1,      // Planifié
    Started = 2,        // Commencé
    OnBreak = 3,        // En pause
    Completed = 4,      // Terminé
    Cancelled = 5       // Annulé
}

/// <summary>
/// Horaires de travail
/// </summary>
public enum WorkSchedule
{
    Morning = 1,        // Matin
    Afternoon = 2,      // Après-midi
    Evening = 3,        // Soir
    Night = 4,          // Nuit
    Split = 5,          // Coupé
    Flexible = 6        // Flexible
}

/// <summary>
/// Statut de travail
/// </summary>
public enum WorkingStatus
{
    Active = 1,         // Actif
    OnBreak = 2,        // En pause
    OffDuty = 3,        // Hors service
    Sick = 4,           // Malade
    Vacation = 5,       // En congé
    Suspended = 6       // Suspendu
}

/// <summary>
/// Compétences du personnel
/// </summary>
public enum StaffSkill
{
    Cooking = 1,        // Cuisine
    Serving = 2,        // Service
    Bartending = 3,     // Bar
    Management = 4,     // Gestion
    Languages = 5,      // Langues
    CustomerService = 6, // Service client
    Sales = 7,          // Vente
    Leadership = 8      // Leadership
}

/// <summary>
/// Niveaux de compétence
/// </summary>
public enum SkillLevel
{
    Beginner = 1,       // Débutant
    Intermediate = 2,   // Intermédiaire
    Advanced = 3,       // Avancé
    Expert = 4          // Expert
}

/// <summary>
/// Genre
/// </summary>
public enum Gender
{
    Male = 1,           // Homme
    Female = 2,         // Femme
    Other = 3,          // Autre
    PreferNotToSay = 4  // Préfère ne pas dire
}

/// <summary>
/// Permissions système
/// </summary>
public enum SystemPermission
{
    ViewOrders = 1,     // Voir commandes
    CreateOrders = 2,   // Créer commandes
    EditOrders = 3,     // Modifier commandes
    CancelOrders = 4,   // Annuler commandes
    ViewReports = 5,    // Voir rapports
    ManageStaff = 6,    // Gérer personnel
    ManageMenu = 7,     // Gérer menu
    ManageTables = 8,   // Gérer tables
    ProcessPayments = 9, // Traiter paiements
    SystemAdmin = 10    // Admin système
}

/// <summary>
/// Types de pause
/// </summary>
public enum BreakType
{
    Lunch = 1,          // Déjeuner
    Coffee = 2,         // Café
    Dinner = 3,         // Dîner
    Personal = 4,       // Personnel
    Smoke = 5           // Cigarette
}

/// <summary>
/// Types de congé
/// </summary>
public enum VacationType
{
    Paid = 1,           // Congé payé
    Unpaid = 2,         // Congé sans solde
    Sick = 3,           // Congé maladie
    Maternity = 4,      // Congé maternité
    Paternity = 5,      // Congé paternité
    Emergency = 6,      // Congé urgence
    Bereavement = 7     // Congé deuil
}

/// <summary>
/// Statut demande de congé
/// </summary>
public enum VacationRequestStatus
{
    Pending = 1,        // En attente
    Approved = 2,       // Approuvé
    Rejected = 3,       // Rejeté
    Cancelled = 4       // Annulé
}

/// <summary>
/// Types d'entrée de temps
/// </summary>
public enum TimeEntryType
{
    ClockIn = 1,        // Arrivée
    ClockOut = 2,       // Départ
    BreakStart = 3,     // Début pause
    BreakEnd = 4        // Fin pause
}

/// <summary>
/// Types d'avertissement
/// </summary>
public enum WarningType
{
    Verbal = 1,         // Verbal
    Written = 2,        // Écrit
    Final = 3,          // Final
    Disciplinary = 4    // Disciplinaire
}

/// <summary>
/// Localisation (pour les adresses)
/// </summary>
public enum Location
{
    OnSite = 1,         // Sur site
    Remote = 2,         // À distance
    Client = 3,         // Chez client
    Warehouse = 4       // Entrepôt
}