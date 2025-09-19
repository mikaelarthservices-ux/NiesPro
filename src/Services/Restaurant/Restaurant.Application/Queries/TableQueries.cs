using MediatR;
using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Queries;

/// <summary>
/// Requête pour obtenir une table par ID
/// </summary>
public record GetTableByIdQuery(Guid TableId) : IRequest<TableDto?>;

/// <summary>
/// Requête pour obtenir une table par numéro
/// </summary>
public record GetTableByNumberQuery(string TableNumber) : IRequest<TableDto?>;

/// <summary>
/// Requête pour obtenir toutes les tables
/// </summary>
public record GetAllTablesQuery(
    TableStatus? Status = null,
    RestaurantSection? Section = null,
    bool? IsAvailable = null,
    bool? RequiresCleaning = null) : IRequest<List<TableDto>>;

/// <summary>
/// Requête pour obtenir les tables disponibles
/// </summary>
public record GetAvailableTablesQuery(
    DateTime? ForDateTime = null,
    TimeSpan? Duration = null,
    int? MinCapacity = null,
    int? MaxCapacity = null,
    RestaurantSection? Section = null) : IRequest<List<TableAvailabilityDto>>;

/// <summary>
/// Requête pour obtenir les tables par section
/// </summary>
public record GetTablesBySectionQuery(
    RestaurantSection Section,
    TableStatus? Status = null) : IRequest<List<TableDto>>;

/// <summary>
/// Requête pour obtenir les tables nécessitant un nettoyage
/// </summary>
public record GetTablesNeedingCleaningQuery(
    RestaurantSection? Section = null) : IRequest<List<TableDto>>;

/// <summary>
/// Requête pour obtenir les tables occupées
/// </summary>
public record GetOccupiedTablesQuery(
    RestaurantSection? Section = null) : IRequest<List<TableDto>>;

/// <summary>
/// Requête pour obtenir les tables réservées
/// </summary>
public record GetReservedTablesQuery(
    DateTime? ForDate = null,
    RestaurantSection? Section = null) : IRequest<List<TableDto>>;

/// <summary>
/// Requête pour obtenir le plan de salle
/// </summary>
public record GetFloorPlanQuery(
    RestaurantSection? Section = null) : IRequest<FloorPlanDto>;

/// <summary>
/// Requête pour obtenir les statistiques des tables
/// </summary>
public record GetTableStatisticsQuery(
    Guid? TableId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    RestaurantSection? Section = null) : IRequest<List<TableStatisticsDto>>;

/// <summary>
/// Requête pour obtenir l'historique d'une table
/// </summary>
public record GetTableHistoryQuery(
    Guid TableId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageSize = 20,
    int PageNumber = 1) : IRequest<PagedTableHistoryResultDto>;

/// <summary>
/// Requête pour obtenir le taux d'occupation
/// </summary>
public record GetOccupancyRateQuery(
    DateTime StartDate,
    DateTime EndDate,
    RestaurantSection? Section = null,
    TimeSpan? TimeInterval = null) : IRequest<OccupancyRateDto>;

/// <summary>
/// Requête pour obtenir les recommandations de tables
/// </summary>
public record GetTableRecommendationsQuery(
    int PartySize,
    DateTime PreferredTime,
    TimeSpan EstimatedDuration,
    List<string>? SpecialRequirements = null) : IRequest<List<TableRecommendationDto>>;

/// <summary>
/// Requête pour obtenir la disponibilité en temps réel
/// </summary>
public record GetRealTimeAvailabilityQuery() : IRequest<RealTimeAvailabilityDto>;

/// <summary>
/// Requête pour obtenir les alertes de table
/// </summary>
public record GetTableAlertsQuery(
    AlertPriority? Priority = null,
    bool? IsResolved = null) : IRequest<List<TableAlertDto>>;

