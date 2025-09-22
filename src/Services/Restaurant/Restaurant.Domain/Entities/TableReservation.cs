using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant une réservation de table
/// </summary>
public sealed class TableReservation : Entity, IAggregateRoot
{
    public Guid TableId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime ReservationDateTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    public int PartySize { get; private set; }
    public int? ActualPartySize { get; private set; }
    public ReservationStatus Status { get; private set; }
    public string? SpecialRequests { get; private set; }
    public string? CustomerNotes { get; private set; }
    public string? InternalNotes { get; private set; }

    // Informations de contact
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }

    // Préférences
    public bool PrefersSmoking { get; private set; }
    public bool RequiresAccessibility { get; private set; }
    public bool PrefersView { get; private set; }
    public RestaurantZone? PreferredZone { get; private set; }
    public List<AllergenType> Allergies { get; private set; } = new();

    // Informations de service
    public Guid? AssignedWaiterId { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? SeatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // Métriques
    public TimeSpan? ActualDuration { get; private set; }
    public decimal? FinalBill { get; private set; }
    public Rating? CustomerRating { get; private set; }

    private readonly List<ReservationModification> _modifications = new();
    public IReadOnlyList<ReservationModification> Modifications => _modifications.AsReadOnly();

    private TableReservation() { } // EF Constructor

    public TableReservation(
        Guid tableId,
        Guid customerId,
        DateTime reservationDateTime,
        int partySize,
        string customerName,
        string customerPhone,
        TimeSpan? duration = null,
        string? customerEmail = null,
        string? specialRequests = null,
        bool prefersSmoking = false,
        bool requiresAccessibility = false,
        bool prefersView = false,
        RestaurantZone? preferredZone = null,
        List<AllergenType>? allergies = null)
    {
        if (tableId == Guid.Empty)
            throw new ArgumentException("Table ID cannot be empty", nameof(tableId));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
        
        if (reservationDateTime <= DateTime.UtcNow)
            throw new ArgumentException("Reservation date must be in the future", nameof(reservationDateTime));
        
        if (partySize <= 0)
            throw new ArgumentException("Party size must be positive", nameof(partySize));
        
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name cannot be null or empty", nameof(customerName));
        
        if (string.IsNullOrWhiteSpace(customerPhone))
            throw new ArgumentException("Customer phone cannot be null or empty", nameof(customerPhone));

        Id = Guid.NewGuid();
        TableId = tableId;
        CustomerId = customerId;
        ReservationDateTime = reservationDateTime;
        Duration = duration ?? TimeSpan.FromHours(2); // Durée par défaut de 2 heures
        PartySize = partySize;
        Status = ReservationStatus.Pending;
        CustomerName = customerName.Trim();
        CustomerPhone = customerPhone.Trim();
        CustomerEmail = customerEmail?.Trim();
        SpecialRequests = specialRequests?.Trim();
        PrefersSmoking = prefersSmoking;
        RequiresAccessibility = requiresAccessibility;
        PrefersView = prefersView;
        PreferredZone = preferredZone;
        Allergies = allergies?.ToList() ?? new List<AllergenType>();
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TableReservationCreatedEvent(
            Id,
            TableId,
            CustomerId,
            ReservationDateTime,
            PartySize,
            SpecialRequests,
            CreatedAt));
    }

    /// <summary>
    /// Confirmer la réservation
    /// </summary>
    public void Confirm(Guid? assignedWaiterId = null)
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm reservation with status {Status}");

        if (ReservationDateTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot confirm past reservation");

        Status = ReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        AssignedWaiterId = assignedWaiterId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TableReservationConfirmedEvent(
            Id,
            TableId,
            CustomerId,
            ReservationDateTime,
            ConfirmedAt.Value));
    }

    /// <summary>
    /// Installer les clients
    /// </summary>
    public void SeatCustomers(int actualPartySize, Guid? waiterId = null)
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException($"Cannot seat customers for reservation with status {Status}");

        if (actualPartySize <= 0)
            throw new ArgumentException("Actual party size must be positive", nameof(actualPartySize));

        // Permettre une variation de ±20% par rapport à la taille prévue
        var tolerance = Math.Max(1, PartySize * 0.2);
        if (Math.Abs(actualPartySize - PartySize) > tolerance)
        {
            throw new InvalidOperationException($"Actual party size ({actualPartySize}) differs too much from reserved size ({PartySize})");
        }

        Status = ReservationStatus.Seated;
        ActualPartySize = actualPartySize;
        SeatedAt = DateTime.UtcNow;
        
        if (waiterId.HasValue)
            AssignedWaiterId = waiterId;
        
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomersSeatedEvent(
            Id,
            TableId,
            actualPartySize,
            AssignedWaiterId,
            SeatedAt.Value));
    }

    /// <summary>
    /// Terminer la réservation
    /// </summary>
    public void Complete(decimal? finalBill = null, Rating? customerRating = null)
    {
        if (Status != ReservationStatus.Seated)
            throw new InvalidOperationException($"Cannot complete reservation with status {Status}");

        Status = ReservationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        FinalBill = finalBill;
        CustomerRating = customerRating;
        
        if (SeatedAt.HasValue)
        {
            ActualDuration = CompletedAt.Value - SeatedAt.Value;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Annuler la réservation
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reservation is already cancelled");
        
        if (Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed reservation");

        Status = ReservationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason?.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TableReservationCancelledEvent(
            Id,
            TableId,
            CustomerId,
            CancellationReason,
            CancelledAt.Value));
    }

    /// <summary>
    /// Marquer comme no-show (client absent)
    /// </summary>
    public void MarkAsNoShow()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException($"Cannot mark as no-show reservation with status {Status}");

        if (ReservationDateTime > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot mark future reservation as no-show");

        Status = ReservationStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CustomerNoShowEvent(
            Id,
            TableId,
            CustomerId,
            ReservationDateTime,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Modifier la réservation
    /// </summary>
    public void ModifyReservation(
        DateTime? newDateTime = null,
        int? newPartySize = null,
        TimeSpan? newDuration = null,
        Guid? newTableId = null,
        string? reason = null,
        Guid? modifiedByUserId = null)
    {
        if (Status != ReservationStatus.Pending && Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException($"Cannot modify reservation with status {Status}");

        var modification = new ReservationModification(
            Id,
            ReservationDateTime,
            PartySize,
            Duration,
            TableId,
            newDateTime,
            newPartySize,
            newDuration,
            newTableId,
            reason,
            modifiedByUserId);

        _modifications.Add(modification);

        // Appliquer les modifications
        if (newDateTime.HasValue)
        {
            if (newDateTime.Value <= DateTime.UtcNow)
                throw new ArgumentException("New reservation date must be in the future", nameof(newDateTime));
            ReservationDateTime = newDateTime.Value;
        }

        if (newPartySize.HasValue)
        {
            if (newPartySize.Value <= 0)
                throw new ArgumentException("New party size must be positive", nameof(newPartySize));
            PartySize = newPartySize.Value;
        }

        if (newDuration.HasValue)
        {
            if (newDuration.Value <= TimeSpan.Zero)
                throw new ArgumentException("New duration must be positive", nameof(newDuration));
            Duration = newDuration.Value;
        }

        if (newTableId.HasValue)
        {
            if (newTableId.Value == Guid.Empty)
                throw new ArgumentException("New table ID cannot be empty", nameof(newTableId));
            TableId = newTableId.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour les informations de contact
    /// </summary>
    public void UpdateContactInformation(string? customerName = null, string? customerPhone = null, string? customerEmail = null)
    {
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            CustomerName = customerName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            CustomerPhone = customerPhone.Trim();
        }

        if (customerEmail != null)
        {
            CustomerEmail = customerEmail.Trim();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mettre à jour les préférences
    /// </summary>
    public void UpdatePreferences(
        bool? prefersSmoking = null,
        bool? requiresAccessibility = null,
        bool? prefersView = null,
        RestaurantZone? preferredZone = null,
        List<AllergenType>? allergies = null)
    {
        if (prefersSmoking.HasValue)
            PrefersSmoking = prefersSmoking.Value;

        if (requiresAccessibility.HasValue)
            RequiresAccessibility = requiresAccessibility.Value;

        if (prefersView.HasValue)
            PrefersView = prefersView.Value;

        if (preferredZone.HasValue)
            PreferredZone = preferredZone.Value;

        if (allergies != null)
            Allergies = allergies.ToList();

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter des notes internes
    /// </summary>
    public void AddInternalNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return;

        InternalNotes = string.IsNullOrEmpty(InternalNotes) 
            ? notes.Trim() 
            : $"{InternalNotes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {notes.Trim()}";

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigner un serveur
    /// </summary>
    public void AssignWaiter(Guid waiterId)
    {
        if (waiterId == Guid.Empty)
            throw new ArgumentException("Waiter ID cannot be empty", nameof(waiterId));

        AssignedWaiterId = waiterId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculer le temps restant avant la réservation
    /// </summary>
    public TimeSpan? TimeUntilReservation => ReservationDateTime > DateTime.UtcNow 
        ? ReservationDateTime - DateTime.UtcNow 
        : null;

    /// <summary>
    /// Vérifier si la réservation est bientôt (dans les 30 minutes)
    /// </summary>
    public bool IsUpcoming => TimeUntilReservation.HasValue && TimeUntilReservation.Value <= TimeSpan.FromMinutes(30);

    /// <summary>
    /// Vérifier si la réservation est en retard
    /// </summary>
    public bool IsLate => Status == ReservationStatus.Confirmed && DateTime.UtcNow > ReservationDateTime.Add(TimeSpan.FromMinutes(15));

    /// <summary>
    /// Calculer l'heure de fin prévue
    /// </summary>
    public DateTime ExpectedEndTime => ReservationDateTime.Add(Duration);

    /// <summary>
    /// Vérifier si la réservation peut être modifiée
    /// </summary>
    public bool CanBeModified => (Status == ReservationStatus.Pending || Status == ReservationStatus.Confirmed) 
                               && ReservationDateTime > DateTime.UtcNow.AddHours(2); // 2h avant la réservation

    /// <summary>
    /// Obtenir le nombre de modifications
    /// </summary>
    public int ModificationCount => _modifications.Count;

    /// <summary>
    /// Vérifier si c'est une réservation récurrente potentielle
    /// </summary>
    public bool IsPotentialRecurring => TotalReservationsForCustomer > 3; // Serait calculé depuis le repository

    private int TotalReservationsForCustomer { get; set; } // Serait rempli par le repository
}

/// <summary>
/// Historique des modifications d'une réservation
/// </summary>
public sealed class ReservationModification : Entity
{
    public Guid ReservationId { get; private set; }
    
    // Valeurs précédentes
    public DateTime PreviousDateTime { get; private set; }
    public int PreviousPartySize { get; private set; }
    public TimeSpan PreviousDuration { get; private set; }
    public Guid PreviousTableId { get; private set; }
    
    // Nouvelles valeurs
    public DateTime? NewDateTime { get; private set; }
    public int? NewPartySize { get; private set; }
    public TimeSpan? NewDuration { get; private set; }
    public Guid? NewTableId { get; private set; }
    
    public string? Reason { get; private set; }
    public Guid? ModifiedByUserId { get; private set; }

    private ReservationModification() { } // EF Constructor

    public ReservationModification(
        Guid reservationId,
        DateTime previousDateTime,
        int previousPartySize,
        TimeSpan previousDuration,
        Guid previousTableId,
        DateTime? newDateTime,
        int? newPartySize,
        TimeSpan? newDuration,
        Guid? newTableId,
        string? reason,
        Guid? modifiedByUserId)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        PreviousDateTime = previousDateTime;
        PreviousPartySize = previousPartySize;
        PreviousDuration = previousDuration;
        PreviousTableId = previousTableId;
        NewDateTime = newDateTime;
        NewPartySize = newPartySize;
        NewDuration = newDuration;
        NewTableId = newTableId;
        Reason = reason?.Trim();
        ModifiedByUserId = modifiedByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtenir un résumé des changements
    /// </summary>
    public string GetChangesSummary()
    {
        var changes = new List<string>();

        if (NewDateTime.HasValue)
            changes.Add($"Date: {PreviousDateTime:dd/MM/yyyy HH:mm} → {NewDateTime:dd/MM/yyyy HH:mm}");

        if (NewPartySize.HasValue)
            changes.Add($"Personnes: {PreviousPartySize} → {NewPartySize}");

        if (NewDuration.HasValue)
            changes.Add($"Durée: {PreviousDuration} → {NewDuration}");

        if (NewTableId.HasValue)
            changes.Add($"Table changée");

        return string.Join(", ", changes);
    }
}