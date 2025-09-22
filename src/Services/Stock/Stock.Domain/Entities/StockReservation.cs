using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Stock.Domain.Enums;
using Stock.Domain.Events;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Entité représentant une réservation de stock
/// </summary>
public sealed class StockReservation : Entity, IAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid LocationId { get; private set; }
    public StockQuantity ReservedQuantity { get; private set; }
    public StockQuantity? ConfirmedQuantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? Reference { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid UserId { get; private set; }

    // Navigation properties
    public Guid? OrderId { get; private set; }
    public Guid? CustomerId { get; private set; }

    private StockReservation() 
    { 
        ReservedQuantity = new StockQuantity(0, "unit");
    }

    public StockReservation(
        Guid productId,
        Guid locationId,
        StockQuantity reservedQuantity,
        DateTime expirationDate,
        Guid userId,
        string? reference = null,
        Guid? orderId = null,
        Guid? customerId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expirationDate <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));

        if (reservedQuantity.IsZero)
            throw new ArgumentException("Reserved quantity cannot be zero", nameof(reservedQuantity));

        Id = Guid.NewGuid();
        ProductId = productId;
        LocationId = locationId;
        ReservedQuantity = reservedQuantity;
        Status = ReservationStatus.Active;
        Reference = reference?.Trim();
        ExpirationDate = expirationDate;
        UserId = userId;
        OrderId = orderId;
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;

        // Déclencher l'événement de domaine
        AddDomainEvent(new StockReservationCreatedEvent(
            Id,
            ProductId,
            LocationId,
            ReservedQuantity,
            Reference,
            ExpirationDate,
            CreatedAt));
    }

    /// <summary>
    /// Confirmer la réservation avec une quantité spécifique
    /// </summary>
    public void Confirm(StockQuantity confirmedQuantity)
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot confirm reservation with status {Status}");

        if (confirmedQuantity.Unit != ReservedQuantity.Unit)
            throw new ArgumentException($"Confirmed quantity unit ({confirmedQuantity.Unit}) must match reserved quantity unit ({ReservedQuantity.Unit})");

        if (confirmedQuantity.Value > ReservedQuantity.Value)
            throw new ArgumentException("Confirmed quantity cannot exceed reserved quantity");

        ConfirmedQuantity = confirmedQuantity;
        Status = ReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Déclencher l'événement de domaine
        AddDomainEvent(new StockReservationConfirmedEvent(
            Id,
            ProductId,
            LocationId,
            confirmedQuantity,
            ConfirmedAt.Value));
    }

    /// <summary>
    /// Confirmer la réservation avec la quantité totale
    /// </summary>
    public void ConfirmFull()
    {
        Confirm(ReservedQuantity);
    }

    /// <summary>
    /// Annuler la réservation
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reservation is already cancelled");

        if (Status == ReservationStatus.Confirmed)
            throw new InvalidOperationException("Cannot cancel a confirmed reservation");

        var previousStatus = Status;
        Status = ReservationStatus.Cancelled;
        CancellationReason = reason?.Trim();
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Déclencher l'événement approprié
        if (previousStatus == ReservationStatus.Expired)
        {
            AddDomainEvent(new StockReservationExpiredEvent(
                Id,
                ProductId,
                LocationId,
                ReservedQuantity,
                CancelledAt.Value));
        }
        else
        {
            AddDomainEvent(new StockReservationCancelledEvent(
                Id,
                ProductId,
                LocationId,
                ReservedQuantity,
                CancellationReason,
                CancelledAt.Value));
        }
    }

    /// <summary>
    /// Marquer comme expirée
    /// </summary>
    public void MarkAsExpired()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot expire reservation with status {Status}");

        if (DateTime.UtcNow <= ExpirationDate)
            throw new InvalidOperationException("Reservation has not yet expired");

        Status = ReservationStatus.Expired;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockReservationExpiredEvent(
            Id,
            ProductId,
            LocationId,
            ReservedQuantity,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Étendre la date d'expiration
    /// </summary>
    public void ExtendExpiration(DateTime newExpirationDate)
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot extend reservation with status {Status}");

        if (newExpirationDate <= ExpirationDate)
            throw new ArgumentException("New expiration date must be after current expiration date", nameof(newExpirationDate));

        if (newExpirationDate <= DateTime.UtcNow)
            throw new ArgumentException("New expiration date must be in the future", nameof(newExpirationDate));

        ExpirationDate = newExpirationDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si la réservation est active
    /// </summary>
    public bool IsActive => Status == ReservationStatus.Active && DateTime.UtcNow <= ExpirationDate;

    /// <summary>
    /// Vérifier si la réservation est expirée
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpirationDate && Status == ReservationStatus.Active;

    /// <summary>
    /// Vérifier si la réservation peut être modifiée
    /// </summary>
    public bool CanBeModified => Status == ReservationStatus.Active;

    /// <summary>
    /// Obtenir la quantité restante à confirmer
    /// </summary>
    public StockQuantity GetRemainingQuantity()
    {
        if (ConfirmedQuantity == null)
            return ReservedQuantity;

        return ReservedQuantity.Subtract(ConfirmedQuantity);
    }

    /// <summary>
    /// Calculer le temps restant avant expiration
    /// </summary>
    public TimeSpan? GetTimeUntilExpiration()
    {
        if (Status != ReservationStatus.Active)
            return null;

        var remaining = ExpirationDate - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Créer une réservation pour une commande
    /// </summary>
    public static StockReservation CreateForOrder(
        Guid productId,
        Guid locationId,
        StockQuantity quantity,
        Guid orderId,
        Guid userId,
        TimeSpan reservationDuration = default)
    {
        var duration = reservationDuration == default ? TimeSpan.FromHours(24) : reservationDuration;
        var expirationDate = DateTime.UtcNow.Add(duration);
        
        return new StockReservation(
            productId,
            locationId,
            quantity,
            expirationDate,
            userId,
            reference: $"ORDER-{orderId}",
            orderId: orderId);
    }

    /// <summary>
    /// Créer une réservation temporaire
    /// </summary>
    public static StockReservation CreateTemporary(
        Guid productId,
        Guid locationId,
        StockQuantity quantity,
        Guid userId,
        TimeSpan duration = default)
    {
        var reservationDuration = duration == default ? TimeSpan.FromMinutes(30) : duration;
        var expirationDate = DateTime.UtcNow.Add(reservationDuration);
        
        return new StockReservation(
            productId,
            locationId,
            quantity,
            expirationDate,
            userId,
            reference: "TEMP");
    }
}