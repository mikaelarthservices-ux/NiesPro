namespace Restaurant.Domain.Enums;

/// <summary>
/// Types de service/shift de travail
/// </summary>
public enum ShiftType
{
    Morning = 1,    // Service du matin
    Lunch = 2,      // Service du midi
    Dinner = 3,     // Service du soir
    Night = 4,      // Service de nuit
    DoubleShift = 5 // Double service
}

/// <summary>
/// Statut du planning du personnel
/// </summary>
public enum ScheduleStatus
{
    Scheduled = 1,  // Planifié
    CheckedIn = 2,  // Arrivé
    CheckedOut = 3, // Parti
    Absent = 4,     // Absent
    Late = 5,       // En retard
    EarlyLeave = 6  // Parti tôt
}

/// <summary>
/// Types de formation
/// </summary>
public enum TrainingType
{
    Orientation = 1,        // Formation d'orientation
    SafetyTraining = 2,     // Formation sécurité
    FoodSafety = 3,         // Sécurité alimentaire
    CustomerService = 4,    // Service client
    TechnicalSkills = 5,    // Compétences techniques
    Leadership = 6,         // Leadership
    Communication = 7,      // Communication
    ProductKnowledge = 8,   // Connaissance produit
    EquipmentTraining = 9,  // Formation équipement
    ComplianceTraining = 10 // Formation conformité
}

/// <summary>
/// Statut de la formation
/// </summary>
public enum TrainingStatus
{
    Scheduled = 1,  // Planifiée
    InProgress = 2, // En cours
    Completed = 3,  // Terminée
    Failed = 4,     // Échouée
    Cancelled = 5,  // Annulée
    Expired = 6     // Expirée
}

/// <summary>
/// Statut d'un item de commande de cuisine
/// </summary>
public enum KitchenOrderItemStatus
{
    Pending = 1,        // En attente
    Accepted = 2,       // Accepté
    InPreparation = 3,  // En préparation
    Ready = 4,          // Prêt
    Served = 5,         // Servi
    Cancelled = 6,      // Annulé
    OnHold = 7          // En attente
}

/// <summary>
/// Actions possibles sur une commande de cuisine
/// </summary>
public enum KitchenOrderAction
{
    Created = 1,            // Créée
    Accepted = 2,           // Acceptée
    Started = 3,            // Commencée
    ItemCompleted = 4,      // Item terminé
    Ready = 5,              // Prête
    Served = 6,             // Servie
    Completed = 7,          // Terminée
    Cancelled = 8,          // Annulée
    Modified = 9,           // Modifiée
    ItemAdded = 10,         // Item ajouté
    ItemRemoved = 11,       // Item retiré
    ItemModified = 12,      // Item modifié
    PriorityChanged = 13,   // Priorité changée
    Delayed = 14,           // Retardée
    QualityCheck = 15       // Contrôle qualité
}