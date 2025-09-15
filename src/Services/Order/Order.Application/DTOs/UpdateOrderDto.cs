using Order.Domain.Enums;

namespace Order.Application.DTOs;

public sealed record UpdateOrderStatusDto
{
    public Guid OrderId { get; init; }
    public OrderStatus NewStatus { get; init; }
    public string? Reason { get; init; }
    public string? TrackingNumber { get; init; }
}

public sealed record AddOrderItemDto
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = "EUR";
}

public sealed record UpdateOrderItemDto
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int NewQuantity { get; init; }
}

public sealed record RemoveOrderItemDto
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; } = int.MaxValue;
}

public sealed record ProcessPaymentDto
{
    public Guid OrderId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? TransactionId { get; init; }
    public string? ProviderReference { get; init; }
}