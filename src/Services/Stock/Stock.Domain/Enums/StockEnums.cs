namespace Stock.Domain.Enums;

/// <summary>
/// Types de mouvements de stock pour traçabilité complète
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Entrée de stock - réception marchandise
    /// </summary>
    In = 1,
    Inbound = 1, // Alias pour compatibilité

    /// <summary>
    /// Sortie de stock - vente ou consommation
    /// </summary>
    Out = 2,
    Outbound = 2, // Alias pour compatibilité

    /// <summary>
    /// Ajustement positif - correction d'inventaire
    /// </summary>
    AdjustmentIn = 3,

    /// <summary>
    /// Ajustement négatif - correction d'inventaire
    /// </summary>
    AdjustmentOut = 4,

    /// <summary>
    /// Transfert entre emplacements
    /// </summary>
    Transfer = 5,
    TransferOut = 5, // Alias pour compatibilité

    /// <summary>
    /// Réservation pour commande
    /// </summary>
    Reservation = 6,

    /// <summary>
    /// Libération de réservation
    /// </summary>
    ReservationRelease = 7,

    /// <summary>
    /// Retour client
    /// </summary>
    Return = 8,

    /// <summary>
    /// Perte ou casse
    /// </summary>
    Loss = 9,

    /// <summary>
    /// Péremption produit
    /// </summary>
    Expiry = 10
}

/// <summary>
/// Status des réservations de stock
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Réservation active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Réservation confirmée et consommée
    /// </summary>
    Consumed = 2,
    Confirmed = 2, // Alias pour compatibilité

    /// <summary>
    /// Réservation libérée/annulée
    /// </summary>
    Released = 3,
    Cancelled = 3, // Alias pour compatibilité

    /// <summary>
    /// Réservation expirée automatiquement
    /// </summary>
    Expired = 4
}

/// <summary>
/// Types d'emplacements de stockage
/// </summary>
public enum LocationType
{
    /// <summary>
    /// Magasin principal
    /// </summary>
    Store = 1,

    /// <summary>
    /// Entrepôt
    /// </summary>
    Warehouse = 2,

    /// <summary>
    /// Zone de réception
    /// </summary>
    Receiving = 3,

    /// <summary>
    /// Zone d'expédition
    /// </summary>
    Shipping = 4,

    /// <summary>
    /// Zone de retour
    /// </summary>
    Returns = 5,

    /// <summary>
    /// Zone de quarantaine
    /// </summary>
    Quarantine = 6,

    /// <summary>
    /// Cuisine (spécifique restaurant)
    /// </summary>
    Kitchen = 7,

    /// <summary>
    /// Bar (spécifique restaurant)
    /// </summary>
    Bar = 8,

    /// <summary>
    /// Stockage congelé
    /// </summary>
    FrozenStorage = 9,

    /// <summary>
    /// Stockage froid
    /// </summary>
    ColdStorage = 10,

    /// <summary>
    /// Stockage sec
    /// </summary>
    DryStorage = 11,

    /// <summary>
    /// Zone de production
    /// </summary>
    Production = 12,

    /// <summary>
    /// Zone produits endommagés
    /// </summary>
    Damaged = 13
}

/// <summary>
/// Statuts des commandes fournisseurs
/// </summary>
public enum PurchaseOrderStatus
{
    /// <summary>
    /// Brouillon en cours de création
    /// </summary>
    Draft = 1,

    /// <summary>
    /// En attente d'approbation
    /// </summary>
    PendingApproval = 2,

    /// <summary>
    /// Approuvée et envoyée au fournisseur
    /// </summary>
    Approved = 3,
    Confirmed = 3, // Alias pour compatibilité
    Sent = 3, // Alias pour compatibilité

    /// <summary>
    /// Partiellement reçue
    /// </summary>
    PartiallyReceived = 4,

    /// <summary>
    /// Complètement reçue
    /// </summary>
    Received = 5,

    /// <summary>
    /// Annulée
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Fermée
    /// </summary>
    Closed = 7
}

/// <summary>
/// Méthodes de valorisation du stock
/// </summary>
public enum ValuationMethod
{
    /// <summary>
    /// Premier entré, premier sorti
    /// </summary>
    FIFO = 1,

    /// <summary>
    /// Dernier entré, premier sorti
    /// </summary>
    LIFO = 2,

    /// <summary>
    /// Coût moyen pondéré
    /// </summary>
    WeightedAverage = 3,

    /// <summary>
    /// Coût standard
    /// </summary>
    StandardCost = 4
}

/// <summary>
/// Types d'alertes de stock
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Stock faible
    /// </summary>
    LowStock = 1,

    /// <summary>
    /// Rupture de stock
    /// </summary>
    OutOfStock = 2,
    StockOut = 2, // Alias pour compatibilité

    /// <summary>
    /// Surstock
    /// </summary>
    Overstock = 3,

    /// <summary>
    /// Produit à péremption proche
    /// </summary>
    NearExpiry = 4,

    /// <summary>
    /// Produit périmé
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Mouvement suspect détecté
    /// </summary>
    SuspiciousMovement = 6,

    /// <summary>
    /// Point de récommande atteint
    /// </summary>
    ReorderPoint = 7,

    /// <summary>
    /// Stock de sécurité atteint
    /// </summary>
    SafetyStock = 8
}

/// <summary>
/// Extensions pour les énumérations
/// </summary>
public static class StockEnumExtensions
{
    /// <summary>
    /// Vérifier si le mouvement augmente le stock
    /// </summary>
    public static bool IncreasesStock(this MovementType movementType)
    {
        return movementType is MovementType.In or MovementType.Inbound or MovementType.AdjustmentIn or 
               MovementType.Return or MovementType.ReservationRelease;
    }

    /// <summary>
    /// Vérifier si le mouvement diminue le stock
    /// </summary>
    public static bool DecreasesStock(this MovementType movementType)
    {
        return movementType is MovementType.Out or MovementType.Outbound or MovementType.AdjustmentOut or 
               MovementType.Reservation or MovementType.Loss or MovementType.Expiry;
    }

    /// <summary>
    /// Vérifier si le mouvement est entrant
    /// </summary>
    public static bool IsInbound(this MovementType movementType)
    {
        return movementType is MovementType.In or MovementType.Inbound or MovementType.AdjustmentIn or 
               MovementType.Return or MovementType.ReservationRelease;
    }

    /// <summary>
    /// Vérifier si le mouvement est sortant
    /// </summary>
    public static bool IsOutbound(this MovementType movementType)
    {
        return movementType is MovementType.Out or MovementType.Outbound or MovementType.AdjustmentOut or 
               MovementType.Reservation or MovementType.Loss or MovementType.Expiry or MovementType.Transfer or MovementType.TransferOut;
    }

    /// <summary>
    /// Vérifier si la commande peut être modifiée
    /// </summary>
    public static bool CanBeModified(this PurchaseOrderStatus status)
    {
        return status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.PendingApproval;
    }

    /// <summary>
    /// Vérifier si la commande est terminée
    /// </summary>
    public static bool IsCompleted(this PurchaseOrderStatus status)
    {
        return status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled or 
               PurchaseOrderStatus.Closed;
    }
}