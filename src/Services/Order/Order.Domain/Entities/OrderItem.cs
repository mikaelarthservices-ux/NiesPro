using NiesPro.Contracts.Primitives;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? ProductSku { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero();
    public Money TotalPrice => UnitPrice.Multiply(Quantity);
    
    // Propriétés de suivi
    public DateTime AddedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    // Navigation property
    public Guid OrderId { get; private set; }

    private OrderItem() { } // EF Constructor

    private OrderItem(
        Guid id,
        Guid productId,
        string productName,
        string? productSku,
        int quantity,
        Money unitPrice,
        Guid orderId) : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        OrderId = orderId;
        AddedAt = DateTime.UtcNow;
    }

    public static OrderItem Create(
        Guid productId,
        string productName,
        string? productSku,
        int quantity,
        Money unitPrice,
        Guid orderId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be null or empty", nameof(productName));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (unitPrice.Amount <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero", nameof(unitPrice));
        
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));

        return new OrderItem(
            Guid.NewGuid(),
            productId,
            productName,
            productSku,
            quantity,
            unitPrice,
            orderId
        );
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money newUnitPrice)
    {
        if (newUnitPrice.Amount <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero", nameof(newUnitPrice));

        if (UnitPrice.Currency != newUnitPrice.Currency)
            throw new InvalidOperationException($"Currency mismatch: {UnitPrice.Currency} vs {newUnitPrice.Currency}");

        UnitPrice = newUnitPrice;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsSameProduct(Guid productId) => ProductId == productId;

    public override string ToString() => $"{ProductName} x{Quantity} @ {UnitPrice} = {TotalPrice}";
}