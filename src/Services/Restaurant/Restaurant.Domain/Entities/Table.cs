using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant une table de restaurant
/// </summary>
public sealed class Table : Entity, IAggregateRoot
{
    public string TableNumber { get; private set; } = string.Empty;
    public TableCapacity Capacity { get; private set; }
    public TableStatus Status { get; private set; }
    public RestaurantZone Zone { get; private set; }
    public TablePosition? Position { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSmokingAllowed { get; private set; }
    public bool HasView { get; private set; }
    public bool IsAccessible { get; private set; } // Accessible aux personnes à mobilité réduite
    public string? SpecialFeatures { get; private set; }

    // Informations de service
    public Guid? CurrentReservationId { get; private set; }
    public Guid? AssignedWaiterId { get; private set; }
    public DateTime? LastCleaned { get; private set; }
    public DateTime? LastOccupied { get; private set; }
    public TimeSpan? AverageOccupationTime { get; private set; }

    // Métriques
    public int TotalReservations { get; private set; }
    public decimal? AverageRating { get; private set; }
    public decimal? DailyRevenue { get; private set; }

    private readonly List<TableMaintenance> _maintenanceHistory = new();
    public IReadOnlyList<TableMaintenance> MaintenanceHistory => _maintenanceHistory.AsReadOnly();

    private Table() { } // EF Constructor

    public Table(
        string tableNumber,
        TableCapacity capacity,
        RestaurantZone zone,
        TablePosition? position = null,
        string? description = null,
        bool isSmokingAllowed = false,
        bool hasView = false,
        bool isAccessible = false)
    {
        if (string.IsNullOrWhiteSpace(tableNumber))
            throw new ArgumentException("Table number cannot be null or empty", nameof(tableNumber));

        Id = Guid.NewGuid();
        TableNumber = tableNumber.Trim().ToUpperInvariant();
        Capacity = capacity ?? throw new ArgumentNullException(nameof(capacity));
        Status = TableStatus.Available;
        Zone = zone;
        Position = position;
        Description = description?.Trim();
        IsActive = true;
        IsSmokingAllowed = isSmokingAllowed;
        HasView = hasView;
        IsAccessible = isAccessible;
        TotalReservations = 0;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new TableAddedEvent(
            Id,
            TableNumber,
            Capacity,
            Zone,
            Position,
            CreatedAt));
    }

