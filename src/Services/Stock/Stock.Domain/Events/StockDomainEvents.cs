using BuildingBlocks.Domain;
using Stock.Domain.Enums;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Events;

/// <summary>
/// Événement déclenché lors d'un mouvement de stock
/// </summary>
public sealed record StockMovementOccurredEvent(
    Guid StockMovementId,
    Guid ProductId,
    Guid LocationId,
    MovementType MovementType,
    StockQuantity Quantity,
    string? Reference,
    string? Reason,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lorsque le stock d'un produit atteint le seuil d'alerte
/// </summary>
public sealed record StockAlertTriggeredEvent(
    Guid ProductId,
    Guid LocationId,
    AlertType AlertType,
    StockQuantity CurrentQuantity,
    StockQuantity ThresholdQuantity,
    DateTime TriggeredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la création d'une réservation de stock
/// </summary>
public sealed record StockReservationCreatedEvent(
    Guid ReservationId,
    Guid ProductId,
    Guid LocationId,
    StockQuantity ReservedQuantity,
    string? Reference,
    DateTime ExpirationDate,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la confirmation d'une réservation
/// </summary>
public sealed record StockReservationConfirmedEvent(
    Guid ReservationId,
    Guid ProductId,
    Guid LocationId,
    StockQuantity ConfirmedQuantity,
    DateTime ConfirmedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'annulation d'une réservation
/// </summary>
public sealed record StockReservationCancelledEvent(
    Guid ReservationId,
    Guid ProductId,
    Guid LocationId,
    StockQuantity CancelledQuantity,
    string? CancellationReason,
    DateTime CancelledAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lorsqu'une réservation expire
/// </summary>
public sealed record StockReservationExpiredEvent(
    Guid ReservationId,
    Guid ProductId,
    Guid LocationId,
    StockQuantity ExpiredQuantity,
    DateTime ExpiredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la création d'un bon de commande
/// </summary>
public sealed record PurchaseOrderCreatedEvent(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string OrderNumber,
    Money TotalAmount,
    DateTime ExpectedDeliveryDate,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la réception d'un bon de commande
/// </summary>
public sealed record PurchaseOrderReceivedEvent(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string OrderNumber,
    Money TotalAmount,
    DateTime ReceivedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'annulation d'un bon de commande
/// </summary>
public sealed record PurchaseOrderCancelledEvent(
    Guid PurchaseOrderId,
    Guid SupplierId,
    string OrderNumber,
    string? CancellationReason,
    DateTime CancelledAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la création d'un nouvel emplacement
/// </summary>
public sealed record LocationCreatedEvent(
    Guid LocationId,
    string Name,
    Enums.LocationType LocationType,
    string Code,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la désactivation d'un emplacement
/// </summary>
public sealed record LocationDeactivatedEvent(
    Guid LocationId,
    string Name,
    string? DeactivationReason,
    DateTime DeactivatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la création d'un nouveau fournisseur
/// </summary>
public sealed record SupplierCreatedEvent(
    Guid SupplierId,
    string Name,
    string Code,
    string Email,
    string Phone,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la mise à jour des informations d'un fournisseur
/// </summary>
public sealed record SupplierUpdatedEvent(
    Guid SupplierId,
    string Name,
    string Code,
    string Email,
    string Phone,
    DateTime UpdatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la désactivation d'un fournisseur
/// </summary>
public sealed record SupplierDeactivatedEvent(
    Guid SupplierId,
    string Name,
    string? DeactivationReason,
    DateTime DeactivatedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'un transfert de stock entre emplacements
/// </summary>
public sealed record StockTransferredEvent(
    Guid TransferId,
    Guid ProductId,
    Guid FromLocationId,
    Guid ToLocationId,
    StockQuantity TransferredQuantity,
    string? TransferReason,
    DateTime TransferredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'un ajustement de stock (inventaire)
/// </summary>
public sealed record StockAdjustedEvent(
    Guid AdjustmentId,
    Guid ProductId,
    Guid LocationId,
    StockQuantity PreviousQuantity,
    StockQuantity NewQuantity,
    StockQuantity AdjustmentQuantity,
    string? AdjustmentReason,
    DateTime AdjustedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la péremption d'un produit
/// </summary>
public sealed record ProductExpiredEvent(
    Guid ProductId,
    Guid LocationId,
    StockQuantity ExpiredQuantity,
    DateTime ExpirationDate,
    DateTime DetectedAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'un contrôle qualité défaillant
/// </summary>
public sealed record QualityControlFailedEvent(
    Guid ProductId,
    Guid LocationId,
    StockQuantity RejectedQuantity,
    string QualityIssue,
    DateTime ControlledAt
) : IDomainEvent;