using NiesPro.Contracts.Infrastructure;
using NiesPro.Contracts.Primitives;
using Restaurant.Domain.Enums;
using Restaurant.Domain.Events;
using Restaurant.Domain.ValueObjects;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant une commande cuisine
/// </summary>
public sealed class KitchenOrder : Entity, IAggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid TableId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? WaiterId { get; private set; }
    public Guid? ChefId { get; private set; }
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }
    public OrderPriority Priority { get; private set; }
    public KitchenSection KitchenSection { get; private set; }

    // Informations de timing
    public DateTime OrderedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? ReadyAt { get; private set; }
    public DateTime? ServedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    // Temps de préparation
    public PreparationTime EstimatedPreparationTime { get; private set; }
    public TimeSpan? ActualPreparationTime { get; private set; }
    public DateTime EstimatedReadyTime { get; private set; }

    // Instructions et notes
    public string? SpecialInstructions { get; private set; }
    public string? CustomerNotes { get; private set; }
    public string? ChefNotes { get; private set; }
    public string? CancellationReason { get; private set; }

    // Métrique et qualité
    public bool IsRush { get; private set; }
    public bool IsComplicated { get; private set; }
    public bool RequiresSpecialAttention { get; private set; }
    public QualityLevel? QualityLevel { get; private set; }
    public int? CustomerRating { get; private set; }
    public string? CustomerFeedback { get; private set; }

    // Gestion des allergies et régimes
    public List<AllergenType> Allergens { get; private set; } = new();
    public List<string> DietaryRequirements { get; private set; } = new();
    public bool RequiresAllergenAttention { get; private set; }

    // Prix et coûts
    public decimal SubTotal { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? TipAmount { get; private set; }

    private readonly List<KitchenOrderItem> _items = new();
    public IReadOnlyList<KitchenOrderItem> Items => _items.AsReadOnly();

    private readonly List<KitchenOrderLog> _logs = new();
    public IReadOnlyList<KitchenOrderLog> Logs => _logs.AsReadOnly();

    private readonly List<KitchenOrderModification> _modifications = new();
    public IReadOnlyList<KitchenOrderModification> Modifications => _modifications.AsReadOnly();

    private KitchenOrder() { } // EF Constructor

    public KitchenOrder(
        Guid tableId,
        OrderType orderType,
        List<KitchenOrderItemRequest> itemRequests,
        Guid? customerId = null,
        Guid? waiterId = null,
        string? specialInstructions = null)
    {
        if (tableId == Guid.Empty)
            throw new ArgumentException("Table ID cannot be empty", nameof(tableId));

        if (itemRequests == null || !itemRequests.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(itemRequests));

        Id = Guid.NewGuid();
        OrderNumber = GenerateOrderNumber();
        TableId = tableId;
        CustomerId = customerId;
        WaiterId = waiterId;
        OrderType = orderType;
        Status = OrderStatus.Pending;
        Priority = OrderPriority.Normal;
        OrderedAt = DateTime.UtcNow;
        SpecialInstructions = specialInstructions?.Trim();
        CreatedAt = DateTime.UtcNow;

        // Ajouter les éléments de commande
        foreach (var request in itemRequests)
        {
            AddItem(request.MenuItemId, request.Quantity, request.SpecialRequests, request.Modifications);
        }

        // Calculer le temps de préparation estimé et la section cuisine
        CalculateEstimatedTime();
        DetermineKitchenSection();
        CalculateTotals();

        // Déterminer la priorité automatiquement
        DeterminePriority();

        // Analyser les allergènes
        AnalyzeAllergens();

        EstimatedReadyTime = OrderedAt.Add(EstimatedPreparationTime.TotalTime);

        AddDomainEvent(new KitchenOrderPlacedEvent(
            Id,
            OrderNumber,
            TableId,
            CustomerId,
            OrderType,
            _items.Count,
            TotalAmount,
            EstimatedPreparationTime,
            OrderedAt));

        LogAction(OrderAction.Placed, "Commande créée", waiterId);
    }

    /// <summary>
    /// Accepter la commande
    /// </summary>
    public void Accept(Guid chefId, string? notes = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot accept order in status {Status}");

        ChefId = chefId;
        Status = OrderStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        ChefNotes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KitchenOrderAcceptedEvent(
            Id,
            OrderNumber,
            chefId,
            KitchenSection,
            AcceptedAt.Value));

        LogAction(OrderAction.Accepted, "Commande acceptée par le chef", chefId);
    }

    /// <summary>
    /// Démarrer la préparation
    /// </summary>
    public void StartPreparation(Guid? chefId = null)
    {
        if (Status != OrderStatus.Accepted)
            throw new InvalidOperationException($"Cannot start preparation for order in status {Status}");

        if (chefId.HasValue)
            ChefId = chefId.Value;

        Status = OrderStatus.InPreparation;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KitchenOrderPreparationStartedEvent(
            Id,
            OrderNumber,
            ChefId ?? Guid.Empty,
            StartedAt.Value,
            EstimatedReadyTime));

        LogAction(OrderAction.PreparationStarted, "Préparation commencée", ChefId);
    }

    /// <summary>
    /// Marquer comme prêt
    /// </summary>
    public void MarkAsReady(Guid? chefId = null, QualityLevel? qualityLevel = null)
    {
        if (Status != OrderStatus.InPreparation)
            throw new InvalidOperationException($"Cannot mark as ready order in status {Status}");

        if (chefId.HasValue)
            ChefId = chefId.Value;

        Status = OrderStatus.Ready;
        ReadyAt = DateTime.UtcNow;
        QualityLevel = qualityLevel;
        UpdatedAt = DateTime.UtcNow;

        // Calculer le temps de préparation réel
        if (StartedAt.HasValue)
            ActualPreparationTime = ReadyAt.Value - StartedAt.Value;

        AddDomainEvent(new KitchenOrderReadyEvent(
            Id,
            OrderNumber,
            TableId,
            ReadyAt.Value,
            ActualPreparationTime,
            qualityLevel));

        LogAction(OrderAction.Ready, "Commande prête à servir", ChefId);

        // Événement d'alerte si le plat refroidit
        Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ =>
        {
            if (Status == OrderStatus.Ready) // Toujours pas servi
            {
                AddDomainEvent(new KitchenOrderCoolingDownEvent(
                    Id,
                    OrderNumber,
                    TableId,
                    DateTime.UtcNow));
            }
        });
    }

    /// <summary>
    /// Marquer comme servi
    /// </summary>
    public void MarkAsServed(Guid? waiterId = null)
    {
        if (Status != OrderStatus.Ready)
            throw new InvalidOperationException($"Cannot mark as served order in status {Status}");

        if (waiterId.HasValue)
            WaiterId = waiterId.Value;

        Status = OrderStatus.Served;
        ServedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KitchenOrderServedEvent(
            Id,
            OrderNumber,
            TableId,
            ServedAt.Value,
            WaiterId));

        LogAction(OrderAction.Served, "Commande servie au client", WaiterId);
    }

    /// <summary>
    /// Compléter la commande
    /// </summary>
    public void Complete(int? customerRating = null, string? customerFeedback = null, decimal? tipAmount = null)
    {
        if (Status != OrderStatus.Served)
            throw new InvalidOperationException($"Cannot complete order in status {Status}");

        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CustomerRating = customerRating;
        CustomerFeedback = customerFeedback?.Trim();
        TipAmount = tipAmount;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KitchenOrderCompletedEvent(
            Id,
            OrderNumber,
            CompletedAt.Value,
            CustomerRating,
            TipAmount,
            GetTotalPreparationTime()));

        LogAction(OrderAction.Completed, "Commande terminée", null);
    }

    /// <summary>
    /// Annuler la commande
    /// </summary>
    public void Cancel(string reason, Guid? cancelledById = null)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel order in status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new KitchenOrderCancelledEvent(
            Id,
            OrderNumber,
            previousStatus,
            reason,
            CancelledAt.Value,
            cancelledById));

        LogAction(OrderAction.Cancelled, reason, cancelledById);
    }

    /// <summary>
    /// Changer la priorité
    /// </summary>
    public void ChangePriority(OrderPriority newPriority, string? reason = null, Guid? changedById = null)
    {
        var previousPriority = Priority;
        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;

        // Recalculer le temps estimé si priorité élevée
        if (newPriority == OrderPriority.High || newPriority == OrderPriority.Urgent)
        {
            var reductionFactor = newPriority == OrderPriority.Urgent ? 0.7 : 0.85;
            EstimatedPreparationTime = new PreparationTime(
                TimeSpan.FromMinutes(EstimatedPreparationTime.TotalTime.TotalMinutes * reductionFactor),
                EstimatedPreparationTime.ActiveTime);
            
            EstimatedReadyTime = (StartedAt ?? OrderedAt).Add(EstimatedPreparationTime.TotalTime);
        }

        AddDomainEvent(new KitchenOrderPriorityChangedEvent(
            Id,
            OrderNumber,
            previousPriority,
            newPriority,
            reason,
            UpdatedAt.Value,
            changedById));

        LogAction(OrderAction.PriorityChanged, reason ?? $"Priorité changée de {previousPriority} à {newPriority}", changedById);
    }

    /// <summary>
    /// Ajouter un élément à la commande
    /// </summary>
    public void AddItem(Guid menuItemId, int quantity, string? specialRequests = null, List<string>? modifications = null)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to non-pending order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var orderItem = new KitchenOrderItem(
            Id,
            menuItemId,
            quantity,
            specialRequests,
            modifications);

        _items.Add(orderItem);
        
        // Recalculer les totaux et temps
        CalculateTotals();
        CalculateEstimatedTime();
        DetermineKitchenSection();
        AnalyzeAllergens();
        
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retirer un élément de la commande
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from non-pending order");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            CalculateTotals();
            CalculateEstimatedTime();
            DetermineKitchenSection();
            AnalyzeAllergens();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Modifier un élément de la commande
    /// </summary>
    public void ModifyItem(Guid itemId, int? newQuantity = null, string? newSpecialRequests = null)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify completed or cancelled order");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new ArgumentException("Item not found", nameof(itemId));

        var modification = new KitchenOrderModification(
            Id,
            itemId,
            item.Quantity,
            newQuantity ?? item.Quantity,
            item.SpecialRequests,
            newSpecialRequests ?? item.SpecialRequests,
            DateTime.UtcNow,
            "Item modified by kitchen request");

        _modifications.Add(modification);

        if (newQuantity.HasValue)
            item.UpdateQuantity(newQuantity.Value);

        if (newSpecialRequests != null)
            item.UpdateSpecialRequests(newSpecialRequests);

        CalculateTotals();
        UpdatedAt = DateTime.UtcNow;

        LogAction(OrderAction.Modified, "Élément modifié", null);
    }

    /// <summary>
    /// Appliquer une remise
    /// </summary>
    public void ApplyDiscount(decimal discountAmount, string reason)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        if (discountAmount > SubTotal)
            throw new ArgumentException("Discount amount cannot exceed subtotal", nameof(discountAmount));

        DiscountAmount = discountAmount;
        TotalAmount = SubTotal - discountAmount;
        UpdatedAt = DateTime.UtcNow;

        LogAction(OrderAction.DiscountApplied, $"Remise appliquée: {discountAmount:C} - {reason}", null);
    }

    /// <summary>
    /// Obtenir le temps total de préparation
    /// </summary>
    public TimeSpan? GetTotalPreparationTime()
    {
        if (OrderedAt != null && CompletedAt.HasValue)
            return CompletedAt.Value - OrderedAt;
        
        if (OrderedAt != null && ReadyAt.HasValue)
            return ReadyAt.Value - OrderedAt;

        return null;
    }

    /// <summary>
    /// Vérifier si la commande est en retard
    /// </summary>
    public bool IsOverdue => DateTime.UtcNow > EstimatedReadyTime && Status != OrderStatus.Ready && Status != OrderStatus.Served && Status != OrderStatus.Completed;

    /// <summary>
    /// Calculer le pourcentage de progression
    /// </summary>
    public int ProgressPercentage
    {
        get
        {
            return Status switch
            {
                OrderStatus.Pending => 0,
                OrderStatus.Accepted => 20,
                OrderStatus.InPreparation => 60,
                OrderStatus.Ready => 90,
                OrderStatus.Served => 95,
                OrderStatus.Completed => 100,
                OrderStatus.Cancelled => 0,
                _ => 0
            };
        }
    }

    private void CalculateEstimatedTime()
    {
        if (!_items.Any())
        {
            EstimatedPreparationTime = new PreparationTime(TimeSpan.Zero, TimeSpan.Zero);
            return;
        }

        // Temps de base par nombre d'éléments
        var baseTime = TimeSpan.FromMinutes(_items.Count * 2);
        
        // Temps supplémentaire pour éléments compliqués
        var complexityTime = TimeSpan.FromMinutes(_items.Count(i => i.IsComplicated) * 5);
        
        // Temps supplémentaire pour modifications spéciales
        var modificationTime = TimeSpan.FromMinutes(_items.Sum(i => i.ModificationCount) * 2);

        var totalTime = baseTime + complexityTime + modificationTime;
        var activeTime = TimeSpan.FromMinutes(totalTime.TotalMinutes * 0.8); // 80% du temps total est actif

        EstimatedPreparationTime = new PreparationTime(totalTime, activeTime);
    }

    private void DetermineKitchenSection()
    {
        // Simplification: déterminer la section principale
        var sections = _items
            .Select(i => i.RequiredKitchenSection)
            .Where(s => s.HasValue)
            .GroupBy(s => s.Value)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        KitchenSection = sections?.Key ?? KitchenSection.Hot;
    }

    private void CalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalPrice);
        TotalAmount = SubTotal - (DiscountAmount ?? 0);
    }

    private void DeterminePriority()
    {
        // Logique automatique de priorité
        if (_items.Any(i => i.IsUrgent))
            Priority = OrderPriority.Urgent;
        else if (OrderType == OrderType.TakeAway || _items.Count >= 5)
            Priority = OrderPriority.High;
        else if (_items.Any(i => i.IsComplicated))
            Priority = OrderPriority.Normal;
        else
            Priority = OrderPriority.Low;
    }

    private void AnalyzeAllergens()
    {
        Allergens.Clear();
        DietaryRequirements.Clear();

        foreach (var item in _items)
        {
            foreach (var allergen in item.Allergens)
            {
                if (!Allergens.Contains(allergen))
                    Allergens.Add(allergen);
            }

            foreach (var requirement in item.DietaryRequirements)
            {
                if (!DietaryRequirements.Contains(requirement))
                    DietaryRequirements.Add(requirement);
            }
        }

        RequiresAllergenAttention = Allergens.Any();
    }

    private void LogAction(OrderAction action, string description, Guid? performedBy)
    {
        var log = new KitchenOrderLog(Id, action, description, performedBy);
        _logs.Add(log);
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var random = new Random().Next(100, 999);
        return $"ORD{timestamp}{random}";
    }
}

