using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Events;

/// <summary>
/// Événements Enterprise pour Order Service multi-contexte
/// Architecture event-driven alignée sur NiesPro ERP standards
/// </summary>

/// <summary>
/// Événement déclenché lors de la mise à jour des informations de service
/// </summary>
public sealed record OrderServiceInfoUpdatedEvent(
    Guid OrderId,
    ServiceInfo ServiceInfo,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'assignation d'un serveur
/// </summary>
public sealed record WaiterAssignedEvent(
    Guid OrderId,
    Guid WaiterId,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la mise à jour des notes de service
/// </summary>
public sealed record ServiceNotesUpdatedEvent(
    Guid OrderId,
    string Notes,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de la mise à jour de la durée estimée
/// </summary>
public sealed record EstimatedDurationUpdatedEvent(
    Guid OrderId,
    TimeSpan EstimatedDuration,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand une commande est envoyée en cuisine
/// </summary>
public sealed record OrderSentToKitchenEvent(
    Guid OrderId,
    int? TableNumber,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand une commande est prête à servir
/// </summary>
public sealed record OrderReadyForServiceEvent(
    Guid OrderId,
    int? TableNumber,
    Guid? WaiterId,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand les articles sont scannés en boutique
/// </summary>
public sealed record OrderItemsScannedEvent(
    Guid OrderId,
    Guid? TerminalId,
    int TotalItems,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché quand une commande e-commerce est expédiée
/// </summary>
public sealed record OrderShippedEvent(
    Guid OrderId,
    string? DeliveryAddress,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors du changement de contexte métier
/// </summary>
public sealed record BusinessContextChangedEvent(
    Guid OrderId,
    BusinessContext PreviousContext,
    BusinessContext NewContext,
    string? Reason,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une transition de workflow
/// </summary>
public sealed record WorkflowTransitionEvent(
    Guid OrderId,
    BusinessContext Context,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    string? Reason,
    Guid? PerformedBy,
    TimeSpan TransitionDuration,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une erreur de workflow
/// </summary>
public sealed record WorkflowErrorEvent(
    Guid OrderId,
    BusinessContext Context,
    OrderStatus AttemptedStatus,
    string ErrorMessage,
    string? StackTrace,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'une intégration externe
/// </summary>
public sealed record ExternalIntegrationEvent(
    Guid OrderId,
    string IntegrationType, // "Kitchen", "POS", "Payment", "Shipping"
    string Action,
    bool Success,
    string? Response,
    TimeSpan Duration,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de métriques business
/// </summary>
public sealed record BusinessMetricEvent(
    Guid OrderId,
    BusinessContext Context,
    string MetricType, // "Revenue", "Latency", "Volume"
    decimal Value,
    string? Unit,
    IDictionary<string, object>? Metadata,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors d'alertes opérationnelles
/// </summary>
public sealed record OperationalAlertEvent(
    Guid OrderId,
    BusinessContext Context,
    AlertSeverity Severity,
    string AlertType,
    string Message,
    IDictionary<string, object>? Context_Data,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Événement déclenché lors de l'audit trail
/// </summary>
public sealed record AuditTrailEvent(
    Guid OrderId,
    string Action,
    Guid? PerformedBy,
    string? PerformedByName,
    IDictionary<string, object>? BeforeState,
    IDictionary<string, object>? AfterState,
    string? Notes,
    DateTime OccurredAt
) : IDomainEvent;

/// <summary>
/// Niveaux de sévérité des alertes
/// </summary>
public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}