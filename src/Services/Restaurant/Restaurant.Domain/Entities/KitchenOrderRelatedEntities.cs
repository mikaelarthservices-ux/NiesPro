using BuildingBlocks.Domain;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

/// <summary>
/// Entité représentant un item d'une commande de cuisine
/// </summary>
public class KitchenOrderItem : Entity
{
    /// <summary>
    /// Identifiant de la commande de cuisine
    /// </summary>
    public Guid KitchenOrderId { get; private set; }

    /// <summary>
    /// Identifiant de l'item du menu
    /// </summary>
    public Guid MenuItemId { get; private set; }

    /// <summary>
    /// Nom de l'item du menu (snapshot pour l'historique)
    /// </summary>
    public string MenuItemName { get; private set; } = string.Empty;

    /// <summary>
    /// Quantité commandée
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Prix unitaire (snapshot pour l'historique)
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Statut de l'item
    /// </summary>
    public KitchenOrderItemStatus Status { get; private set; }

    /// <summary>
    /// Demandes spéciales pour cet item
    /// </summary>
    public string? SpecialRequests { get; private set; }

    /// <summary>
    /// Section de cuisine requise
    /// </summary>
    public KitchenSection? RequiredKitchenSection { get; private set; }

    /// <summary>
    /// Item compliqué à préparer
    /// </summary>
    public bool IsComplicated { get; private set; }

    /// <summary>
    /// Item urgent
    /// </summary>
    public bool IsUrgent { get; private set; }

    /// <summary>
    /// Allergènes présents
    /// </summary>
    public List<AllergenType> Allergens { get; private set; } = new();

    /// <summary>
    /// Exigences diététiques
    /// </summary>
    public List<string> DietaryRequirements { get; private set; } = new();

    /// <summary>
    /// Modifications apportées à l'item
    /// </summary>
    public List<string> Modifications { get; private set; } = new();

    // Constructeur privé pour EF Core
    private KitchenOrderItem() { }

    /// <summary>
    /// Constructeur pour créer un nouvel item de commande
    /// </summary>
    public KitchenOrderItem(
        Guid kitchenOrderId,
        Guid menuItemId,
        string menuItemName,
        int quantity,
        decimal unitPrice,
        string? specialRequests = null)
    {
        KitchenOrderId = kitchenOrderId;
        MenuItemId = menuItemId;
        MenuItemName = menuItemName ?? throw new ArgumentNullException(nameof(menuItemName));
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Unit price cannot be negative");
        SpecialRequests = specialRequests;
        Status = KitchenOrderItemStatus.Pending;
    }

    /// <summary>
    /// Marquer l'item comme en préparation
    /// </summary>
    public void StartPreparation()
    {
        if (Status != KitchenOrderItemStatus.Pending)
            throw new InvalidOperationException($"Cannot start preparation with status {Status}");

        Status = KitchenOrderItemStatus.InPreparation;
    }

    /// <summary>
    /// Marquer l'item comme prêt
    /// </summary>
    public void MarkAsReady()
    {
        if (Status != KitchenOrderItemStatus.InPreparation)
            throw new InvalidOperationException($"Cannot mark as ready with status {Status}");

        Status = KitchenOrderItemStatus.Ready;
    }

    /// <summary>
    /// Marquer l'item comme servi
    /// </summary>
    public void MarkAsServed()
    {
        if (Status != KitchenOrderItemStatus.Ready)
            throw new InvalidOperationException($"Cannot mark as served with status {Status}");

        Status = KitchenOrderItemStatus.Served;
    }

    /// <summary>
    /// Modifier la quantité
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        Quantity = newQuantity;
    }

    /// <summary>
    /// Ajouter des informations d'allergènes et diététiques
    /// </summary>
    public void SetDietaryInformation(
        List<AllergenType>? allergens = null,
        List<string>? dietaryRequirements = null)
    {
        if (allergens != null)
            Allergens = allergens;

        if (dietaryRequirements != null)
            DietaryRequirements = dietaryRequirements;
    }

    /// <summary>
    /// Marquer comme compliqué ou urgent
    /// </summary>
    public void SetPriorityFlags(bool isComplicated = false, bool isUrgent = false)
    {
        IsComplicated = isComplicated;
        IsUrgent = isUrgent;
    }
}

/// <summary>
/// Entité représentant un log d'action sur une commande de cuisine
/// </summary>
public class KitchenOrderLog : Entity
{
    /// <summary>
    /// Identifiant de la commande de cuisine
    /// </summary>
    public Guid KitchenOrderId { get; private set; }

    /// <summary>
    /// Action effectuée
    /// </summary>
    public KitchenOrderAction Action { get; private set; }

    /// <summary>
    /// Description détaillée de l'action
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Qui a effectué l'action
    /// </summary>
    public Guid? PerformedBy { get; private set; }

    /// <summary>
    /// Quand l'action a été effectuée
    /// </summary>
    public DateTime PerformedAt { get; private set; }

    // Constructeur privé pour EF Core
    private KitchenOrderLog() { }

    /// <summary>
    /// Constructeur pour créer un nouveau log
    /// </summary>
    public KitchenOrderLog(
        Guid kitchenOrderId,
        KitchenOrderAction action,
        string description,
        Guid? performedBy = null,
        DateTime? performedAt = null)
    {
        KitchenOrderId = kitchenOrderId;
        Action = action;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        PerformedBy = performedBy;
        PerformedAt = performedAt ?? DateTime.UtcNow;
    }
}

/// <summary>
/// Entité représentant une modification apportée à une commande de cuisine
/// </summary>
public class KitchenOrderModification : Entity
{
    /// <summary>
    /// Identifiant de la commande de cuisine
    /// </summary>
    public Guid KitchenOrderId { get; private set; }

    /// <summary>
    /// Identifiant de l'item modifié (optionnel)
    /// </summary>
    public Guid? ItemId { get; private set; }

    /// <summary>
    /// Date de la modification
    /// </summary>
    public DateTime ModifiedAt { get; private set; }

    /// <summary>
    /// Raison de la modification
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// Ancienne quantité (si applicable)
    /// </summary>
    public int? OldQuantity { get; private set; }

    /// <summary>
    /// Nouvelle quantité (si applicable)
    /// </summary>
    public int? NewQuantity { get; private set; }

    /// <summary>
    /// Anciennes demandes spéciales (si applicable)
    /// </summary>
    public string? OldSpecialRequests { get; private set; }

    /// <summary>
    /// Nouvelles demandes spéciales (si applicable)
    /// </summary>
    public string? NewSpecialRequests { get; private set; }

    // Constructeur privé pour EF Core
    private KitchenOrderModification() { }

    /// <summary>
    /// Constructeur pour créer une nouvelle modification
    /// </summary>
    public KitchenOrderModification(
        Guid kitchenOrderId,
        string reason,
        Guid? itemId = null,
        DateTime? modifiedAt = null)
    {
        KitchenOrderId = kitchenOrderId;
        ItemId = itemId;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        ModifiedAt = modifiedAt ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Enregistrer une modification de quantité
    /// </summary>
    public void RecordQuantityChange(int oldQuantity, int newQuantity)
    {
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }

    /// <summary>
    /// Enregistrer une modification de demandes spéciales
    /// </summary>
    public void RecordSpecialRequestsChange(string? oldRequests, string? newRequests)
    {
        OldSpecialRequests = oldRequests;
        NewSpecialRequests = newRequests;
    }
}