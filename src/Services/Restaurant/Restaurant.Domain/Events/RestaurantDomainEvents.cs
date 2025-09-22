using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Events;

/// <summary>
/// Événement déclenché lors de la création d'une réservation
/// </summary>
public sealed record TableReservationCreatedEvent(
    Guid ReservationId,
    Guid TableId,
    Guid CustomerId,
    DateTime ReservationDateTime,
    int PartySize,
    string? SpecialRequests,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la confirmation d'une réservation
/// </summary>
public sealed record TableReservationConfirmedEvent(
    Guid ReservationId,
    Guid TableId,
    Guid CustomerId,
    DateTime ReservationDateTime,
    DateTime ConfirmedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lorsque les clients sont installés
/// </summary>
public sealed record CustomersSeatedEvent(
    Guid ReservationId,
    Guid TableId,
    int ActualPartySize,
    Guid? WaiterId,
    DateTime SeatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'annulation d'une réservation
/// </summary>
public sealed record TableReservationCancelledEvent(
    Guid ReservationId,
    Guid TableId,
    Guid CustomerId,
    string? CancellationReason,
    DateTime CancelledAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand des clients ne se présentent pas
/// </summary>
public sealed record CustomerNoShowEvent(
    Guid ReservationId,
    Guid TableId,
    Guid CustomerId,
    DateTime ExpectedDateTime,
    DateTime RecordedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du changement de statut d'une table
/// </summary>
public sealed record TableStatusChangedEvent(
    Guid TableId,
    TableStatus PreviousStatus,
    TableStatus NewStatus,
    Guid? ChangedByUserId,
    string? Reason,
    DateTime ChangedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la création d'une commande cuisine
/// </summary>
public sealed record KitchenOrderCreatedEvent(
    Guid KitchenOrderId,
    Guid TableId,
    Guid? ReservationId,
    List<Guid> MenuItemIds,
    OrderPriority Priority,
    Guid? WaiterId,
    string? SpecialInstructions,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du changement de statut d'une commande cuisine
/// </summary>
public sealed record KitchenOrderStatusChangedEvent(
    Guid KitchenOrderId,
    KitchenOrderStatus PreviousStatus,
    KitchenOrderStatus NewStatus,
    Guid? ChangedByUserId,
    TimeSpan? PreparationTime,
    DateTime ChangedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand une commande est prête
/// </summary>
public sealed record KitchenOrderReadyEvent(
    Guid KitchenOrderId,
    Guid TableId,
    Guid? WaiterId,
    List<Guid> ReadyItemIds,
    TimeSpan TotalPreparationTime,
    DateTime ReadyAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'ajout d'un élément au menu
/// </summary>
public sealed record MenuItemAddedEvent(
    Guid MenuItemId,
    Guid MenuId,
    string Name,
    MenuCategory Category,
    MenuPrice Price,
    DateTime AddedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la mise à jour du prix d'un élément
/// </summary>
public sealed record MenuItemPriceUpdatedEvent(
    Guid MenuItemId,
    MenuPrice OldPrice,
    MenuPrice NewPrice,
    string? Reason,
    DateTime UpdatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du changement de statut d'un élément de menu
/// </summary>
public sealed record MenuItemStatusChangedEvent(
    Guid MenuItemId,
    MenuItemStatus PreviousStatus,
    MenuItemStatus NewStatus,
    string? Reason,
    DateTime ChangedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la rupture de stock d'un élément
/// </summary>
public sealed record MenuItemOutOfStockEvent(
    Guid MenuItemId,
    string ItemName,
    MenuCategory Category,
    List<Guid> AffectedOrderIds,
    DateTime OutOfStockAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'ajout d'une nouvelle table
/// </summary>
public sealed record TableAddedEvent(
    Guid TableId,
    string TableNumber,
    TableCapacity Capacity,
    RestaurantZone Zone,
    TablePosition? Position,
    DateTime AddedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la mise à jour de la configuration d'une table
/// </summary>
public sealed record TableConfigurationUpdatedEvent(
    Guid TableId,
    string TableNumber,
    TableCapacity? NewCapacity,
    RestaurantZone? NewZone,
    TablePosition? NewPosition,
    DateTime UpdatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'ajout d'un membre du personnel
/// </summary>
public sealed record StaffMemberAddedEvent(
    Guid StaffId,
    string Name,
    StaffType StaffType,
    string? Position,
    DateTime HiredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du changement de statut du personnel
/// </summary>
public sealed record StaffStatusChangedEvent(
    Guid StaffId,
    StaffStatus PreviousStatus,
    StaffStatus NewStatus,
    string? Reason,
    DateTime ChangedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'affectation d'un serveur à une table
/// </summary>
public sealed record WaiterAssignedToTableEvent(
    Guid WaiterId,
    Guid TableId,
    Guid? ReservationId,
    DateTime AssignedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du début d'un service (shift)
/// </summary>
public sealed record ServiceShiftStartedEvent(
    Guid ShiftId,
    List<Guid> StaffIds,
    ServiceType ServiceType,
    DateTime ShiftStartTime,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la fin d'un service
/// </summary>
public sealed record ServiceShiftEndedEvent(
    Guid ShiftId,
    List<Guid> StaffIds,
    DateTime ShiftEndTime,
    decimal TotalRevenue,
    int TotalCustomers,
    DateTime EndedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une alerte en cuisine
/// </summary>
public sealed record KitchenAlertTriggeredEvent(
    Guid AlertId,
    string AlertType,
    string Message,
    OrderPriority Priority,
    List<Guid> AffectedOrderIds,
    DateTime TriggeredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'évaluation d'un plat
/// </summary>
public sealed record MenuItemRatedEvent(
    Guid MenuItemId,
    Guid CustomerId,
    Rating Rating,
    string? ReviewComment,
    DateTime RatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une demande spéciale
/// </summary>
public sealed record SpecialRequestMadeEvent(
    Guid RequestId,
    Guid TableId,
    Guid? ReservationId,
    string RequestType,
    string Description,
    OrderPriority Priority,
    Guid? AssignedStaffId,
    DateTime RequestedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la libération d'une table
/// </summary>
public sealed record TableReleasedEvent(
    Guid TableId,
    Guid? PreviousReservationId,
    TimeSpan OccupationDuration,
    decimal? FinalBill,
    DateTime ReleasedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'un problème de service
/// </summary>
public sealed record ServiceIssueReportedEvent(
    Guid IssueId,
    Guid TableId,
    string IssueType,
    string Description,
    OrderPriority Severity,
    Guid? ReportedByStaffId,
    DateTime ReportedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'un changement de menu saisonnier
/// </summary>
public sealed record SeasonalMenuActivatedEvent(
    Guid MenuId,
    string MenuName,
    MenuType MenuType,
    List<Guid> NewItemIds,
    List<Guid> RemovedItemIds,
    DateTime ActivatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une promotion sur un élément de menu
/// </summary>
public sealed record MenuItemPromotionStartedEvent(
    Guid PromotionId,
    Guid MenuItemId,
    MenuPrice OriginalPrice,
    MenuPrice PromotionalPrice,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt
) : IDomainEvent;