/// <summary>
/// Requête pour obtenir l'analyse de performance des tables
/// </summary>
public record GetTablePerformanceAnalysisQuery(
    DateTime StartDate,
    DateTime EndDate,
    RestaurantSection? Section = null) : IRequest<TablePerformanceAnalysisDto>;

/// <summary>
/// DTO pour historique de table
/// </summary>
public record TableHistoryEntryDto(
    DateTime Timestamp,
    string Action,
    string Details,
    Guid? PerformedBy,
    string? PerformedByName);

/// <summary>
/// DTO pour résultat paginé d'historique de table
/// </summary>
public record PagedTableHistoryResultDto(
    List<TableHistoryEntryDto> Entries,
    int TotalCount,
    int PageSize,
    int PageNumber,
    int TotalPages);

/// <summary>
/// DTO pour taux d'occupation
/// </summary>
public record OccupancyRateDto(
    decimal OverallOccupancyRate,
    Dictionary<RestaurantSection, decimal> OccupancyBySection,
    Dictionary<DateTime, decimal> OccupancyByTimeSlot,
    Dictionary<DayOfWeek, decimal> OccupancyByDayOfWeek,
    int TotalSeats,
    int OccupiedSeats,
    int AvailableSeats,
    DateTime StartDate,
    DateTime EndDate);

/// <summary>
/// DTO pour recommandation de table
/// </summary>
public record TableRecommendationDto(
    Guid TableId,
    string TableNumber,
    int SeatingCapacity,
    TableType TableType,
    RestaurantSection Section,
    decimal RecommendationScore,
    string Reason,
    DateTime AvailableFrom,
    DateTime AvailableUntil,
    bool IsOptimal);

/// <summary>
/// DTO pour disponibilité en temps réel
/// </summary>
public record RealTimeAvailabilityDto(
    int TotalTables,
    int AvailableTables,
    int OccupiedTables,
    int ReservedTables,
    int TablesNeedingCleaning,
    Dictionary<RestaurantSection, SectionAvailabilityDto> SectionAvailability,
    List<TableSummaryDto> NextAvailableTables,
    DateTime LastUpdated);

/// <summary>
/// DTO pour disponibilité de section
/// </summary>
public record SectionAvailabilityDto(
    RestaurantSection Section,
    int TotalTables,
    int AvailableTables,
    int OccupiedTables,
    int ReservedTables,
    decimal OccupancyRate);

/// <summary>
/// DTO pour alerte de table
/// </summary>
public record TableAlertDto(
    Guid Id,
    Guid TableId,
    string TableNumber,
    AlertType AlertType,
    string Message,
    AlertPriority Priority,
    bool IsResolved,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

/// <summary>
/// DTO pour analyse de performance des tables
/// </summary>
public record TablePerformanceAnalysisDto(
    decimal AverageOccupancyRate,
    TimeSpan AverageTurnoverTime,
    decimal AverageRevenuePerTable,
    decimal AverageRevenuePerSeat,
    List<TablePerformanceDto> TablePerformances,
    Dictionary<TimeSpan, int> TurnoverTimeDistribution,
    Dictionary<RestaurantSection, TableSectionPerformanceDto> SectionPerformances,
    List<string> OptimizationRecommendations,
    DateTime StartDate,
    DateTime EndDate);

/// <summary>
/// DTO pour performance de table
/// </summary>
public record TablePerformanceDto(
    Guid TableId,
    string TableNumber,
    int TotalReservations,
    decimal OccupancyRate,
    TimeSpan AverageTurnoverTime,
    decimal TotalRevenue,
    decimal RevenuePerSeat,
    decimal EfficiencyScore);

/// <summary>
/// DTO pour performance de section
/// </summary>
public record TableSectionPerformanceDto(
    RestaurantSection Section,
    int TableCount,
    decimal AverageOccupancyRate,
    decimal TotalRevenue,
    decimal RevenuePerTable,
    decimal EfficiencyScore);