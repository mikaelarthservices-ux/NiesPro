using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

/// <summary>
/// DTO pour créer une commande cuisine
/// </summary>
public record CreateKitchenOrderDto(
    Guid TableId,
    OrderType OrderType,
    List<CreateKitchenOrderItemDto> Items,
    Guid? CustomerId = null,
    Guid? WaiterId = null,
    string? SpecialInstructions = null);

/// <summary>
/// DTO pour créer un élément de commande
/// </summary>
public record CreateKitchenOrderItemDto(
    Guid MenuItemId,
    int Quantity,
    string? SpecialRequests = null,
    List<string>? Modifications = null);

/// <summary>
/// DTO pour mettre à jour une commande
/// </summary>
public record UpdateKitchenOrderDto(
    string? SpecialInstructions = null,
    string? CustomerNotes = null);

/// <summary>
/// DTO de réponse pour une commande cuisine
/// </summary>
public record KitchenOrderDto(
    Guid Id,
    string OrderNumber,
    Guid TableId,
    string TableNumber,
    Guid? CustomerId,
    string? CustomerName,
    Guid? WaiterId,
    string? WaiterName,
    Guid? ChefId,
    string? ChefName,
    OrderType OrderType,
    OrderStatus Status,
    OrderPriority Priority,
    KitchenSection KitchenSection,
    DateTime OrderedAt,
    DateTime? AcceptedAt,
    DateTime? StartedAt,
    DateTime? ReadyAt,
    DateTime? ServedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    PreparationTime EstimatedPreparationTime,
    TimeSpan? ActualPreparationTime,
    DateTime EstimatedReadyTime,
    string? SpecialInstructions,
    string? CustomerNotes,
    string? ChefNotes,
    string? CancellationReason,
    bool IsRush,
    bool IsComplicated,
    bool RequiresSpecialAttention,
    QualityLevel? QualityLevel,
    int? CustomerRating,
    string? CustomerFeedback,
    List<AllergenType> Allergens,
    List<string> DietaryRequirements,
    bool RequiresAllergenAttention,
    decimal SubTotal,
    decimal? DiscountAmount,
    decimal TotalAmount,
    decimal? TipAmount,
    List<KitchenOrderItemDto> Items,
    List<KitchenOrderLogDto> Logs,
    List<KitchenOrderModificationDto> Modifications,
    bool IsOverdue,
    int ProgressPercentage,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO pour élément de commande cuisine
/// </summary>
public record KitchenOrderItemDto(
    Guid Id,
    Guid MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? SpecialRequests,
    KitchenSection? RequiredKitchenSection,
    PreparationTime EstimatedPreparationTime,
    ItemStatus Status,
    bool IsComplicated,
    bool IsUrgent,
    List<AllergenType> Allergens,
    List<string> DietaryRequirements,
    List<string> Modifications);

/// <summary>
/// DTO simplifié pour commande cuisine
/// </summary>
public record KitchenOrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string TableNumber,
    OrderStatus Status,
    OrderPriority Priority,
    KitchenSection KitchenSection,
    DateTime OrderedAt,
    DateTime EstimatedReadyTime,
    int ItemCount,
    decimal TotalAmount,
    bool IsOverdue,
    bool RequiresSpecialAttention,
    int ProgressPercentage);

/// <summary>
/// DTO pour accepter une commande
/// </summary>
public record AcceptKitchenOrderDto(
    Guid ChefId,
    string? Notes = null);

/// <summary>
/// DTO pour démarrer la préparation
/// </summary>
public record StartPreparationDto(
    Guid? ChefId = null);

/// <summary>
/// DTO pour marquer comme prêt
/// </summary>
public record MarkOrderReadyDto(
    Guid? ChefId = null,
    QualityLevel? QualityLevel = null);

/// <summary>
/// DTO pour marquer comme servi
/// </summary>
public record MarkOrderServedDto(
    Guid? WaiterId = null);

/// <summary>
/// DTO pour compléter une commande
/// </summary>
public record CompleteKitchenOrderDto(
    int? CustomerRating = null,
    string? CustomerFeedback = null,
    decimal? TipAmount = null);

/// <summary>
/// DTO pour annuler une commande
/// </summary>
public record CancelKitchenOrderDto(
    string Reason,
    Guid? CancelledById = null);

