using Stock.Domain.Entities;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Interfaces;

/// <summary>
/// Repository pour les mouvements de stock
/// </summary>
public interface IStockMovementRepository
{
    /// <summary>
    /// Obtenir un mouvement par son ID
    /// </summary>
    Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les mouvements d'un produit
    /// </summary>
    Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les mouvements d'un emplacement
    /// </summary>
    Task<IReadOnlyList<StockMovement>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les mouvements par référence
    /// </summary>
    Task<IReadOnlyList<StockMovement>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les mouvements dans une période
    /// </summary>
    Task<IReadOnlyList<StockMovement>> GetByPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un mouvement
    /// </summary>
    Task<StockMovement> AddAsync(StockMovement movement, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer le stock actuel d'un produit dans un emplacement
    /// </summary>
    Task<StockQuantity> CalculateCurrentStockAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer l'historique des stocks
    /// </summary>
    Task<IReadOnlyList<(DateTime Date, StockQuantity Quantity)>> GetStockHistoryAsync(
        Guid productId, 
        Guid locationId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les réservations de stock
/// </summary>
public interface IStockReservationRepository
{
    /// <summary>
    /// Obtenir une réservation par son ID
    /// </summary>
    Task<StockReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations d'un produit
    /// </summary>
    Task<IReadOnlyList<StockReservation>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations par référence
    /// </summary>
    Task<IReadOnlyList<StockReservation>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations actives d'un produit
    /// </summary>
    Task<IReadOnlyList<StockReservation>> GetActiveReservationsAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations expirées
    /// </summary>
    Task<IReadOnlyList<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter une réservation
    /// </summary>
    Task<StockReservation> AddAsync(StockReservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour une réservation
    /// </summary>
    Task UpdateAsync(StockReservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer la quantité totale réservée
    /// </summary>
    Task<StockQuantity> CalculateTotalReservedAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les bons de commande
/// </summary>
public interface IPurchaseOrderRepository
{
    /// <summary>
    /// Obtenir un bon de commande par son ID
    /// </summary>
    Task<PurchaseOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir un bon de commande par son numéro
    /// </summary>
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les bons de commande d'un fournisseur
    /// </summary>
    Task<IReadOnlyList<PurchaseOrder>> GetBySupplierId(Guid supplierId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les bons de commande en attente
    /// </summary>
    Task<IReadOnlyList<PurchaseOrder>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les bons de commande en retard
    /// </summary>
    Task<IReadOnlyList<PurchaseOrder>> GetOverdueOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un bon de commande
    /// </summary>
    Task<PurchaseOrder> AddAsync(PurchaseOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un bon de commande
    /// </summary>
    Task UpdateAsync(PurchaseOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Générer le prochain numéro de commande
    /// </summary>
    Task<string> GenerateNextOrderNumberAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les fournisseurs
/// </summary>
public interface ISupplierRepository
{
    /// <summary>
    /// Obtenir un fournisseur par son ID
    /// </summary>
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir un fournisseur par son code
    /// </summary>
    Task<Supplier?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir tous les fournisseurs actifs
    /// </summary>
    Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rechercher des fournisseurs
    /// </summary>
    Task<IReadOnlyList<Supplier>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un fournisseur
    /// </summary>
    Task<Supplier> AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un fournisseur
    /// </summary>
    Task UpdateAsync(Supplier supplier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier si un code existe déjà
    /// </summary>
    Task<bool> ExistsWithCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les emplacements
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Obtenir un emplacement par son ID
    /// </summary>
    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir un emplacement par son code
    /// </summary>
    Task<Location?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir tous les emplacements actifs
    /// </summary>
    Task<IReadOnlyList<Location>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les emplacements par type
    /// </summary>
    Task<IReadOnlyList<Location>> GetByTypeAsync(Enums.LocationType locationType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rechercher des emplacements
    /// </summary>
    Task<IReadOnlyList<Location>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un emplacement
    /// </summary>
    Task<Location> AddAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un emplacement
    /// </summary>
    Task UpdateAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier si un code existe déjà
    /// </summary>
    Task<bool> ExistsWithCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de domaine pour la gestion des stocks
/// </summary>
public interface IStockDomainService
{
    /// <summary>
    /// Vérifier la disponibilité d'un stock
    /// </summary>
    Task<bool> IsStockAvailableAsync(Guid productId, Guid locationId, StockQuantity requiredQuantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer le stock disponible (stock actuel - réservations)
    /// </summary>
    Task<StockQuantity> CalculateAvailableStockAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier les seuils d'alerte
    /// </summary>
    Task<IReadOnlyList<(Guid ProductId, Guid LocationId, StockQuantity CurrentStock, StockQuantity ThresholdStock)>> 
        CheckStockAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Effectuer un transfert de stock entre emplacements
    /// </summary>
    Task<(StockMovement FromMovement, StockMovement ToMovement)> TransferStockAsync(
        Guid productId,
        Guid fromLocationId,
        Guid toLocationId,
        StockQuantity quantity,
        string? transferReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valider un mouvement de stock
    /// </summary>
    Task ValidateStockMovementAsync(StockMovement movement, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer la valorisation du stock
    /// </summary>
    Task<Money> CalculateStockValuationAsync(Guid productId, Guid locationId, Enums.ValuationMethod method, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de notification pour les alertes de stock
/// </summary>
public interface IStockNotificationService
{
    /// <summary>
    /// Envoyer une alerte de stock faible
    /// </summary>
    Task SendLowStockAlertAsync(Guid productId, Guid locationId, StockQuantity currentQuantity, StockQuantity thresholdQuantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer une alerte de rupture de stock
    /// </summary>
    Task SendStockOutAlertAsync(Guid productId, Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer une alerte de surstock
    /// </summary>
    Task SendOverstockAlertAsync(Guid productId, Guid locationId, StockQuantity currentQuantity, StockQuantity maxQuantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer une notification de réservation expirée
    /// </summary>
    Task SendReservationExpiredNotificationAsync(Guid reservationId, Guid productId, StockQuantity expiredQuantity, CancellationToken cancellationToken = default);
}