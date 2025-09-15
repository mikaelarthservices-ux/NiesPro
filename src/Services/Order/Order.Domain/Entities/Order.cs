using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    private readonly List<Payment> _payments = [];

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public CustomerInfo CustomerInfo { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Address? BillingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money SubTotal => CalculateSubTotal();
    public Money TaxAmount { get; private set; } = Money.Zero();
    public Money ShippingCost { get; private set; } = Money.Zero();
    public Money DiscountAmount { get; private set; } = Money.Zero();
    public Money TotalAmount => SubTotal.Add(TaxAmount).Add(ShippingCost).Subtract(DiscountAmount);
    
    // Propriétés de suivi
    public DateTime CreatedAt { get; private set; }
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
        Address shippingAddress,
        Address? billingAddress = null) : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        CustomerInfo = customerInfo;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
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

    public static Order Create(
        string orderNumber,
        Guid customerId,
        CustomerInfo customerInfo,
        Address shippingAddress,
        Address? billingAddress = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));

        return new Order(
            Guid.NewGuid(),
            orderNumber,
            customerId,
            customerInfo,
            shippingAddress,
            billingAddress
        );
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

    private Money CalculateSubTotal()
    {
        if (!_items.Any())
            return Money.Zero();

        var currency = _items.First().UnitPrice.Currency;
        return _items.Aggregate(Money.Zero(currency), (sum, item) => sum.Add(item.TotalPrice));
    }

    public override string ToString() => $"Order {OrderNumber} - {Status} - {TotalAmount}";
}