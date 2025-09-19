using Customer.Domain.Enums;

namespace Customer.Application.DTOs;

/// <summary>
/// DTO pour la création d'un segment client
/// </summary>
public class CreateCustomerSegmentDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SegmentType Type { get; init; } = SegmentType.Behavioral;
    public bool IsActive { get; init; } = true;
    public bool IsAutomatic { get; init; } = true;
    public decimal MinMatchScore { get; init; } = 0.7m;
    
    // Critères de segmentation
    public List<CreateSegmentCriterionDto> Criteria { get; init; } = new();
    
    // Configuration de rafraîchissement
    public int RefreshIntervalHours { get; init; } = 24;
    public bool AutoRefresh { get; init; } = true;
    
    // Métadonnées
    public string? BusinessPurpose { get; init; }
    public List<string> Tags { get; init; } = new();
}

/// <summary>
/// DTO pour un segment client complet
/// </summary>
public class CustomerSegmentDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public SegmentType Type { get; init; }
    public bool IsActive { get; init; }
    public bool IsAutomatic { get; init; }
    public decimal MinMatchScore { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? LastRefreshDate { get; init; }
    public int RefreshIntervalHours { get; init; }
    public bool AutoRefresh { get; init; }
    
    // Statistiques
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public decimal AverageMatchScore { get; init; }
    public DateTime? LastCustomerAdded { get; init; }
    
    // Critères
    public List<SegmentCriterionDto> Criteria { get; init; } = new();
    
    // Métadonnées
    public string? BusinessPurpose { get; init; }
    public List<string> Tags { get; init; } = new();
    
    // Performance
    public SegmentPerformanceDto? Performance { get; init; }
}

