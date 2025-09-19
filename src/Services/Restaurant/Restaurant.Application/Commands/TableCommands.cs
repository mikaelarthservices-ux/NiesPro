using MediatR;
using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Commands;

/// <summary>
/// Commande pour créer une table
/// </summary>
public record CreateTableCommand(
    string Number,
    int SeatingCapacity,
    TableType TableType,
    RestaurantSection Section,
    decimal? XCoordinate = null,
    decimal? YCoordinate = null,
    string? Description = null,
    List<string>? Features = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour mettre à jour une table
/// </summary>
public record UpdateTableCommand(
    Guid TableId,
    string? Number = null,
    int? SeatingCapacity = null,
    TableType? TableType = null,
    RestaurantSection? Section = null,
    decimal? XCoordinate = null,
    decimal? YCoordinate = null,
    string? Description = null,
    List<string>? Features = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour changer le statut d'une table
/// </summary>
public record ChangeTableStatusCommand(
    Guid TableId,
    TableStatus Status,
    string? Reason = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour nettoyer une table
/// </summary>
public record CleanTableCommand(
    Guid TableId,
    Guid StaffId,
    CleaningType CleaningType = CleaningType.Standard,
    string? Notes = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour réserver une table
/// </summary>
public record ReserveTableCommand(
    Guid TableId,
    DateTime StartTime,
    DateTime EndTime,
    int PartySize,
    string? SpecialRequests = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour libérer une table
/// </summary>
public record ReleaseTableCommand(
    Guid TableId,
    bool RequiresCleaning = true,
    string? Notes = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour occuper une table
/// </summary>
public record OccupyTableCommand(
    Guid TableId,
    int PartySize,
    Guid? ReservationId = null,
    string? Notes = null) : IRequest<TableDto>;

/// <summary>
/// Commande pour supprimer une table
/// </summary>
public record DeleteTableCommand(Guid TableId) : IRequest<bool>;

/// <summary>
/// Commande pour optimiser l'attribution des tables
/// </summary>
public record OptimizeTableAssignmentCommand(
    int PartySize,
    DateTime PreferredTime,
    TimeSpan Duration,
    List<string>? SpecialRequirements = null) : IRequest<List<TableAvailabilityDto>>;

/// <summary>
/// Commande pour réorganiser les tables
/// </summary>
public record RearrangeTablesCommand(
    List<TablePositionDto> NewPositions) : IRequest<FloorPlanDto>;

/// <summary>
/// DTO pour position de table
/// </summary>
public record TablePositionDto(
    Guid TableId,
    decimal XCoordinate,
    decimal YCoordinate);