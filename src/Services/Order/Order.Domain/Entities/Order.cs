using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

/// <summary>
/// Aggregate Root Enterprise pour Order - Architecture multi-contexte
/// Implémentation DDD avancée alignée sur NiesPro ERP standards
/// </summary>
public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    private readonly List<Payment> _payments = [];

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public CustomerInfo CustomerInfo { get; private set; } = null!;
    
    // Enterprise: Contexte métier et informations de service
    public BusinessContext BusinessContext { get; private set; }
    public ServiceInfo ServiceInfo { get; private set; } = null!;
    
    // Adresses selon contexte (optionnelles pour Restaurant/Boutique)
    public Address? ShippingAddress { get; private set; }
    public Address? BillingAddress { get; private set; }
    
    public OrderStatus Status { get; private set; }
    public Money SubTotal => CalculateSubTotal();
    public Money TaxAmount { get; private set; } = Money.Zero();
    public Money ShippingCost { get; private set; } = Money.Zero();
    public Money DiscountAmount { get; private set; } = Money.Zero();
    public Money TotalAmount => SubTotal.Add(TaxAmount).Add(ShippingCost).Subtract(DiscountAmount);
    
    // Propriétés de suivi - utilise Entity.CreatedAt de base
    public new DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    
    // Collections en lecture seule
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();

    private Order() { } // EF Constructor

    private Order(
        Guid id,
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        BusinessContext businessContext,
        ServiceInfo serviceInfo,
        Address? shippingAddress = null,
        Address? billingAddress = null) : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        CustomerInfo = customerInfo;
        BusinessContext = businessContext;
        ServiceInfo = serviceInfo;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        BillingAddress = billingAddress;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        // Raise domain event si ShippingAddress est disponible
        if (ShippingAddress != null)
        {
            RaiseDomainEvent(new OrderCreatedEvent(
                Id,
                CustomerId,
                CustomerInfo,
                ShippingAddress,
                BillingAddress,
                TotalAmount,
                CreatedAt
            ));
        }
    }

    /// <summary>
    /// Factory Enterprise - E-Commerce Context
    /// </summary>
    public static Order CreateECommerce(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        Address shippingAddress,
        Address? billingAddress = null,
        string? deliveryNotes = null)
    {
        ValidateCommonParameters(orderNumber, customerId);

        if (shippingAddress == null)
            throw new ArgumentException("Shipping address is required for e-commerce orders");

        var serviceInfo = ServiceInfo.CreateECommerce(
            shippingAddress.GetFullAddress(),
            customerInfo,
            ServiceType.Delivery);

        return new Order(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            customerInfo,
            BusinessContext.ECommerce,
            serviceInfo,
            shippingAddress,
            billingAddress);
    }

    /// <summary>
    /// Factory Enterprise - Restaurant Context
    /// </summary>
    public static Order CreateRestaurant(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        ServiceType serviceType,
        int? tableNumber = null,
        Guid? waiterId = null,
        string? serviceNotes = null)
    {
        ValidateCommonParameters(orderNumber, customerId);

        var serviceInfo = ServiceInfo.CreateRestaurant(
            serviceType,
            tableNumber,
            waiterId,
            serviceNotes);

        return new Order(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            customerInfo,
            BusinessContext.Restaurant,
            serviceInfo);
    }

    /// <summary>
    /// Factory Enterprise - Boutique Context
    /// </summary>
    public static Order CreateBoutique(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        Guid terminalId,
        string? serviceNotes = null)
    {
        ValidateCommonParameters(orderNumber, customerId);

        var serviceInfo = ServiceInfo.CreateBoutique(
            terminalId,
            ServiceType.InStore,
            serviceNotes);

        return new Order(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            customerInfo,
            BusinessContext.Boutique,
            serviceInfo);
    }

    /// <summary>
    /// Factory Enterprise - Wholesale Context
    /// </summary>
    public static Order CreateWholesale(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        string? deliveryAddress = null)
    {
        ValidateCommonParameters(orderNumber, customerId);

        var serviceInfo = ServiceInfo.CreateWholesale(
            customerInfo,
            deliveryAddress);

        return new Order(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            customerInfo,
            BusinessContext.Wholesale,
            serviceInfo,
            !string.IsNullOrEmpty(deliveryAddress) ? 
                Address.Create(deliveryAddress, "Ville", "00000", "Pays") : null);
    }

    /// <summary>
    /// Factory de compatibilité pour l'existant (E-Commerce par défaut)
    /// </summary>
    public static Order Create(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        Address shippingAddress,
        Address? billingAddress = null)
    {
        return CreateECommerce(orderNumber, customerId, customerInfo, 
            shippingAddress, billingAddress);
    }

    private static void ValidateCommonParameters(string orderNumber, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
    }

    public void AddItem(Guid productId, string productName, string? productSku, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot add items to order in {Status} status");

        var existingItem = _items.FirstOrDefault(i => i.IsSameProduct(productId));
        
        if (existingItem != null)
        {
            var oldQuantity = existingItem.Quantity;
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            
            RaiseDomainEvent(new OrderItemQuantityChangedEvent(
                Id,
                productId,
                oldQuantity,
                existingItem.Quantity,
                DateTime.UtcNow
            ));
        }
        else
        {
            var newItem = OrderItem.Create(productId, productName, productSku, quantity, unitPrice, Id);
            _items.Add(newItem);
            
            RaiseDomainEvent(new OrderItemAddedEvent(
                Id,
                productId,
                productName,
                quantity,
                unitPrice.Amount,
                DateTime.UtcNow
            ));
        }
    }

    public void RemoveItem(Guid productId, int quantity = int.MaxValue)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot remove items from order in {Status} status");

        var item = _items.FirstOrDefault(i => i.IsSameProduct(productId));
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        var quantityToRemove = Math.Min(quantity, item.Quantity);

        if (quantityToRemove == item.Quantity)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(item.Quantity - quantityToRemove);
        }

        RaiseDomainEvent(new OrderItemRemovedEvent(
            Id,
            productId,
            quantityToRemove,
            DateTime.UtcNow
        ));
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot update items in order in {Status} status");

        var item = _items.FirstOrDefault(i => i.IsSameProduct(productId));
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        if (newQuantity <= 0)
        {
            RemoveItem(productId);
            return;
        }

        var oldQuantity = item.Quantity;
        item.UpdateQuantity(newQuantity);

        RaiseDomainEvent(new OrderItemQuantityChangedEvent(
            Id,
            productId,
            oldQuantity,
            newQuantity,
            DateTime.UtcNow
        ));
    }

    public void SetTax(Money taxAmount)
    {
        if (taxAmount.Amount < 0)
            throw new ArgumentException("Tax amount cannot be negative", nameof(taxAmount));

        TaxAmount = taxAmount;
    }

    public void SetShippingCost(Money shippingCost)
    {
        if (shippingCost.Amount < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));

        ShippingCost = shippingCost;
    }

    public void ApplyDiscount(Money discountAmount)
    {
        if (discountAmount.Amount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        if (discountAmount.Amount > SubTotal.Amount)
            throw new ArgumentException("Discount cannot exceed subtotal", nameof(discountAmount));

        DiscountAmount = discountAmount;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        var previousStatus = Status;
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            "Order confirmed by customer",
            ConfirmedAt.Value
        ));
    }

    public void StartProcessing()
    {
        if (!Status.CanTransitionTo(OrderStatus.Processing))
            throw new InvalidOperationException($"Cannot start processing order in {Status} status");

        var previousStatus = Status;
        Status = OrderStatus.Processing;

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            "Order processing started",
            DateTime.UtcNow
        ));
    }

    public void MarkAsShipped(string? trackingNumber = null)
    {
        if (!Status.CanTransitionTo(OrderStatus.Shipped))
            throw new InvalidOperationException($"Cannot ship order in {Status} status");

        var previousStatus = Status;
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            $"Order shipped{(string.IsNullOrEmpty(trackingNumber) ? "" : $" - Tracking: {trackingNumber}")}",
            ShippedAt.Value
        ));
    }

    public void MarkAsDelivered()
    {
        if (!Status.CanTransitionTo(OrderStatus.Delivered))
            throw new InvalidOperationException($"Cannot deliver order in {Status} status");

        var previousStatus = Status;
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            "Order delivered successfully",
            DeliveredAt.Value
        ));
    }

    public void Cancel(string reason)
    {
        if (Status.IsTerminalStatus())
            throw new InvalidOperationException($"Cannot cancel order in terminal status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason is required", nameof(reason));

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            reason,
            CancelledAt.Value
        ));
    }

    public Payment AddPayment(PaymentMethod method)
    {
        if (TotalAmount.Amount <= 0)
            throw new InvalidOperationException("Cannot add payment for zero amount order");

        var payment = Payment.Create(method, TotalAmount, Id);
        _payments.Add(payment);

        return payment;
    }

    public Money GetPaidAmount()
    {
        return _payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Aggregate(Money.Zero(TotalAmount.Currency), (sum, payment) => sum.Add(payment.GetRemainingAmount()));
    }

    public bool IsFullyPaid() => GetPaidAmount().Amount >= TotalAmount.Amount;

    public bool HasItems() => _items.Any();

    public int GetTotalItemCount() => _items.Sum(i => i.Quantity);

    // =================== ENTERPRISE METHODS ===================

    /// <summary>
    /// Transition de statut Enterprise avec validation contextuelle
    /// </summary>
    public void TransitionToStatus(OrderStatus targetStatus, string? reason = null, Guid? performedBy = null)
    {
        if (!Status.CanTransitionTo(targetStatus, BusinessContext))
            throw new InvalidOperationException(
                $"Cannot transition from {Status} to {targetStatus} in {BusinessContext} context");

        var previousStatus = Status;
        Status = targetStatus;

        // Mise à jour des timestamps selon le statut
        UpdateTimestampsForStatus(targetStatus);

        // Événement Enterprise avec contexte
        RaiseDomainEvent(new OrderStatusChangedEvent(
            Id,
            previousStatus,
            Status,
            reason ?? $"Status changed to {targetStatus.GetDisplayName(BusinessContext)}",
            DateTime.UtcNow
        ));

        // Événements spécialisés selon contexte
        RaiseContextSpecificEvents(targetStatus, performedBy);
    }

    /// <summary>
    /// Obtient les transitions valides pour le contexte actuel
    /// </summary>
    public IEnumerable<OrderStatus> GetValidTransitions()
    {
        return Status.GetNextValidStatuses(BusinessContext);
    }

    /// <summary>
    /// Détermine si l'ordre nécessite une intégration cuisine
    /// </summary>
    public bool RequiresKitchenIntegration()
    {
        return BusinessContext.RequiresKitchenIntegration() && 
               Status == OrderStatus.Confirmed;
    }

    /// <summary>
    /// Détermine si l'ordre nécessite une réservation stock
    /// </summary>
    public bool RequiresInventoryReservation()
    {
        return BusinessContext.RequiresPOSIntegration() && 
               Status == OrderStatus.Confirmed;
    }

    /// <summary>
    /// Détermine si l'ordre nécessite une gestion de livraison
    /// </summary>
    public bool RequiresShippingManagement()
    {
        return BusinessContext.RequiresShippingManagement() && 
               Status == OrderStatus.Processing;
    }

    /// <summary>
    /// Met à jour les informations de service
    /// </summary>
    public void UpdateServiceInfo(ServiceInfo newServiceInfo)
    {
        if (newServiceInfo.Context != BusinessContext)
            throw new ArgumentException("Service info context must match order context");

        ServiceInfo = newServiceInfo;

        RaiseDomainEvent(new OrderServiceInfoUpdatedEvent(
            Id, 
            newServiceInfo, 
            DateTime.UtcNow));
    }

    /// <summary>
    /// Assigne un serveur (Restaurant uniquement)
    /// </summary>
    public void AssignWaiter(Guid waiterId)
    {
        if (BusinessContext != BusinessContext.Restaurant)
            throw new InvalidOperationException("Waiter assignment is only valid for restaurant orders");

        ServiceInfo = ServiceInfo.WithWaiter(waiterId);

        RaiseDomainEvent(new WaiterAssignedEvent(
            Id,
            waiterId,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Ajoute des notes de service
    /// </summary>
    public void AddServiceNotes(string notes)
    {
        ServiceInfo = ServiceInfo.WithNotes(notes);

        RaiseDomainEvent(new ServiceNotesUpdatedEvent(
            Id,
            notes,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Met à jour la durée estimée
    /// </summary>
    public void UpdateEstimatedDuration(TimeSpan duration)
    {
        ServiceInfo = ServiceInfo.WithEstimatedDuration(duration);

        RaiseDomainEvent(new EstimatedDurationUpdatedEvent(
            Id,
            duration,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Obtient le contexte d'affichage pour l'UI
    /// </summary>
    public OrderDisplayContext GetDisplayContext()
    {
        return new OrderDisplayContext(
            OrderNumber,
            Status,
            BusinessContext,
            ServiceInfo,
            CreatedAt,
            TotalAmount,
            Status.GetStatusColor(),
            Status.RequiresUserAction());
    }

    private void UpdateTimestampsForStatus(OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Confirmed:
                ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Shipped:
                ShippedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
            case OrderStatus.Served:
            case OrderStatus.Completed:
                DeliveredAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                CancelledAt = DateTime.UtcNow;
                break;
        }
    }

    private void RaiseContextSpecificEvents(OrderStatus status, Guid? performedBy)
    {
        switch (BusinessContext)
        {
            case BusinessContext.Restaurant when status == OrderStatus.KitchenQueue:
                RaiseDomainEvent(new OrderSentToKitchenEvent(Id, ServiceInfo.TableNumber, DateTime.UtcNow));
                break;
                
            case BusinessContext.Restaurant when status == OrderStatus.Ready:
                RaiseDomainEvent(new OrderReadyForServiceEvent(Id, ServiceInfo.TableNumber, ServiceInfo.WaiterId, DateTime.UtcNow));
                break;
                
            case BusinessContext.Boutique when status == OrderStatus.Scanned:
                RaiseDomainEvent(new OrderItemsScannedEvent(Id, ServiceInfo.TerminalId, GetTotalItemCount(), DateTime.UtcNow));
                break;
                
            case BusinessContext.ECommerce when status == OrderStatus.Shipped:
                RaiseDomainEvent(new OrderShippedEvent(Id, ShippingAddress?.GetFullAddress(), DateTime.UtcNow));
                break;
        }
    }

    private Money CalculateSubTotal()
    {
        if (!_items.Any())
            return Money.Zero();

        var currency = _items.First().UnitPrice.Currency;
        return _items.Aggregate(Money.Zero(currency), (sum, item) => sum.Add(item.TotalPrice));
    }

    public override string ToString() => 
        $"Order {OrderNumber} - {BusinessContext} - {Status.GetDisplayName(BusinessContext)} - {TotalAmount}";
}

/// <summary>
/// Contexte d'affichage pour l'UI Enterprise
/// </summary>
public record OrderDisplayContext(
    string OrderNumber,
    OrderStatus Status,
    BusinessContext Context,
    ServiceInfo ServiceInfo,
    DateTime CreatedAt,
    Money TotalAmount,
    string StatusColor,
    bool RequiresAction);