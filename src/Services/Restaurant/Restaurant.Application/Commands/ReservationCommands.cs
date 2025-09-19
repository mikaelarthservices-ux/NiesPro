using MediatR;
using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Commands;

/// <summary>
/// Commande pour créer une réservation
/// </summary>
public record CreateReservationCommand(
    Guid TableId,
    DateTime ReservationDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail = null,
    string? SpecialRequests = null,
    List<string>? DietaryRestrictions = null,
    ReservationType ReservationType = ReservationType.Regular,
    bool IsVip = false) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour mettre à jour une réservation
/// </summary>
public record UpdateReservationCommand(
    Guid ReservationId,
    DateTime? ReservationDateTime = null,
    int? PartySize = null,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? CustomerEmail = null,
    string? SpecialRequests = null,
    List<string>? DietaryRestrictions = null,
    Guid? TableId = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour confirmer une réservation
/// </summary>
public record ConfirmReservationCommand(
    Guid ReservationId,
    int? ActualPartySize = null,
    string? Notes = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour check-in d'une réservation
/// </summary>
public record CheckInReservationCommand(
    Guid ReservationId,
    int? ActualPartySize = null,
    string? Notes = null,
    List<string>? AdditionalRequests = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour asseoir une réservation
/// </summary>
public record SeatReservationCommand(
    Guid ReservationId,
    Guid? TableId = null,
    string? Notes = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour compléter une réservation
/// </summary>
public record CompleteReservationCommand(
    Guid ReservationId,
    decimal? TotalAmount = null,
    TimeSpan? ActualDuration = null,
    string? Notes = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour annuler une réservation
/// </summary>
public record CancelReservationCommand(
    Guid ReservationId,
    string Reason,
    bool RefundDeposit = false,
    bool SendNotification = true) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour reprogrammer une réservation
/// </summary>
public record RescheduleReservationCommand(
    Guid ReservationId,
    DateTime NewDateTime,
    Guid? NewTableId = null,
    string? Reason = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour marquer comme no-show
/// </summary>
public record MarkNoShowCommand(
    Guid ReservationId,
    string? Reason = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour demander un dépôt
/// </summary>
public record RequestDepositCommand(
    Guid ReservationId,
    decimal Amount,
    string Reason,
    DateTime DueDate) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour marquer le dépôt comme payé
/// </summary>
public record MarkDepositPaidCommand(
    Guid ReservationId,
    string PaymentMethod,
    string? TransactionId = null,
    string? Notes = null) : IRequest<ReservationDto>;

/// <summary>
/// Commande pour envoyer un rappel
/// </summary>
public record SendReminderCommand(
    Guid ReservationId,
    ReminderType ReminderType,
    string? CustomMessage = null) : IRequest<bool>;

/// <summary>
/// Commande pour envoyer des rappels en lot
/// </summary>
public record SendBatchRemindersCommand(
    DateTime Date,
    ReminderType ReminderType,
    List<Guid>? ReservationIds = null) : IRequest<int>;

/// <summary>
/// Commande pour créer une réservation récurrente
/// </summary>
public record CreateRecurringReservationCommand(
    Guid TableId,
    DateTime StartDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    RecurrencePattern RecurrencePattern,
    int Occurrences,
    string? CustomerEmail = null,
    string? SpecialRequests = null) : IRequest<List<ReservationDto>>;

/// <summary>
/// Commande pour optimiser les réservations
/// </summary>
public record OptimizeReservationsCommand(
    DateTime Date) : IRequest<List<ReservationOptimizationSuggestionDto>>;

/// <summary>
/// Commande pour créer une liste d'attente
/// </summary>
public record AddToWaitlistCommand(
    DateTime PreferredDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail = null,
    TimeSpan? MaxWaitTime = null) : IRequest<WaitlistEntryDto>;

/// <summary>
/// DTO pour suggestion d'optimisation de réservation
/// </summary>
public record ReservationOptimizationSuggestionDto(
    Guid ReservationId,
    string SuggestionType,
    string Description,
    Guid? SuggestedTableId,
    DateTime? SuggestedTime,
    decimal EstimatedImpact);

/// <summary>
/// DTO pour entrée de liste d'attente
/// </summary>
public record WaitlistEntryDto(
    Guid Id,
    DateTime PreferredDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    WaitlistStatus Status,
    DateTime CreatedAt,
    TimeSpan? MaxWaitTime,
    int Position);

/// <summary>
/// Modèle de récurrence
/// </summary>
public enum RecurrencePattern
{
    Daily,
    Weekly,
    Monthly,
    Custom
}