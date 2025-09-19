using BuildingBlocks.Domain;
using Stock.Domain.Enums;
using Stock.Domain.Events;
using Stock.Domain.ValueObjects;

namespace Stock.Domain.Entities;

/// <summary>
/// Entité représentant un bon de commande fournisseur
/// </summary>
public sealed class PurchaseOrder : Entity, IAggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime ExpectedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public Money TotalAmount { get; private set; } = Money.Zero();
    public string? Notes { get; private set; }
    public string? DeliveryAddress { get; private set; }
    public Guid UserId { get; private set; }

    // Informations de réception
    public DateTime? ReceivedAt { get; private set; }
    public Guid? ReceivedByUserId { get; private set; }
    public string? ReceiptNotes { get; private set; }

    // Informations d'annulation
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private readonly List<PurchaseOrderLine> _lines = new();
    public IReadOnlyList<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder() { } // EF Constructor

    public PurchaseOrder(
        string orderNumber,
        Guid supplierId,
        DateTime expectedDeliveryDate,
        Guid userId,
        string? notes = null,
        string? deliveryAddress = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));
        
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier ID cannot be empty", nameof(supplierId));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (expectedDeliveryDate <= DateTime.UtcNow)
            throw new ArgumentException("Expected delivery date must be in the future", nameof(expectedDeliveryDate));

        Id = Guid.NewGuid();
        OrderNumber = orderNumber.Trim();
        SupplierId = supplierId;
        Status = PurchaseOrderStatus.Draft;
        OrderDate = DateTime.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate;
        UserId = userId;
        Notes = notes?.Trim();
        DeliveryAddress = deliveryAddress?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter une ligne de commande
    /// </summary>
    public void AddLine(Guid productId, StockQuantity quantity, UnitCost unitCost, string? notes = null)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to order with status {Status}");

        // Vérifier si le produit existe déjà
        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine != null)
        {
            existingLine.UpdateQuantity(existingLine.OrderedQuantity.Add(quantity));
            existingLine.UpdateUnitCost(unitCost);
        }
        else
        {
            var line = new PurchaseOrderLine(Id, productId, quantity, unitCost, notes);
            _lines.Add(line);
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer une ligne de commande
    /// </summary>
    public void RemoveLine(Guid productId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot remove lines from order with status {Status}");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line != null)
        {
            _lines.Remove(line);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Mettre à jour une ligne de commande
    /// </summary>
    public void UpdateLine(Guid productId, StockQuantity? quantity = null, UnitCost? unitCost = null, string? notes = null)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot update lines of order with status {Status}");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        if (quantity != null)
            line.UpdateQuantity(quantity);

        if (unitCost != null)
            line.UpdateUnitCost(unitCost);

        if (notes != null)
            line.UpdateNotes(notes);

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirmer la commande
    /// </summary>
    public void Confirm()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot confirm order with status {Status}");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot confirm order without lines");

        Status = PurchaseOrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PurchaseOrderCreatedEvent(
            Id,
            SupplierId,
            OrderNumber,
            TotalAmount,
            ExpectedDeliveryDate,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Envoyer la commande au fournisseur
    /// </summary>
    public void Send()
    {
        if (Status != PurchaseOrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot send order with status {Status}");

        Status = PurchaseOrderStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marquer comme reçue
    /// </summary>
    public void MarkAsReceived(Guid receivedByUserId, string? receiptNotes = null)
    {
        if (Status != PurchaseOrderStatus.Sent)
            throw new InvalidOperationException($"Cannot receive order with status {Status}");

        Status = PurchaseOrderStatus.Received;
        ActualDeliveryDate = DateTime.UtcNow;
        ReceivedAt = DateTime.UtcNow;
        ReceivedByUserId = receivedByUserId;
        ReceiptNotes = receiptNotes?.Trim();
        UpdatedAt = DateTime.UtcNow;

        // Marquer toutes les lignes comme reçues
        foreach (var line in _lines)
        {
            line.MarkAsReceived(line.OrderedQuantity);
        }

        AddDomainEvent(new PurchaseOrderReceivedEvent(
            Id,
            SupplierId,
            OrderNumber,
            TotalAmount,
            ReceivedAt.Value));
    }

    /// <summary>
    /// Réception partielle
    /// </summary>
    public void ReceivePartially(Dictionary<Guid, StockQuantity> receivedQuantities, Guid receivedByUserId, string? receiptNotes = null)
    {
        if (Status != PurchaseOrderStatus.Sent)
            throw new InvalidOperationException($"Cannot receive order with status {Status}");

        bool hasReceivedItems = false;

        foreach (var kvp in receivedQuantities)
        {
            var line = _lines.FirstOrDefault(l => l.ProductId == kvp.Key);
            if (line != null && kvp.Value.IsPositive)
            {
                line.ReceiveQuantity(kvp.Value);
                hasReceivedItems = true;
            }
        }

        if (hasReceivedItems)
        {
            Status = PurchaseOrderStatus.PartiallyReceived;
            ReceivedAt = DateTime.UtcNow;
            ReceivedByUserId = receivedByUserId;
            ReceiptNotes = receiptNotes?.Trim();
            UpdatedAt = DateTime.UtcNow;

            // Vérifier si tout a été reçu
            if (_lines.All(l => l.IsFullyReceived))
            {
                Status = PurchaseOrderStatus.Received;
                ActualDeliveryDate = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Annuler la commande
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == PurchaseOrderStatus.Received)
            throw new InvalidOperationException("Cannot cancel a received order");

        if (Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        Status = PurchaseOrderStatus.Cancelled;
        CancellationReason = reason.Trim();
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PurchaseOrderCancelledEvent(
            Id,
            SupplierId,
            OrderNumber,
            CancellationReason,
            CancelledAt.Value));
    }

    /// <summary>
    /// Vérifier si la commande est en retard
    /// </summary>
    public bool IsOverdue => Status == PurchaseOrderStatus.Sent && DateTime.UtcNow > ExpectedDeliveryDate;

    /// <summary>
    /// Obtenir le délai de retard
    /// </summary>
    public TimeSpan? GetOverduePeriod()
    {
        if (!IsOverdue) return null;
        return DateTime.UtcNow - ExpectedDeliveryDate;
    }

    /// <summary>
    /// Mettre à jour la date de livraison prévue
    /// </summary>
    public void UpdateExpectedDeliveryDate(DateTime newDate)
    {
        if (Status == PurchaseOrderStatus.Received || Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot update delivery date for order with status {Status}");

        if (newDate <= DateTime.UtcNow)
            throw new ArgumentException("Expected delivery date must be in the future", nameof(newDate));

        ExpectedDeliveryDate = newDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        if (!_lines.Any())
        {
            TotalAmount = Money.Zero(TotalAmount.Currency);
            return;
        }

        var currency = _lines.First().TotalCost.Currency;
        var total = _lines.Aggregate(
            Money.Zero(currency),
            (sum, line) => sum.Add(line.TotalCost));

        TotalAmount = total;
    }
}

/// <summary>
/// Ligne de commande d'achat
/// </summary>
public sealed class PurchaseOrderLine : Entity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public StockQuantity OrderedQuantity { get; private set; }
    public StockQuantity ReceivedQuantity { get; private set; }
    public UnitCost UnitCost { get; private set; }
    public Money TotalCost { get; private set; }
    public string? Notes { get; private set; }

    private PurchaseOrderLine() { } // EF Constructor

    public PurchaseOrderLine(
        Guid purchaseOrderId,
        Guid productId,
        StockQuantity orderedQuantity,
        UnitCost unitCost,
        string? notes = null)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order ID cannot be empty", nameof(purchaseOrderId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        Id = Guid.NewGuid();
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        OrderedQuantity = orderedQuantity ?? throw new ArgumentNullException(nameof(orderedQuantity));
        ReceivedQuantity = StockQuantity.Zero(orderedQuantity.Unit);
        UnitCost = unitCost ?? throw new ArgumentNullException(nameof(unitCost));
        TotalCost = unitCost.CalculateTotal(orderedQuantity);
        Notes = notes?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateQuantity(StockQuantity newQuantity)
    {
        OrderedQuantity = newQuantity ?? throw new ArgumentNullException(nameof(newQuantity));
        TotalCost = UnitCost.CalculateTotal(OrderedQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateUnitCost(UnitCost newUnitCost)
    {
        UnitCost = newUnitCost ?? throw new ArgumentNullException(nameof(newUnitCost));
        TotalCost = UnitCost.CalculateTotal(OrderedQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReceiveQuantity(StockQuantity quantity)
    {
        if (quantity.Unit != OrderedQuantity.Unit)
            throw new ArgumentException($"Received quantity unit ({quantity.Unit}) must match ordered quantity unit ({OrderedQuantity.Unit})");

        var newReceivedQuantity = ReceivedQuantity.Add(quantity);
        if (newReceivedQuantity.Value > OrderedQuantity.Value)
            throw new ArgumentException("Total received quantity cannot exceed ordered quantity");

        ReceivedQuantity = newReceivedQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReceived(StockQuantity receivedQuantity)
    {
        ReceivedQuantity = receivedQuantity ?? throw new ArgumentNullException(nameof(receivedQuantity));
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsFullyReceived => ReceivedQuantity.Value >= OrderedQuantity.Value;
    
    public StockQuantity GetRemainingQuantity() => OrderedQuantity.Subtract(ReceivedQuantity);
}