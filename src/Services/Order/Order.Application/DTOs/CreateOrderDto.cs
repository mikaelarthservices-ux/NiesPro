namespace Order.Application.DTOs;

public sealed record CreateOrderDto
{
    public Guid CustomerId { get; init; }
    public CustomerInfoDto CustomerInfo { get; init; } = null!;
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto? BillingAddress { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TaxAmount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal DiscountAmount { get; init; }
    public string Currency { get; init; } = "EUR";
}

public sealed record CustomerInfoDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public sealed record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string? AddressLine2 { get; init; }
}

public sealed record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}