/// <summary>
/// DTO pour un critère de segment
/// </summary>
public class SegmentCriterionDto
{
    public required string Field { get; init; }
    public required string Operator { get; init; }
    public required string Value { get; init; }
    public decimal Weight { get; init; }
    public bool IsRequired { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// DTO pour créer un critère
/// </summary>
public class CreateSegmentCriterionDto
{
    public required string Field { get; init; }
    public required string Operator { get; init; }
    public required string Value { get; init; }
    public decimal Weight { get; init; } = 1.0m;
    public bool IsRequired { get; init; } = false;
    public string? Description { get; init; }
}

/// <summary>
/// DTO pour les performances d'un segment
/// </summary>
public class SegmentPerformanceDto
{
    public decimal ConversionRate { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal CustomerLifetimeValue { get; init; }
    public int AverageVisitsPerMonth { get; init; }
    public decimal ChurnRate { get; init; }
    public decimal EngagementScore { get; init; }
    public DateTime AnalysisDate { get; init; }
}

/// <summary>
/// DTO pour l'évaluation d'un client pour les segments
/// </summary>
public class CustomerSegmentEvaluationDto
{
    public Guid CustomerId { get; init; }
    public List<SegmentMatchDto> MatchingSegments { get; init; } = new();
    public List<SegmentMatchDto> PotentialSegments { get; init; } = new();
    public DateTime EvaluationDate { get; init; }
}

/// <summary>
/// DTO pour une correspondance de segment
/// </summary>
public class SegmentMatchDto
{
    public Guid SegmentId { get; init; }
    public required string SegmentName { get; init; }
    public decimal MatchScore { get; init; }
    public List<string> MatchingCriteria { get; init; } = new();
    public List<string> MissingCriteria { get; init; } = new();
    public bool IsCurrentlyAssigned { get; init; }
    public DateTime? AssignedDate { get; init; }
}

/// <summary>
/// DTO pour assigner un segment manuellement
/// </summary>
public class AssignCustomerSegmentDto
{
    public Guid CustomerId { get; init; }
    public Guid SegmentId { get; init; }
    public string? Reason { get; init; }
    public bool OverrideAutomatic { get; init; } = false;
}

/// <summary>
/// DTO pour une interaction client
/// </summary>
public class CustomerInteractionDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public InteractionType Type { get; init; }
    public InteractionChannel Channel { get; init; }
    public required string Subject { get; init; }
    public string? Description { get; init; }
    public DateTime InteractionDate { get; init; }
    public string? Staff { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool RequiresFollowUp { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? Notes { get; init; }
    
    // Informations calculées
    public bool IsOverdue => RequiresFollowUp && FollowUpDate.HasValue && DateTime.UtcNow > FollowUpDate.Value;
    public bool IsCompleted => CompletedDate.HasValue;
    public int DaysSinceInteraction => (DateTime.UtcNow - InteractionDate).Days;
}

/// <summary>
/// DTO pour créer une interaction
/// </summary>
public class CreateCustomerInteractionDto
{
    public Guid CustomerId { get; init; }
    public InteractionType Type { get; init; }
    public InteractionChannel Channel { get; init; }
    public required string Subject { get; init; }
    public string? Description { get; init; }
    public string? Staff { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool RequiresFollowUp { get; init; } = false;
    public DateTime? FollowUpDate { get; init; }
    public List<string> Tags { get; init; } = new();
    public string? Notes { get; init; }
}

/// <summary>
/// DTO pour mettre à jour une interaction
/// </summary>
public class UpdateCustomerInteractionDto
{
    public string? Description { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool? RequiresFollowUp { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public List<string>? Tags { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO pour compléter une interaction
/// </summary>
public class CompleteInteractionDto
{
    public InteractionOutcome Outcome { get; init; }
    public string? Resolution { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool ScheduleFollowUp { get; init; } = false;
    public DateTime? FollowUpDate { get; init; }
    public string? FollowUpReason { get; init; }
}

/// <summary>
/// DTO pour rechercher les interactions
/// </summary>
public class InteractionSearchDto
{
    public Guid? CustomerId { get; init; }
    public InteractionType? Type { get; init; }
    public InteractionChannel? Channel { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Staff { get; init; }
    public int? MinSatisfactionRating { get; init; }
    public int? MaxSatisfactionRating { get; init; }
    public bool? RequiresFollowUp { get; init; }
    public bool? IsOverdue { get; init; }
    public bool? IsCompleted { get; init; }
    public List<string>? Tags { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// DTO pour les statistiques d'interactions
/// </summary>
public class InteractionStatsDto
{
    public int TotalInteractions { get; init; }
    public int InteractionsThisMonth { get; init; }
    public int PendingFollowUps { get; init; }
    public int OverdueFollowUps { get; init; }
    public double AverageSatisfactionRating { get; init; }
    public Dictionary<InteractionType, int> InteractionsByType { get; init; } = new();
    public Dictionary<InteractionChannel, int> InteractionsByChannel { get; init; } = new();
    public Dictionary<InteractionOutcome, int> InteractionsByOutcome { get; init; } = new();
    public Dictionary<int, int> SatisfactionDistribution { get; init; } = new();
    public Dictionary<string, int> TopStaff { get; init; } = new();
    public Dictionary<string, int> PopularTags { get; init; } = new();
}

/// <summary>
/// DTO résumé pour les interactions
/// </summary>
public class InteractionSummaryDto
{
    public Guid Id { get; init; }
    public InteractionType Type { get; init; }
    public InteractionChannel Channel { get; init; }
    public required string Subject { get; init; }
    public DateTime InteractionDate { get; init; }
    public string? Staff { get; init; }
    public InteractionOutcome? Outcome { get; init; }
    public int? SatisfactionRating { get; init; }
    public bool RequiresFollowUp { get; init; }
    public bool IsOverdue { get; init; }
    public bool IsCompleted { get; init; }
}

/// <summary>
/// DTO pour l'historique des interactions d'un client
/// </summary>
public class CustomerInteractionHistoryDto
{
    public Guid CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public int TotalInteractions { get; init; }
    public DateTime? LastInteractionDate { get; init; }
    public double? AverageSatisfactionRating { get; init; }
    public List<InteractionSummaryDto> RecentInteractions { get; init; } = new();
    public Dictionary<InteractionType, int> InteractionsByType { get; init; } = new();
    public List<string> CommonTags { get; init; } = new();
    public int PendingFollowUps { get; init; }
    public InteractionTrendDto? Trend { get; init; }
}

/// <summary>
/// DTO pour les tendances d'interaction
/// </summary>
public class InteractionTrendDto
{
    public string Period { get; init; } = string.Empty;
    public int InteractionCount { get; init; }
    public double AverageSatisfaction { get; init; }
    public string TrendDirection { get; init; } = string.Empty; // "up", "down", "stable"
    public decimal TrendPercentage { get; init; }
}