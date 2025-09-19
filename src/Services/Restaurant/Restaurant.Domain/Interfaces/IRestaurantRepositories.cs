using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Interfaces;

/// <summary>
/// Repository pour les tables de restaurant
/// </summary>
public interface ITableRepository
{
    /// <summary>
    /// Obtenir une table par son ID
    /// </summary>
    Task<Table?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir une table par son numéro
    /// </summary>
    Task<Table?> GetByNumberAsync(string tableNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir toutes les tables actives
    /// </summary>
    Task<IReadOnlyList<Table>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les tables par zone
    /// </summary>
    Task<IReadOnlyList<Table>> GetByZoneAsync(RestaurantZone zone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les tables par statut
    /// </summary>
    Task<IReadOnlyList<Table>> GetByStatusAsync(TableStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les tables disponibles pour une capacité donnée
    /// </summary>
    Task<IReadOnlyList<Table>> GetAvailableTablesAsync(int partySize, DateTime? reservationTime = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter une table
    /// </summary>
    Task<Table> AddAsync(Table table, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour une table
    /// </summary>
    Task UpdateAsync(Table table, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier si un numéro de table existe déjà
    /// </summary>
    Task<bool> ExistsWithNumberAsync(string tableNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les réservations de tables
/// </summary>
public interface ITableReservationRepository
{
    /// <summary>
    /// Obtenir une réservation par son ID
    /// </summary>
    Task<TableReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations d'un client
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations d'une table
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetByTableIdAsync(Guid tableId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations par statut
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations pour une période
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations actives
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetActiveReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les réservations en conflit pour une table
    /// </summary>
    Task<IReadOnlyList<TableReservation>> GetConflictingReservationsAsync(Guid tableId, DateTime startTime, DateTime endTime, Guid? excludeReservationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter une réservation
    /// </summary>
    Task<TableReservation> AddAsync(TableReservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour une réservation
    /// </summary>
    Task UpdateAsync(TableReservation reservation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les menus
/// </summary>
public interface IMenuRepository
{
    /// <summary>
    /// Obtenir un menu par son ID
    /// </summary>
    Task<Menu?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir un menu par son nom
    /// </summary>
    Task<Menu?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les menus actifs
    /// </summary>
    Task<IReadOnlyList<Menu>> GetActiveMenusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les menus par type
    /// </summary>
    Task<IReadOnlyList<Menu>> GetByTypeAsync(MenuType menuType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir le menu principal actuel
    /// </summary>
    Task<Menu?> GetCurrentMainMenuAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un menu
    /// </summary>
    Task<Menu> AddAsync(Menu menu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un menu
    /// </summary>
    Task UpdateAsync(Menu menu, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les éléments de menu
/// </summary>
public interface IMenuItemRepository
{
    /// <summary>
    /// Obtenir un élément par son ID
    /// </summary>
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les éléments d'un menu
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetByMenuIdAsync(Guid menuId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les éléments par catégorie
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(MenuCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les éléments disponibles
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetAvailableItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rechercher des éléments par nom
    /// </summary>
    Task<IReadOnlyList<MenuItem>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les éléments par allergènes
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetByAllergenAsync(AllergenType allergen, bool excludeAllergen = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un élément de menu
    /// </summary>
    Task<MenuItem> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un élément de menu
    /// </summary>
    Task UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les commandes cuisine
/// </summary>
public interface IKitchenOrderRepository
{
    /// <summary>
    /// Obtenir une commande par son ID
    /// </summary>
    Task<KitchenOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les commandes d'une table
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> GetByTableIdAsync(Guid tableId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les commandes par statut
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> GetByStatusAsync(KitchenOrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les commandes actives (en cours)
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> GetActiveOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les commandes par priorité
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> GetByPriorityAsync(OrderPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les commandes en retard
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> GetOverdueOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter une commande
    /// </summary>
    Task<KitchenOrder> AddAsync(KitchenOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour une commande
    /// </summary>
    Task UpdateAsync(KitchenOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les statistiques de performance de la cuisine
    /// </summary>
    Task<KitchenPerformanceStats> GetPerformanceStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour le personnel
/// </summary>
public interface IStaffRepository
{
    /// <summary>
    /// Obtenir un membre du personnel par son ID
    /// </summary>
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir le personnel par type
    /// </summary>
    Task<IReadOnlyList<Staff>> GetByTypeAsync(StaffType staffType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir le personnel actif
    /// </summary>
    Task<IReadOnlyList<Staff>> GetActiveStaffAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir le personnel en service
    /// </summary>
    Task<IReadOnlyList<Staff>> GetOnDutyStaffAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtenir les serveurs disponibles
    /// </summary>
    Task<IReadOnlyList<Staff>> GetAvailableWaitersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajouter un membre du personnel
    /// </summary>
    Task<Staff> AddAsync(Staff staff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mettre à jour un membre du personnel
    /// </summary>
    Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de domaine pour la gestion des réservations
/// </summary>
public interface IReservationDomainService
{
    /// <summary>
    /// Vérifier la disponibilité d'une table
    /// </summary>
    Task<bool> IsTableAvailableAsync(Guid tableId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Trouver les meilleures tables pour une réservation
    /// </summary>
    Task<IReadOnlyList<Table>> FindBestTablesAsync(int partySize, DateTime reservationTime, TimeSpan duration, RestaurantZone? preferredZone = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimiser l'attribution des tables
    /// </summary>
    Task<Dictionary<Guid, Guid>> OptimizeTableAssignmentsAsync(List<TableReservation> reservations, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer le temps d'attente estimé
    /// </summary>
    Task<TimeSpan> CalculateEstimatedWaitTimeAsync(int partySize, DateTime currentTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggérer des créneaux alternatifs
    /// </summary>
    Task<IReadOnlyList<DateTime>> SuggestAlternativeTimeSlotsAsync(int partySize, DateTime preferredTime, TimeSpan duration, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de domaine pour la gestion de la cuisine
/// </summary>
public interface IKitchenDomainService
{
    /// <summary>
    /// Calculer le temps de préparation estimé
    /// </summary>
    Task<TimeSpan> CalculateEstimatedPreparationTimeAsync(List<Guid> menuItemIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimiser l'ordre des commandes
    /// </summary>
    Task<IReadOnlyList<KitchenOrder>> OptimizeOrderSequenceAsync(List<KitchenOrder> orders, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier la disponibilité des ingrédients
    /// </summary>
    Task<Dictionary<Guid, bool>> CheckIngredientAvailabilityAsync(List<Guid> menuItemIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer la charge de travail de la cuisine
    /// </summary>
    Task<KitchenWorkload> CalculateKitchenWorkloadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Détecter les goulots d'étranglement
    /// </summary>
    Task<IReadOnlyList<KitchenBottleneck>> DetectBottlenecksAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de domaine pour la gestion du personnel
/// </summary>
public interface IStaffDomainService
{
    /// <summary>
    /// Assigner un serveur optimal à une table
    /// </summary>
    Task<Staff?> AssignOptimalWaiterAsync(Guid tableId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculer la charge de travail du personnel
    /// </summary>
    Task<Dictionary<Guid, StaffWorkload>> CalculateStaffWorkloadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimiser les affectations du personnel
    /// </summary>
    Task<Dictionary<Guid, List<Guid>>> OptimizeStaffAssignmentsAsync(List<Table> tables, List<Staff> availableStaff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Vérifier si le personnel minimum est présent
    /// </summary>
    Task<bool> IsMinimumStaffPresentAsync(ServiceType serviceType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service de notification pour les événements restaurant
/// </summary>
public interface IRestaurantNotificationService
{
    /// <summary>
    /// Notifier une nouvelle réservation
    /// </summary>
    Task NotifyNewReservationAsync(TableReservation reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifier l'arrivée imminente de clients
    /// </summary>
    Task NotifyUpcomingArrivalAsync(TableReservation reservation, TimeSpan timeUntilArrival, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifier qu'une commande est prête
    /// </summary>
    Task NotifyOrderReadyAsync(KitchenOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifier un retard de commande
    /// </summary>
    Task NotifyOrderDelayAsync(KitchenOrder order, TimeSpan delay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifier une rupture de stock
    /// </summary>
    Task NotifyItemOutOfStockAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifier un problème de service
    /// </summary>
    Task NotifyServiceIssueAsync(Guid tableId, string issueDescription, OrderPriority severity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistiques de performance de la cuisine
/// </summary>
public record KitchenPerformanceStats(
    TimeSpan AveragePreparationTime,
    decimal OrderCompletionRate,
    int TotalOrdersProcessed,
    int OrdersOnTime,
    int OrdersLate,
    TimeSpan AverageDelayTime
);

/// <summary>
/// Charge de travail de la cuisine
/// </summary>
public record KitchenWorkload(
    int ActiveOrders,
    int PendingOrders,
    TimeSpan EstimatedCompletionTime,
    decimal CapacityUtilization,
    List<string> Bottlenecks
);

/// <summary>
/// Goulot d'étranglement en cuisine
/// </summary>
public record KitchenBottleneck(
    string BottleneckType,
    string Description,
    OrderPriority Severity,
    TimeSpan EstimatedResolutionTime,
    List<Guid> AffectedOrderIds
);

/// <summary>
/// Charge de travail du personnel
/// </summary>
public record StaffWorkload(
    Guid StaffId,
    int AssignedTables,
    int ActiveOrders,
    decimal WorkloadPercentage,
    TimeSpan AverageServiceTime,
    bool IsOverloaded
);