using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;

namespace Order.Domain.Events;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus,
    string? Reason,
    DateTime ChangedAt
) : IDomainEvent;