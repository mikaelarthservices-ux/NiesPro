using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

public sealed record PaymentProcessedEvent(
    Guid OrderId,
    Guid PaymentId,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    Money Amount,
    string? TransactionId,
    DateTime ProcessedAt
) : IDomainEvent;

public sealed record RefundProcessedEvent(
    Guid OrderId,
    Guid PaymentId,
    Money RefundAmount,
    string Reason,
    DateTime RefundedAt
) : IDomainEvent;