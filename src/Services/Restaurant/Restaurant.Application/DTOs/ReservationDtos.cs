using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

/// <summary>
/// DTO pour créer une réservation
/// </summary>
public record CreateReservationDto(
    Guid TableId,
    DateTime ReservationDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail = null,
    string? SpecialRequests = null,
    List<string>? DietaryRestrictions = null,
    ReservationType ReservationType = ReservationType.Regular,
    bool IsVip = false);

/// <summary>
/// DTO pour mettre à jour une réservation
/// </summary>
public record UpdateReservationDto(
    DateTime? ReservationDateTime = null,
    int? PartySize = null,
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? CustomerEmail = null,
    string? SpecialRequests = null,
    List<string>? DietaryRestrictions = null,
    Guid? TableId = null);

/// <summary>
/// DTO de réponse pour une réservation
/// </summary>
public record ReservationDto(
    Guid Id,
    string ReservationNumber,
    Guid TableId,
    string TableNumber,
    DateTime ReservationDateTime,
    DateTime EstimatedEndTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    ReservationStatus Status,
    ReservationType ReservationType,
    bool IsVip,
    string? SpecialRequests,
    List<string> DietaryRestrictions,
    DateTime? CheckInTime,
    DateTime? SeatedTime,
    DateTime? CompletedTime,
    DateTime? CancelledTime,
    string? CancellationReason,
    decimal? DepositAmount,
    bool DepositPaid,
    bool ReminderSent,
    bool ConfirmationSent,
    int? ActualPartySize,
    decimal? TotalAmount,
    TimeSpan? ActualDuration,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO simplifié pour liste de réservations
/// </summary>
public record ReservationSummaryDto(
    Guid Id,
    string ReservationNumber,
    string TableNumber,
    DateTime ReservationDateTime,
    int PartySize,
    string CustomerName,
    string CustomerPhone,
    ReservationStatus Status,
    bool IsVip,
    bool IsToday);

/// <summary>
/// DTO pour confirmer une réservation
/// </summary>
public record ConfirmReservationDto(
    int? ActualPartySize = null,
    string? Notes = null);

/// <summary>
/// DTO pour check-in d'une réservation
/// </summary>
public record CheckInReservationDto(
    int? ActualPartySize = null,
    string? Notes = null,
    List<string>? AdditionalRequests = null);

/// <summary>
/// DTO pour annuler une réservation
/// </summary>
public record CancelReservationDto(
    string Reason,
    bool RefundDeposit = false,
    bool SendNotification = true);

/// <summary>
/// DTO pour modifier l'heure d'une réservation
/// </summary>
public record RescheduleReservationDto(
    DateTime NewDateTime,
    Guid? NewTableId = null,
    string? Reason = null);

/// <summary>
/// DTO pour demander un dépôt
/// </summary>
public record RequestDepositDto(
    decimal Amount,
    string Reason,
    DateTime DueDate);

/// <summary>
/// DTO pour marquer le dépôt comme payé
/// </summary>
public record MarkDepositPaidDto(
    string PaymentMethod,
    string? TransactionId = null,
    string? Notes = null);

/// <summary>
/// DTO pour envoyer des rappels
/// </summary>
public record SendReminderDto(
    ReminderType ReminderType,
    string? CustomMessage = null);

/// <summary>
/// DTO pour statistiques de réservations
/// </summary>
public record ReservationStatisticsDto(
    int TotalReservations,
    int ConfirmedReservations,
    int CompletedReservations,
    int CancelledReservations,
    int NoShowReservations,
    decimal CompletionRate,
    decimal CancellationRate,
    decimal NoShowRate,
    decimal AveragePartySize,
    decimal TotalRevenue,
    decimal AverageRevenuePerReservation,
    TimeSpan AverageReservationDuration,
    int VipReservations,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour disponibilité de table
/// </summary>
public record TableAvailabilityDto(
    Guid TableId,
    string TableNumber,
    int SeatingCapacity,
    TableType TableType,
    RestaurantSection Section,
    bool IsAvailable,
    DateTime? NextAvailableTime,
    List<TimeSlotDto> AvailableSlots);

/// <summary>
/// DTO pour créneau horaire
/// </summary>
public record TimeSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    int AvailableCapacity,
    bool IsRecommended);

/// <summary>
/// DTO pour recherche de réservations
/// </summary>
public record ReservationSearchDto(
    string? CustomerName = null,
    string? CustomerPhone = null,
    string? ReservationNumber = null,
    DateTime? Date = null,
    ReservationStatus? Status = null,
    bool? IsVip = null,
    Guid? TableId = null,
    int PageSize = 20,
    int PageNumber = 1);

/// <summary>
/// DTO pour résultat de recherche paginé
/// </summary>
public record PagedReservationResultDto(
    List<ReservationSummaryDto> Reservations,
    int TotalCount,
    int PageSize,
    int PageNumber,
    int TotalPages);

/// <summary>
/// DTO pour planning journalier
/// </summary>
public record DailyScheduleDto(
    DateTime Date,
    List<ReservationDto> Reservations,
    List<TableAvailabilityDto> TableAvailabilities,
    ReservationStatisticsDto Statistics,
    List<string> Alerts);

/// <summary>
/// DTO pour alerte de réservation
/// </summary>
public record ReservationAlertDto(
    Guid ReservationId,
    string ReservationNumber,
    string CustomerName,
    AlertType AlertType,
    string Message,
    AlertPriority Priority,
    DateTime CreatedAt);