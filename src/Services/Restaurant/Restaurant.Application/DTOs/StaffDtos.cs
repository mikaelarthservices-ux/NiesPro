using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

/// <summary>
/// DTO pour créer un membre du personnel
/// </summary>
public record CreateStaffDto(
    string EmployeeId,
    string FirstName,
    string LastName,
    string Email,
    StaffRole Role,
    EmploymentType EmploymentType,
    DateTime HireDate,
    decimal HourlyRate,
    CreateWorkScheduleDto WorkSchedule,
    string? PhoneNumber = null,
    AddressDto? Address = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null);

/// <summary>
/// DTO pour mettre à jour un membre du personnel
/// </summary>
public record UpdateStaffDto(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? PhoneNumber = null,
    AddressDto? Address = null,
    DateTime? DateOfBirth = null,
    Gender? Gender = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? Notes = null,
    string? ProfileImageUrl = null);

/// <summary>
/// DTO de réponse pour un membre du personnel
/// </summary>
public record StaffDto(
    Guid Id,
    string EmployeeId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    AddressDto? Address,
    StaffRole Role,
    StaffStatus Status,
    EmploymentType EmploymentType,
    Department Department,
    DateTime HireDate,
    DateTime? TerminationDate,
    decimal HourlyRate,
    decimal? MonthlyBonus,
    int VacationDaysPerYear,
    int UsedVacationDays,
    int RemainingVacationDays,
    int SickDaysUsed,
    WorkScheduleDto WorkSchedule,
    bool IsAvailableToday,
    DateTime? LastClockIn,
    DateTime? LastClockOut,
    WorkingStatus CurrentWorkingStatus,
    List<StaffSkillDto> Skills,
    List<string> Certifications,
    List<string> Languages,
    SkillLevel OverallSkillLevel,
    decimal? PerformanceRating,
    DateTime? LastPerformanceReview,
    int CompletedOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    decimal CustomerSatisfactionScore,
    DateTime? DateOfBirth,
    Gender? Gender,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? Notes,
    string? ProfileImageUrl,
    int WarningCount,
    int CommendationCount,
    DateTime? LastWarningDate,
    DateTime? LastCommendationDate,
    List<SystemPermission> Permissions,
    string? BadgeNumber,
    DateTime? LastLoginAt,
    bool RequiresPasswordReset,
    bool IsSupervisor,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO simplifié pour membre du personnel
/// </summary>
public record StaffSummaryDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    StaffRole Role,
    Department Department,
    StaffStatus Status,
    WorkingStatus CurrentWorkingStatus,
    bool IsAvailableToday,
    string? ProfileImageUrl);

/// <summary>
/// DTO pour adresse
/// </summary>
public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country,
    string? State = null,
    string? AdditionalInfo = null);

/// <summary>
/// DTO pour horaire de travail
/// </summary>
public record WorkScheduleDto(
    List<WorkDayDto> WorkDays,
    int TotalHoursPerWeek,
    bool IsFlexible);

/// <summary>
/// DTO pour créer un horaire de travail
/// </summary>
public record CreateWorkScheduleDto(
    List<CreateWorkDayDto> WorkDays,
    bool IsFlexible = false);

/// <summary>
/// DTO pour jour de travail
/// </summary>
public record WorkDayDto(
    DayOfWeek DayOfWeek,
    bool IsWorkingDay,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    TimeOnly? BreakStartTime,
    TimeOnly? BreakEndTime);

/// <summary>
/// DTO pour créer un jour de travail
/// </summary>
public record CreateWorkDayDto(
    DayOfWeek DayOfWeek,
    bool IsWorkingDay,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    TimeOnly? BreakStartTime = null,
    TimeOnly? BreakEndTime = null);

/// <summary>
/// DTO pour compétence du personnel
/// </summary>
public record StaffSkillDto(
    string Name,
    SkillLevel Level,
    DateTime? CertifiedDate,
    bool IsCertified);

/// <summary>
/// DTO pour changer le statut
/// </summary>
public record ChangeStaffStatusDto(
    StaffStatus Status,
    string? Reason = null,
    DateTime? EffectiveDate = null);

/// <summary>
/// DTO pour changer de rôle
/// </summary>
public record ChangeStaffRoleDto(
    StaffRole Role,
    Department? Department = null,
    decimal? HourlyRate = null,
    string? Reason = null);

/// <summary>
/// DTO pour pointer
/// </summary>
public record ClockInDto(
    DateTime? ClockInTime = null);

/// <summary>
/// DTO pour dépointer
/// </summary>
public record ClockOutDto(
    DateTime? ClockOutTime = null);

/// <summary>
/// DTO pour prendre une pause
/// </summary>
public record StartBreakDto(
    BreakType BreakType);

/// <summary>
/// DTO pour ajouter une compétence
/// </summary>
public record AddStaffSkillDto(
    string SkillName,
    SkillLevel Level,
    DateTime? CertifiedDate = null);

/// <summary>
/// DTO pour mettre à jour une compétence
/// </summary>
public record UpdateStaffSkillDto(
    string SkillName,
    SkillLevel NewLevel);

