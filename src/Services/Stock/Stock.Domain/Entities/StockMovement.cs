using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Stock.Domain.Enums;
using Stock.Domain.Events;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Entité représentant un mouvement de stock
/// </summary>
public sealed class StockMovement : Entity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid LocationId { get; private set; }
    public MovementType MovementType { get; private set; }
    public StockQuantity Quantity { get; private set; }
    public string? Reference { get; private set; }
    public string? Reason { get; private set; }
    public UnitCost? UnitCost { get; private set; }
    public Money? TotalCost { get; private set; }
    public DateTime MovementDate { get; private set; }
    public Guid UserId { get; private set; }

    // Navigation properties
    public Guid? PurchaseOrderId { get; private set; }
    public Guid? ReservationId { get; private set; }
    public Guid? TransferToLocationId { get; private set; }

    private StockMovement() 
    { 
        Quantity = new StockQuantity(0, "unit");
    }

    public StockMovement(
        Guid productId,
        Guid locationId,
        MovementType movementType,
        StockQuantity quantity,
        Guid userId,
        string? reference = null,
        string? reason = null,
        UnitCost? unitCost = null,
        DateTime? movementDate = null,
        Guid? purchaseOrderId = null,
        Guid? reservationId = null,
        Guid? transferToLocationId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        ValidateMovementType(movementType, transferToLocationId);

        Id = Guid.NewGuid();
        ProductId = productId;
        LocationId = locationId;
        MovementType = movementType;
        Quantity = quantity ?? throw new ArgumentNullException(nameof(quantity));
        UserId = userId;
        Reference = reference?.Trim();
        Reason = reason?.Trim();
        UnitCost = unitCost;
        MovementDate = movementDate ?? DateTime.UtcNow;
        PurchaseOrderId = purchaseOrderId;
        ReservationId = reservationId;
        TransferToLocationId = transferToLocationId;

        // Calculer le coût total si le coût unitaire est fourni
        if (unitCost != null)
        {
            TotalCost = unitCost.CalculateTotal(quantity);
        }

        CreatedAt = DateTime.UtcNow;

        // Déclencher l'événement de domaine
        AddDomainEvent(new StockMovementOccurredEvent(
            Id,
            ProductId,
            LocationId,
            MovementType,
            Quantity,
            Reference,
            Reason,
            MovementDate));
    }

    /// <summary>
    /// Créer un mouvement d'entrée de stock
    /// </summary>
    public static StockMovement CreateInbound(
        Guid productId,
        Guid locationId,
        StockQuantity quantity,
        Guid userId,
        UnitCost? unitCost = null,
        string? reference = null,
        string? reason = null,
        Guid? purchaseOrderId = null)
    {
        return new StockMovement(
            productId,
            locationId,
            MovementType.Inbound,
            quantity,
            userId,
            reference,
            reason,
            unitCost,
            purchaseOrderId: purchaseOrderId);
    }

    /// <summary>
    /// Créer un mouvement de sortie de stock
    /// </summary>
    public static StockMovement CreateOutbound(
        Guid productId,
        Guid locationId,
        StockQuantity quantity,
        Guid userId,
        string? reference = null,
        string? reason = null,
        Guid? reservationId = null)
    {
        return new StockMovement(
            productId,
            locationId,
            MovementType.Outbound,
            quantity,
            userId,
            reference,
            reason,
            reservationId: reservationId);
    }

    /// <summary>
    /// Créer un mouvement de transfert
    /// </summary>
    public static StockMovement CreateTransfer(
        Guid productId,
        Guid fromLocationId,
        Guid toLocationId,
        StockQuantity quantity,
        Guid userId,
        string? reference = null,
        string? reason = null)
    {
        if (fromLocationId == toLocationId)
            throw new ArgumentException("Source and destination locations cannot be the same");

        return new StockMovement(
            productId,
            fromLocationId,
            MovementType.TransferOut,
            quantity,
            userId,
            reference,
            reason,
            transferToLocationId: toLocationId);
    }

    /// <summary>
    /// Créer un mouvement d'ajustement
    /// </summary>
    public static StockMovement CreateAdjustment(
        Guid productId,
        Guid locationId,
        StockQuantity adjustmentQuantity,
        Guid userId,
        string? reason = null)
    {
        var movementType = adjustmentQuantity.Value >= 0 
            ? MovementType.AdjustmentIn 
            : MovementType.AdjustmentOut;

        return new StockMovement(
            productId,
            locationId,
            movementType,
            new StockQuantity(Math.Abs(adjustmentQuantity.Value), adjustmentQuantity.Unit),
            userId,
            reason: reason);
    }

    /// <summary>
    /// Créer un mouvement de perte
    /// </summary>
    public static StockMovement CreateLoss(
        Guid productId,
        Guid locationId,
        StockQuantity quantity,
        Guid userId,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for loss movements", nameof(reason));

        return new StockMovement(
            productId,
            locationId,
            MovementType.Loss,
            quantity,
            userId,
            reason: reason);
    }

    /// <summary>
    /// Mettre à jour le coût unitaire
    /// </summary>
    public void UpdateUnitCost(UnitCost unitCost)
    {
        UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
        TotalCost = unitCost.CalculateTotal(Quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si c'est un mouvement d'entrée
    /// </summary>
    public bool IsInboundMovement()
    {
        return MovementType.IsInbound();
    }

    /// <summary>
    /// Vérifier si c'est un mouvement de sortie
    /// </summary>
    public bool IsOutboundMovement()
    {
        return MovementType.IsOutbound();
    }

    /// <summary>
    /// Obtenir l'impact sur le stock (positif ou négatif)
    /// </summary>
    public StockQuantity GetStockImpact()
    {
        var multiplier = IsInboundMovement() ? 1 : -1;
        return new StockQuantity(Quantity.Value * multiplier, Quantity.Unit);
    }

    private static void ValidateMovementType(MovementType movementType, Guid? transferToLocationId)
    {
        if (movementType == MovementType.TransferOut && transferToLocationId == null)
            throw new ArgumentException("Transfer destination location is required for transfer movements");
        
        if (movementType != MovementType.TransferOut && transferToLocationId != null)
            throw new ArgumentException("Transfer destination location should only be specified for transfer movements");
    }
}