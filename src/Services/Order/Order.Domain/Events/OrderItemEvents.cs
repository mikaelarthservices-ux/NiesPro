using NiesPro.Contracts.Primitives;

namespace Order.Domain.Events;

public sealed record OrderItemAddedEvent(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    DateTime AddedAt
) : IDomainEvent;

public sealed record OrderItemRemovedEvent(
    Guid OrderId,
    Guid ProductId,
    int RemovedQuantity,
    DateTime RemovedAt
) : IDomainEvent;

public sealed record OrderItemQuantityChangedEvent(
    Guid OrderId,
    Guid ProductId,
    int PreviousQuantity,
    int NewQuantity,
    DateTime ChangedAt
) : IDomainEvent;