/// <summary>
/// Élément d'une commande cuisine
/// </summary>
public sealed class KitchenOrderItem : Entity
{
    public Guid KitchenOrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string MenuItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? SpecialRequests { get; private set; }
    public KitchenSection? RequiredKitchenSection { get; private set; }
    public PreparationTime EstimatedPreparationTime { get; private set; }
    public ItemStatus Status { get; private set; }
    public bool IsComplicated { get; private set; }
    public bool IsUrgent { get; private set; }
    public List<AllergenType> Allergens { get; private set; } = new();
    public List<string> DietaryRequirements { get; private set; } = new();
    public List<string> Modifications { get; private set; } = new();
    public int ModificationCount => Modifications.Count;

    private KitchenOrderItem() { } // EF Constructor

    public KitchenOrderItem(
        Guid kitchenOrderId,
        Guid menuItemId,
        int quantity,
        string? specialRequests = null,
        List<string>? modifications = null)
    {
        Id = Guid.NewGuid();
        KitchenOrderId = kitchenOrderId;
        MenuItemId = menuItemId;
        Quantity = quantity;
        SpecialRequests = specialRequests?.Trim();
        Status = ItemStatus.Pending;
        Modifications = modifications?.ToList() ?? new List<string>();
        Allergens = new List<AllergenType>();
        DietaryRequirements = new List<string>();
        
        // Ces propriétés seraient normalement remplies via une relation avec MenuItem
        MenuItemName = "Item Name"; // Temporaire
        UnitPrice = 10.00m; // Temporaire
        EstimatedPreparationTime = new PreparationTime(TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(12));
        RequiredKitchenSection = KitchenSection.Hot;
        
        CreatedAt = DateTime.UtcNow;
        
        AnalyzeComplexity();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSpecialRequests(string? newSpecialRequests)
    {
        SpecialRequests = newSpecialRequests?.Trim();
        UpdatedAt = DateTime.UtcNow;
        AnalyzeComplexity();
    }

    public void ChangeStatus(ItemStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private void AnalyzeComplexity()
    {
        IsComplicated = !string.IsNullOrEmpty(SpecialRequests) || Modifications.Any();
        IsUrgent = SpecialRequests?.ToLowerInvariant().Contains("urgent") == true ||
                   SpecialRequests?.ToLowerInvariant().Contains("rush") == true;
    }
}

/// <summary>
/// Journal des actions sur une commande
/// </summary>
public sealed class KitchenOrderLog : Entity
{
    public Guid KitchenOrderId { get; private set; }
    public OrderAction Action { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? PerformedBy { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private KitchenOrderLog() { } // EF Constructor

    public KitchenOrderLog(Guid kitchenOrderId, OrderAction action, string description, Guid? performedBy)
    {
        Id = Guid.NewGuid();
        KitchenOrderId = kitchenOrderId;
        Action = action;
        Description = description;
        PerformedBy = performedBy;
        PerformedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Modification apportée à une commande
/// </summary>
public sealed class KitchenOrderModification : Entity
{
    public Guid KitchenOrderId { get; private set; }
    public Guid? ItemId { get; private set; }
    public int? OldQuantity { get; private set; }
    public int? NewQuantity { get; private set; }
    public string? OldSpecialRequests { get; private set; }
    public string? NewSpecialRequests { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private KitchenOrderModification() { } // EF Constructor

    public KitchenOrderModification(
        Guid kitchenOrderId,
        Guid? itemId,
        int? oldQuantity,
        int? newQuantity,
        string? oldSpecialRequests,
        string? newSpecialRequests,
        DateTime modifiedAt,
        string reason)
    {
        Id = Guid.NewGuid();
        KitchenOrderId = kitchenOrderId;
        ItemId = itemId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
        OldSpecialRequests = oldSpecialRequests;
        NewSpecialRequests = newSpecialRequests;
        ModifiedAt = modifiedAt;
        Reason = reason;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Requête pour ajouter un élément à une commande
/// </summary>
public record KitchenOrderItemRequest(
    Guid MenuItemId,
    int Quantity,
    string? SpecialRequests = null,
    List<string>? Modifications = null);