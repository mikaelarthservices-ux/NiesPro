using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using NiesPro.Contracts.Common;
using NiesPro.Contracts.Primitives;

namespace Order.Domain.Interfaces;

/// <summary>
/// Interface Enterprise pour orchestration des workflows multi-contexte
/// Architecture event-driven alignée sur les standards NiesPro
/// </summary>
public interface IOrderOrchestrationService
{
    /// <summary>
    /// Initie le workflow approprié selon le contexte métier
    /// </summary>
    Task<OrchestrationResult> InitiateWorkflowAsync(
        Guid orderId, 
        BusinessContext context, 
        ServiceInfo serviceInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exécute une transition de statut avec validation métier
    /// </summary>
    Task<OrchestrationResult> ExecuteStatusTransitionAsync(
        Guid orderId,
        OrderStatus targetStatus,
        string? reason = null,
        Guid? performedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intégration Restaurant - Création commande cuisine
    /// </summary>
    Task<KitchenOrderResult> TriggerKitchenWorkflowAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intégration Restaurant - Mise à jour depuis cuisine
    /// </summary>
    Task<OrchestrationResult> UpdateFromKitchenAsync(
        Guid orderId,
        KitchenOrderStatus kitchenStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intégration Boutique - Réservation stock
    /// </summary>
    Task<InventoryReservationResult> ReserveInventoryAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intégration Payment - Autorisation paiement
    /// </summary>
    Task<PaymentAuthorizationResult> AuthorizePaymentAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Intégration Notification - Envoi notifications contextuelles
    /// </summary>
    Task<NotificationResult> SendContextualNotificationAsync(
        Guid orderId,
        OrderStatus status,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le moteur de workflow Enterprise
/// </summary>
public interface IOrderWorkflowEngine
{
    /// <summary>
    /// Exécute une transition avec validation des règles métier
    /// </summary>
    Task<WorkflowResult> ExecuteTransitionAsync(
        Guid orderId,
        OrderStatus targetStatus,
        WorkflowContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtient les transitions valides pour un ordre donné
    /// </summary>
    Task<IEnumerable<OrderStatus>> GetValidTransitionsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre un hook de workflow personnalisé
    /// </summary>
    Task RegisterWorkflowHookAsync<T>(
        WorkflowHook<T> hook,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Valide si une transition est autorisée
    /// </summary>
    Task<bool> ValidateTransitionAsync(
        Guid orderId,
        OrderStatus targetStatus,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour les intégrations Restaurant
/// </summary>
public interface IRestaurantIntegrationService
{
    /// <summary>
    /// Crée une commande cuisine depuis un ordre
    /// </summary>
    Task<KitchenOrderResult> CreateKitchenOrderAsync(
        Guid orderId,
        IEnumerable<MenuItem> items,
        ServiceInfo serviceInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Met à jour le statut ordre depuis la cuisine
    /// </summary>
    Task<OrchestrationResult> UpdateOrderStatusAsync(
        Guid orderId,
        KitchenOrderStatus kitchenStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigne une table à un ordre
    /// </summary>
    Task<TableAssignmentResult> AssignTableAsync(
        Guid orderId,
        int tableNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigne un serveur à un ordre
    /// </summary>
    Task<WaiterAssignmentResult> AssignWaiterAsync(
        Guid orderId,
        Guid waiterId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour les métriques business Enterprise
/// </summary>
public interface IOrderMetricsService
{
    /// <summary>
    /// Enregistre la latence d'un ordre
    /// </summary>
    Task TrackOrderLatencyAsync(
        Guid orderId,
        TimeSpan duration,
        BusinessContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Incrémente le volume par contexte
    /// </summary>
    Task IncrementOrderVolumeByContextAsync(
        BusinessContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enregistre les métriques de revenus
    /// </summary>
    Task TrackRevenueMetricsAsync(
        Guid orderId,
        decimal revenue,
        BusinessContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère un rapport de performance
    /// </summary>
    Task<OrderPerformanceReport> GenerateReportAsync(
        DateRange period,
        BusinessContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtient les métriques temps réel
    /// </summary>
    Task<RealTimeMetrics> GetRealTimeMetricsAsync(
        BusinessContext? context = null,
        CancellationToken cancellationToken = default);
}

// DTOs et types de résultats

/// <summary>
/// Résultat d'orchestration Enterprise
/// </summary>
public record OrchestrationResult(
    bool Success,
    string? Message = null,
    IDictionary<string, object>? Metadata = null,
    Exception? Exception = null);

/// <summary>
/// Résultat de workflow
/// </summary>
public record WorkflowResult(
    bool Success,
    OrderStatus? NewStatus = null,
    string? Message = null,
    IEnumerable<string>? Warnings = null);

/// <summary>
/// Contexte de workflow
/// </summary>
public record WorkflowContext(
    BusinessContext BusinessContext,
    ServiceInfo ServiceInfo,
    Guid? PerformedBy = null,
    string? Reason = null,
    IDictionary<string, object>? AdditionalData = null);

/// <summary>
/// Hook de workflow personnalisable
/// </summary>
public record WorkflowHook<T>(
    string Name,
    Func<T, CancellationToken, Task<bool>> Handler,
    int Priority = 0) where T : class;

/// <summary>
/// Résultat de commande cuisine
/// </summary>
public record KitchenOrderResult(
    bool Success,
    Guid? KitchenOrderId = null,
    string? OrderNumber = null,
    string? Message = null);

/// <summary>
/// Résultat de réservation inventaire
/// </summary>
public record InventoryReservationResult(
    bool Success,
    Guid? ReservationId = null,
    TimeSpan? ReservationDuration = null,
    string? Message = null);

/// <summary>
/// Résultat d'autorisation paiement
/// </summary>
public record PaymentAuthorizationResult(
    bool Success,
    string? AuthorizationId = null,
    decimal? AuthorizedAmount = null,
    string? Message = null);

/// <summary>
/// Résultat de notification
/// </summary>
public record NotificationResult(
    bool Success,
    IEnumerable<string>? SentChannels = null,
    string? Message = null);

/// <summary>
/// Statuts de commande cuisine (mapping Restaurant.API)
/// </summary>
public enum KitchenOrderStatus
{
    Pending = 1,
    Received = 2,
    InPreparation = 3,
    Ready = 4,
    Served = 5,
    Cancelled = 6
}

/// <summary>
/// Element de menu (mapping Restaurant.API)
/// </summary>
public record MenuItem(
    Guid Id,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? SpecialRequests = null);

/// <summary>
/// Commande cuisine (mapping Restaurant.API)
/// </summary>
public record KitchenOrder(
    Guid Id,
    string OrderNumber,
    Guid TableId,
    IEnumerable<MenuItem> Items,
    KitchenOrderStatus Status);

/// <summary>
/// Assignment de table
/// </summary>
public record TableAssignment(
    Guid OrderId,
    int TableNumber,
    DateTime AssignedAt,
    Guid? AssignedBy = null);

/// <summary>
/// Assignment de serveur
/// </summary>
public record WaiterAssignment(
    Guid OrderId,
    Guid WaiterId,
    string WaiterName,
    DateTime AssignedAt);

/// <summary>
/// Résultat d'assignment de table
/// </summary>
public record TableAssignmentResult(
    bool Success,
    TableAssignment? Assignment = null,
    string? Message = null);

/// <summary>
/// Résultat d'assignment de serveur
/// </summary>
public record WaiterAssignmentResult(
    bool Success,
    WaiterAssignment? Assignment = null,
    string? Message = null);

/// <summary>
/// Rapport de performance
/// </summary>
public record OrderPerformanceReport(
    DateRange Period,
    BusinessContext? Context,
    int TotalOrders,
    decimal TotalRevenue,
    TimeSpan AverageOrderTime,
    decimal CompletionRate,
    IDictionary<OrderStatus, int> StatusDistribution,
    IDictionary<BusinessContext, int>? ContextDistribution = null);

/// <summary>
/// Métriques temps réel
/// </summary>
public record RealTimeMetrics(
    int ActiveOrders,
    int OrdersToday,
    decimal RevenueToday,
    TimeSpan AverageOrderTime,
    IDictionary<BusinessContext, int> OrdersByContext);

/// <summary>
/// Période de dates
/// </summary>
public record DateRange(DateTime Start, DateTime End)
{
    public static DateRange Today => new(DateTime.Today, DateTime.Today.AddDays(1).AddTicks(-1));
    public static DateRange ThisWeek => new(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek), DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek));
    public static DateRange ThisMonth => new(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)));
};