/// <summary>
/// DTO pour demander des congés
/// </summary>
public record RequestVacationDto(
    DateTime StartDate,
    DateTime EndDate,
    VacationType VacationType,
    string? Reason = null);

/// <summary>
/// DTO pour traiter une demande de congé
/// </summary>
public record ProcessVacationRequestDto(
    bool Approved,
    string? Comments = null);

/// <summary>
/// DTO pour demande de congé
/// </summary>
public record StaffVacationRequestDto(
    Guid Id,
    DateTime StartDate,
    DateTime EndDate,
    VacationType VacationType,
    VacationRequestStatus Status,
    string? Reason,
    Guid? ApprovedBy,
    DateTime? ProcessedAt,
    string? Comments,
    int DaysRequested,
    bool IsApproved);

/// <summary>
/// DTO pour ajouter une formation
/// </summary>
public record AddStaffTrainingDto(
    string TrainingName,
    DateTime StartDate,
    DateTime? EndDate = null,
    TrainingStatus Status = TrainingStatus.Scheduled);

/// <summary>
/// DTO pour compléter une formation
/// </summary>
public record CompleteTrainingDto(
    DateTime CompletionDate,
    decimal? Score = null);

/// <summary>
/// DTO pour formation
/// </summary>
public record StaffTrainingDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime? EndDate,
    TrainingStatus Status,
    DateTime? CompletionDate,
    decimal? Score);

/// <summary>
/// DTO pour ajouter un avertissement
/// </summary>
public record AddWarningDto(
    string Reason,
    WarningType WarningType = WarningType.Verbal);

/// <summary>
/// DTO pour ajouter une félicitation
/// </summary>
public record AddCommendationDto(
    string Reason,
    decimal? BonusAmount = null);

/// <summary>
/// DTO pour évaluation de performance
/// </summary>
public record UpdatePerformanceRatingDto(
    decimal Rating,
    string? Comments = null);

/// <summary>
/// DTO pour évaluation
/// </summary>
public record StaffEvaluationDto(
    Guid Id,
    decimal Rating,
    Guid EvaluatedBy,
    string? EvaluatedByName,
    DateTime EvaluationDate,
    string? Comments,
    List<string> Strengths,
    List<string> AreasForImprovement);

/// <summary>
/// DTO pour équipe
/// </summary>
public record StaffShiftDto(
    Guid Id,
    DateTime Date,
    DateTime StartTime,
    DateTime EndTime,
    ShiftType ShiftType,
    ShiftStatus Status,
    Department Department,
    string? Notes,
    TimeSpan Duration,
    bool IsToday);

/// <summary>
/// DTO pour créer une équipe
/// </summary>
public record CreateStaffShiftDto(
    DateTime Date,
    DateTime StartTime,
    DateTime EndTime,
    ShiftType ShiftType,
    Department Department);

/// <summary>
/// DTO pour entrée de temps
/// </summary>
public record StaffTimeEntryDto(
    Guid Id,
    DateTime Timestamp,
    TimeEntryType Type,
    string? Notes);

/// <summary>
/// DTO pour statistiques du personnel
/// </summary>
public record StaffStatisticsDto(
    int TotalStaff,
    int ActiveStaff,
    int OnDutyStaff,
    int OnBreakStaff,
    Dictionary<Department, int> StaffByDepartment,
    Dictionary<StaffRole, int> StaffByRole,
    Dictionary<EmploymentType, int> StaffByEmploymentType,
    decimal AverageHourlyRate,
    decimal TotalMonthlySalary,
    int TotalVacationDaysUsed,
    int TotalSickDaysUsed,
    decimal AveragePerformanceRating,
    int TotalWarnings,
    int TotalCommendations,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour performance du personnel
/// </summary>
public record StaffPerformanceDto(
    Guid StaffId,
    string FullName,
    StaffRole Role,
    decimal HoursWorked,
    int OrdersCompleted,
    decimal Revenue,
    decimal? PerformanceRating,
    decimal CustomerSatisfactionScore,
    int WarningCount,
    int CommendationCount,
    DateTime PeriodStart,
    DateTime PeriodEnd);

/// <summary>
/// DTO pour recherche du personnel
/// </summary>
public record StaffSearchDto(
    string? Name = null,
    StaffRole? Role = null,
    Department? Department = null,
    StaffStatus? Status = null,
    bool? IsAvailable = null,
    SkillLevel? MinSkillLevel = null,
    string? Skill = null,
    int PageSize = 20,
    int PageNumber = 1);

/// <summary>
/// DTO pour résultat de recherche du personnel
/// </summary>
public record PagedStaffResultDto(
    List<StaffSummaryDto> Staff,
    int TotalCount,
    int PageSize,
    int PageNumber,
    int TotalPages,
    StaffStatisticsDto Statistics);

/// <summary>
/// DTO pour planning du personnel
/// </summary>
public record StaffScheduleDto(
    DateTime Date,
    List<StaffShiftDto> Shifts,
    List<StaffSummaryDto> AvailableStaff,
    List<StaffSummaryDto> OnDutyStaff,
    List<StaffVacationRequestDto> VacationRequests,
    Dictionary<Department, List<StaffShiftDto>> ShiftsByDepartment);