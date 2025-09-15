using Order.Domain.Enums;

namespace Order.Application.DTOs;

public sealed record OrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public CustomerInfoDto CustomerInfo { get; init; } = null!;
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto? BillingAddress { get; init; }
    public OrderStatus Status { get; init; }
    public List<OrderItemResponseDto> Items { get; init; } = [];
    public MoneyDto SubTotal { get; init; } = null!;
    public MoneyDto TaxAmount { get; init; } = null!;
    public MoneyDto ShippingCost { get; init; } = null!;
    public MoneyDto DiscountAmount { get; init; } = null!;
    public MoneyDto TotalAmount { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
}

public sealed record OrderItemResponseDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public MoneyDto UnitPrice { get; init; } = null!;
    public MoneyDto TotalPrice { get; init; } = null!;
    public DateTime AddedAt { get; init; }
    public DateTime? LastModifiedAt { get; init; }
}

public sealed record MoneyDto
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}