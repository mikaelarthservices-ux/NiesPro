using NiesPro.Contracts.Primitives;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    CustomerInfo CustomerInfo,
    Address ShippingAddress,
    Address? BillingAddress,
    Money TotalAmount,
    DateTime CreatedAt
) : IDomainEvent;