/// <summary>
/// DTO pour changer la priorité
/// </summary>
public record ChangeOrderPriorityDto(
    OrderPriority Priority,
    string? Reason = null);

/// <summary>
/// DTO pour modifier un élément
/// </summary>
public record ModifyOrderItemDto(
    Guid ItemId,
    int? NewQuantity = null,
    string? NewSpecialRequests = null);

/// <summary>
/// DTO pour appliquer une remise
/// </summary>
public record ApplyDiscountDto(
    decimal DiscountAmount,
    string Reason);

/// <summary>
/// DTO pour journal de commande
/// </summary>
public record KitchenOrderLogDto(
    Guid Id,
    OrderAction Action,
    string Description,
    Guid? PerformedBy,
    string? PerformedByName,
    DateTime PerformedAt);

/// <summary>
/// DTO pour modification de commande
/// </summary>
public record KitchenOrderModificationDto(
    Guid Id,
    Guid? ItemId,
    int? OldQuantity,
    int? NewQuantity,
    string? OldSpecialRequests,
    string? NewSpecialRequests,
    DateTime ModifiedAt,
    string Reason);

/// <summary>
/// DTO pour statistiques de commandes
/// </summary>
public record KitchenOrderStatisticsDto(
    int TotalOrders,
    int PendingOrders,
    int InPreparationOrders,
    int ReadyOrders,
    int CompletedOrders,
    int CancelledOrders,
    decimal CompletionRate,
    decimal CancellationRate,
    TimeSpan AveragePreparationTime,
    TimeSpan AverageServiceTime,
    decimal AverageOrderValue,
    decimal TotalRevenue,
    int OverdueOrders,
    decimal AverageCustomerRating,
    Dictionary<KitchenSection, int> OrdersBySection,
    Dictionary<OrderPriority, int> OrdersByPriority,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour tableau de bord cuisine
/// </summary>
public record KitchenDashboardDto(
    List<KitchenOrderSummaryDto> ActiveOrders,
    List<KitchenOrderSummaryDto> OverdueOrders,
    List<KitchenOrderSummaryDto> HighPriorityOrders,
    KitchenOrderStatisticsDto TodayStatistics,
    List<KitchenAlertDto> Alerts,
    Dictionary<KitchenSection, List<KitchenOrderSummaryDto>> OrdersBySection,
    DateTime LastUpdated);

/// <summary>
/// DTO pour alerte cuisine
/// </summary>
public record KitchenAlertDto(
    Guid OrderId,
    string OrderNumber,
    AlertType AlertType,
    string Message,
    AlertPriority Priority,
    DateTime CreatedAt);

/// <summary>
/// DTO pour recherche de commandes
/// </summary>
public record KitchenOrderSearchDto(
    string? OrderNumber = null,
    Guid? TableId = null,
    Guid? CustomerId = null,
    Guid? ChefId = null,
    OrderStatus? Status = null,
    OrderPriority? Priority = null,
    KitchenSection? KitchenSection = null,
    DateTime? OrderedAfter = null,
    DateTime? OrderedBefore = null,
    bool? IsOverdue = null,
    int PageSize = 20,
    int PageNumber = 1);

/// <summary>
/// DTO pour résultat de recherche de commandes
/// </summary>
public record PagedKitchenOrderResultDto(
    List<KitchenOrderSummaryDto> Orders,
    int TotalCount,
    int PageSize,
    int PageNumber,
    int TotalPages,
    KitchenOrderStatisticsDto Statistics);

/// <summary>
/// DTO pour performance de cuisine
/// </summary>
public record KitchenPerformanceDto(
    TimeSpan AveragePreparationTime,
    TimeSpan TargetPreparationTime,
    decimal PerformanceRatio,
    int OrdersOnTime,
    int TotalOrders,
    decimal OnTimePercentage,
    int OverdueOrders,
    decimal AverageQualityRating,
    List<ChefPerformanceDto> ChefPerformances,
    Dictionary<DateTime, TimeSpan> PreparationTimesByHour,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour performance d'un chef
/// </summary>
public record ChefPerformanceDto(
    Guid ChefId,
    string ChefName,
    int OrdersCompleted,
    TimeSpan AveragePreparationTime,
    decimal AverageQualityRating,
    int OrdersOnTime,
    decimal OnTimePercentage);