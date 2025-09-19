using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

/// <summary>
/// DTO pour la création d'une table
/// </summary>
public record CreateTableDto(
    string Number,
    int SeatingCapacity,
    TableType TableType,
    RestaurantSection Section,
    decimal? XCoordinate = null,
    decimal? YCoordinate = null,
    string? Description = null,
    List<string>? Features = null);

/// <summary>
/// DTO pour la mise à jour d'une table
/// </summary>
public record UpdateTableDto(
    string? Number = null,
    int? SeatingCapacity = null,
    TableType? TableType = null,
    RestaurantSection? Section = null,
    decimal? XCoordinate = null,
    decimal? YCoordinate = null,
    string? Description = null,
    List<string>? Features = null);

/// <summary>
/// DTO de réponse pour une table
/// </summary>
public record TableDto(
    Guid Id,
    string Number,
    int SeatingCapacity,
    int MaxCapacity,
    TableType TableType,
    TableStatus Status,
    RestaurantSection Section,
    decimal? XCoordinate,
    decimal? YCoordinate,
    string? Description,
    List<string> Features,
    bool IsAvailable,
    bool RequiresCleaning,
    DateTime? LastCleanedAt,
    DateTime? CurrentReservationStart,
    DateTime? CurrentReservationEnd,
    Guid? CurrentReservationId,
    string? CurrentReservationCustomer,
    int TotalCovers,
    decimal Revenue,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO simplifié pour liste de tables
/// </summary>
public record TableSummaryDto(
    Guid Id,
    string Number,
    int SeatingCapacity,
    TableType TableType,
    TableStatus Status,
    RestaurantSection Section,
    bool IsAvailable,
    bool RequiresCleaning,
    Guid? CurrentReservationId);

/// <summary>
/// DTO pour changer le statut d'une table
/// </summary>
public record ChangeTableStatusDto(
    TableStatus Status,
    string? Reason = null);

/// <summary>
/// DTO pour nettoyer une table
/// </summary>
public record CleanTableDto(
    Guid StaffId,
    CleaningType CleaningType = CleaningType.Standard,
    string? Notes = null);

/// <summary>
/// DTO pour réserver une table
/// </summary>
public record ReserveTableDto(
    DateTime StartTime,
    DateTime EndTime,
    int PartySize,
    string? SpecialRequests = null);

/// <summary>
/// DTO pour statistiques de table
/// </summary>
public record TableStatisticsDto(
    Guid TableId,
    string TableNumber,
    int TotalReservations,
    int CompletedReservations,
    int CancelledReservations,
    int NoShowReservations,
    decimal OccupancyRate,
    decimal AveragePartySize,
    decimal TotalRevenue,
    decimal AverageRevenuePerCover,
    TimeSpan AverageReservationDuration,
    int TotalCovers,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour le plan de salle
/// </summary>
public record FloorPlanDto(
    List<TableDto> Tables,
    List<RestaurantSectionDto> Sections,
    decimal Width,
    decimal Height,
    DateTime LastUpdated);

/// <summary>
/// DTO pour section de restaurant
/// </summary>
public record RestaurantSectionDto(
    RestaurantSection Section,
    string Name,
    int TableCount,
    int AvailableTables,
    int OccupiedTables,
    int ReservedTables,
    int TablesNeedingCleaning,
    decimal OccupancyRate);