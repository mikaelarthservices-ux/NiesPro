using AutoMapper;
using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Domain.ValueObjects;

namespace Order.Application.Mappings;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Domain to DTO mappings
        CreateMap<Domain.Entities.Order, OrderDto>()
            .ForMember(dest => dest.CustomerInfo, opt => opt.MapFrom(src => src.CustomerInfo))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
            .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.BillingAddress))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal))
            .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));

        CreateMap<OrderItem, OrderItemResponseDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.RefundedAmount, opt => opt.MapFrom(src => src.RefundedAmount));

        // Value Objects to DTO mappings
        CreateMap<CustomerInfo, CustomerInfoDto>();
        CreateMap<Address, AddressDto>();
        CreateMap<Money, MoneyDto>();

        // DTO to Value Objects mappings (for commands)
        CreateMap<CustomerInfoDto, CustomerInfo>()
            .ConstructUsing(src => CustomerInfo.Create(
                src.FirstName,
                src.LastName,
                src.Email,
                src.PhoneNumber));

        CreateMap<AddressDto, Address>()
            .ConstructUsing(src => Address.Create(
                src.Street,
                src.City,
                src.PostalCode,
                src.Country,
                src.AddressLine2));

        CreateMap<MoneyDto, Money>()
            .ConstructUsing(src => Money.Create(src.Amount, src.Currency));
    }
}

public sealed record PaymentDto
{
    public Guid Id { get; init; }
    public string Method { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public MoneyDto Amount { get; init; } = null!;
    public MoneyDto RefundedAmount { get; init; } = null!;
    public string? TransactionId { get; init; }
    public string? ProviderReference { get; init; }
    public string? FailureReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? FailedAt { get; init; }
}