    /// <summary>
    /// Changer le statut de la table
    /// </summary>
    public void ChangeStatus(TableStatus newStatus, Guid? changedByUserId = null, string? reason = null)
    {
        if (Status == newStatus)
            return;

        ValidateStatusTransition(Status, newStatus);

        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        // Actions spécifiques selon le nouveau statut
        switch (newStatus)
        {
            case TableStatus.Available:
                CurrentReservationId = null;
                AssignedWaiterId = null;
                break;
            
            case TableStatus.Cleaning:
                LastCleaned = DateTime.UtcNow;
                break;
            
            case TableStatus.Occupied:
                LastOccupied = DateTime.UtcNow;
                break;
        }

        AddDomainEvent(new TableStatusChangedEvent(
            Id,
            previousStatus,
            newStatus,
            changedByUserId,
            reason,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Assigner une réservation à la table
    /// </summary>
    public void AssignReservation(Guid reservationId, Guid? waiterId = null)
    {
        if (Status != TableStatus.Available)
            throw new InvalidOperationException($"Cannot assign reservation to table with status {Status}");

        CurrentReservationId = reservationId;
        AssignedWaiterId = waiterId;
        ChangeStatus(TableStatus.Reserved);
        TotalReservations++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Installer les clients à la table
    /// </summary>
    public void SeatCustomers(int actualPartySize, Guid? waiterId = null)
    {
        if (Status != TableStatus.Reserved)
            throw new InvalidOperationException($"Cannot seat customers at table with status {Status}");

        if (!Capacity.CanAccommodate(actualPartySize))
            throw new InvalidOperationException($"Table cannot accommodate {actualPartySize} people (capacity: {Capacity})");

        if (waiterId.HasValue)
            AssignedWaiterId = waiterId;

        ChangeStatus(TableStatus.Occupied);
    }

    /// <summary>
    /// Libérer la table
    /// </summary>
    public void ReleaseTable(decimal? finalBill = null)
    {
        if (Status != TableStatus.Occupied)
            throw new InvalidOperationException($"Cannot release table with status {Status}");

        var occupationDuration = LastOccupied.HasValue 
            ? DateTime.UtcNow - LastOccupied.Value 
            : TimeSpan.Zero;

        // Mettre à jour les métriques
        UpdateAverageOccupationTime(occupationDuration);
        
        if (finalBill.HasValue)
        {
            DailyRevenue = (DailyRevenue ?? 0) + finalBill.Value;
        }

        var previousReservationId = CurrentReservationId;
        CurrentReservationId = null;
        AssignedWaiterId = null;

        AddDomainEvent(new TableReleasedEvent(
            Id,
            previousReservationId,
            occupationDuration,
            finalBill,
            DateTime.UtcNow));

        ChangeStatus(TableStatus.Cleaning);
    }

    /// <summary>
    /// Assigner un serveur à la table
    /// </summary>
    public void AssignWaiter(Guid waiterId)
    {
        AssignedWaiterId = waiterId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new WaiterAssignedToTableEvent(
            waiterId,
            Id,
            CurrentReservationId,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Mettre à jour la configuration de la table
    /// </summary>
    public void UpdateConfiguration(
        TableCapacity? newCapacity = null,
        RestaurantZone? newZone = null,
        TablePosition? newPosition = null,
        string? description = null,
        bool? isSmokingAllowed = null,
        bool? hasView = null,
        bool? isAccessible = null,
        string? specialFeatures = null)
    {
        var hasChanges = false;

        if (newCapacity != null && !newCapacity.Equals(Capacity))
        {
            Capacity = newCapacity;
            hasChanges = true;
        }

        if (newZone.HasValue && newZone != Zone)
        {
            Zone = newZone.Value;
            hasChanges = true;
        }

        if (newPosition != null && !newPosition.Equals(Position))
        {
            Position = newPosition;
            hasChanges = true;
        }

        if (description != null)
        {
            Description = description.Trim();
            hasChanges = true;
        }

        if (isSmokingAllowed.HasValue && isSmokingAllowed != IsSmokingAllowed)
        {
            IsSmokingAllowed = isSmokingAllowed.Value;
            hasChanges = true;
        }

        if (hasView.HasValue && hasView != HasView)
        {
            HasView = hasView.Value;
            hasChanges = true;
        }

        if (isAccessible.HasValue && isAccessible != IsAccessible)
        {
            IsAccessible = isAccessible.Value;
            hasChanges = true;
        }

        if (specialFeatures != null)
        {
            SpecialFeatures = specialFeatures.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new TableConfigurationUpdatedEvent(
                Id,
                TableNumber,
                newCapacity,
                newZone,
                newPosition,
                UpdatedAt.Value));
        }
    }

    /// <summary>
    /// Ajouter un enregistrement de maintenance
    /// </summary>
    public void AddMaintenance(string maintenanceType, string description, Guid performedByUserId, decimal? cost = null)
    {
        if (string.IsNullOrWhiteSpace(maintenanceType))
            throw new ArgumentException("Maintenance type cannot be null or empty", nameof(maintenanceType));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        var maintenance = new TableMaintenance(Id, maintenanceType, description, performedByUserId, cost);
        _maintenanceHistory.Add(maintenance);

        if (Status != TableStatus.Maintenance)
        {
            ChangeStatus(TableStatus.OutOfService, performedByUserId, $"Maintenance: {maintenanceType}");
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Terminer la maintenance
    /// </summary>
    public void CompleteMaintenance(Guid completedByUserId, string? notes = null)
    {
        var lastMaintenance = _maintenanceHistory.LastOrDefault();
        if (lastMaintenance != null && !lastMaintenance.CompletedAt.HasValue)
        {
            lastMaintenance.Complete(notes);
        }

        ChangeStatus(TableStatus.Available, completedByUserId, "Maintenance completed");
    }

    /// <summary>
    /// Mettre à jour la note moyenne
    /// </summary>
    public void UpdateRating(decimal newRating)
    {
        if (newRating < 0 || newRating > 5)
            throw new ArgumentException("Rating must be between 0 and 5", nameof(newRating));

        // Calcul simple - en production, on utiliserait une moyenne pondérée
        AverageRating = AverageRating.HasValue 
            ? (AverageRating.Value + newRating) / 2 
            : newRating;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Désactiver la table
    /// </summary>
    public void Deactivate(string reason, Guid deactivatedByUserId)
    {
        if (!IsActive)
            throw new InvalidOperationException("Table is already deactivated");

        if (Status == TableStatus.Occupied || Status == TableStatus.Reserved)
            throw new InvalidOperationException("Cannot deactivate table that is occupied or reserved");

        IsActive = false;
        ChangeStatus(TableStatus.OutOfService, deactivatedByUserId, reason);
    }

    /// <summary>
    /// Réactiver la table
    /// </summary>
    public void Reactivate(Guid reactivatedByUserId)
    {
        if (IsActive)
            throw new InvalidOperationException("Table is already active");

        IsActive = true;
        ChangeStatus(TableStatus.Available, reactivatedByUserId, "Table reactivated");
    }

    /// <summary>
    /// Vérifier si la table est disponible pour réservation
    /// </summary>
    public bool IsAvailableForReservation => IsActive && Status == TableStatus.Available;

    /// <summary>
    /// Vérifier si la table nécessite un nettoyage
    /// </summary>
    public bool RequiresCleaning => Status == TableStatus.Cleaning;

    /// <summary>
    /// Obtenir le temps depuis le dernier nettoyage
    /// </summary>
    public TimeSpan? TimeSinceLastCleaning => LastCleaned.HasValue ? DateTime.UtcNow - LastCleaned.Value : null;

    /// <summary>
    /// Calculer l'efficacité de la table (revenus/temps d'occupation)
    /// </summary>
    public decimal CalculateEfficiency()
    {
        if (!DailyRevenue.HasValue || !AverageOccupationTime.HasValue || AverageOccupationTime.Value.TotalHours == 0)
            return 0;

        return DailyRevenue.Value / (decimal)AverageOccupationTime.Value.TotalHours;
    }

    /// <summary>
    /// Réinitialiser les métriques quotidiennes
    /// </summary>
    public void ResetDailyMetrics()
    {
        DailyRevenue = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateAverageOccupationTime(TimeSpan occupationDuration)
    {
        if (TotalReservations == 1)
        {
            AverageOccupationTime = occupationDuration;
        }
        else if (AverageOccupationTime.HasValue)
        {
            // Moyenne mobile simple
            var totalTime = AverageOccupationTime.Value.Multiply(TotalReservations - 1);
            AverageOccupationTime = totalTime.Add(occupationDuration).Divide(TotalReservations);
        }
    }

    private static void ValidateStatusTransition(TableStatus currentStatus, TableStatus newStatus)
    {
        var validTransitions = new Dictionary<TableStatus, TableStatus[]>
        {
            [TableStatus.Available] = new[] { TableStatus.Reserved, TableStatus.Occupied, TableStatus.Cleaning, TableStatus.OutOfService, TableStatus.Maintenance },
            [TableStatus.Reserved] = new[] { TableStatus.Occupied, TableStatus.Available, TableStatus.Cleaning },
            [TableStatus.Occupied] = new[] { TableStatus.Cleaning, TableStatus.Available },
            [TableStatus.Cleaning] = new[] { TableStatus.Available, TableStatus.OutOfService, TableStatus.Maintenance },
            [TableStatus.OutOfService] = new[] { TableStatus.Available, TableStatus.Maintenance },
            [TableStatus.Maintenance] = new[] { TableStatus.Available, TableStatus.OutOfService }
        };

        if (!validTransitions.ContainsKey(currentStatus) || !validTransitions[currentStatus].Contains(newStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}");
        }
    }
}

/// <summary>
/// Enregistrement de maintenance d'une table
/// </summary>
public sealed class TableMaintenance : Entity
{
    public Guid TableId { get; private set; }
    public string MaintenanceType { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid PerformedByUserId { get; private set; }
    public decimal? Cost { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletionNotes { get; private set; }

    private TableMaintenance() { } // EF Constructor

    public TableMaintenance(Guid tableId, string maintenanceType, string description, Guid performedByUserId, decimal? cost = null)
    {
        if (tableId == Guid.Empty)
            throw new ArgumentException("Table ID cannot be empty", nameof(tableId));
        
        if (string.IsNullOrWhiteSpace(maintenanceType))
            throw new ArgumentException("Maintenance type cannot be null or empty", nameof(maintenanceType));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty", nameof(performedByUserId));

        if (cost.HasValue && cost.Value < 0)
            throw new ArgumentException("Cost cannot be negative", nameof(cost));

        Id = Guid.NewGuid();
        TableId = tableId;
        MaintenanceType = maintenanceType.Trim();
        Description = description.Trim();
        PerformedByUserId = performedByUserId;
        Cost = cost;
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete(string? notes = null)
    {
        if (CompletedAt.HasValue)
            throw new InvalidOperationException("Maintenance is already completed");

        CompletedAt = DateTime.UtcNow;
        CompletionNotes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCompleted => CompletedAt.HasValue;
    
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : null;
}