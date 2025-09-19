using Customer.Domain.Enums;

namespace Customer.Application.DTOs;

/// <summary>
/// DTO pour une préférence client
/// </summary>
public class CustomerPreferenceDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public PreferenceType Type { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? DisplayValue { get; init; }
    public int Priority { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime LastUsedDate { get; init; }
    public int UsageCount { get; init; }
    public string? Source { get; init; }
    public decimal? Confidence { get; init; }
    public string? Context { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Notes { get; init; }
    
    // Propriétés calculées
    public bool IsExpired { get; init; }
    public bool IsRecentlyUsed { get; init; }
    public bool IsFrequentlyUsed { get; init; }
    public bool IsHighConfidence { get; init; }
    public bool IsValid { get; init; }
    public decimal RelevanceScore { get; init; }
    public int DaysSinceLastUse { get; init; }
    public int DaysUntilExpiration { get; init; }
}

/// <summary>
/// DTO pour créer une préférence
/// </summary>
public class CreateCustomerPreferenceDto
{
    public Guid CustomerId { get; init; }
    public PreferenceType Type { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? DisplayValue { get; init; }
    public int Priority { get; init; } = 0;
    public string? Source { get; init; }
    public decimal? Confidence { get; init; }
    public string? Context { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO pour mettre à jour une préférence
/// </summary>
public class UpdateCustomerPreferenceDto
{
    public string? Value { get; init; }
    public string? DisplayValue { get; init; }
    public int? Priority { get; init; }
    public string? Source { get; init; }
    public decimal? Confidence { get; init; }
    public string? Context { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO pour les préférences culinaires
/// </summary>
public class CulinaryPreferenceDto
{
    public required string DishName { get; init; }
    public bool IsLiked { get; init; }
    public decimal Confidence { get; init; }
    public int TimesOrdered { get; init; }
    public DateTime LastOrdered { get; init; }
    public string? Category { get; init; }
    public List<string> Ingredients { get; init; } = new();
    public string? Notes { get; init; }
}

/// <summary>
/// DTO pour les préférences de table
/// </summary>
public class TablePreferenceDto
{
    public string? PreferredLocation { get; init; }
    public int? PreferredSize { get; init; }
    public bool AvoidHighTraffic { get; init; }
    public bool PreferWindow { get; init; }
    public bool PreferPrivate { get; init; }
    public string? SpecialRequests { get; init; }
}

/// <summary>
/// DTO pour les préférences d'ambiance
/// </summary>
public class AmbiancePreferenceDto
{
    public AmbiancePreference Primary { get; init; }
    public List<AmbiancePreference> Alternatives { get; init; } = new();
    public bool AvoidLoudMusic { get; init; }
    public bool PreferDimLighting { get; init; }
    public PreferredTimeSlot? PreferredTime { get; init; }
    public string? SpecialOccasions { get; init; }
}

/// <summary>
/// DTO pour rechercher les préférences
/// </summary>
public class PreferenceSearchDto
{
    public Guid? CustomerId { get; init; }
    public PreferenceType? Type { get; init; }
    public string? Key { get; init; }
    public string? Value { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsValid { get; init; }
    public decimal? MinConfidence { get; init; }
    public decimal? MaxConfidence { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    public DateTime? UsedAfter { get; init; }
    public string? Source { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// DTO pour les statistiques de préférences
/// </summary>
public class PreferenceStatsDto
{
    public int TotalPreferences { get; init; }
    public int ActivePreferences { get; init; }
    public int ExpiredPreferences { get; init; }
    public int HighConfidencePreferences { get; init; }
    public Dictionary<PreferenceType, int> PreferencesByType { get; init; } = new();
    public Dictionary<string, int> TopKeys { get; init; } = new();
    public Dictionary<string, int> TopValues { get; init; } = new();
    public Dictionary<string, int> PreferencesBySour 
    public double AverageConfidence { get; init; }
    public double AverageUsageCount { get; init; }
}

/// <summary>
/// DTO pour les conflits de préférences
/// </summary>
public class PreferenceConflictDto
{
    public Guid CustomerId { get; init; }
    public List<ConflictingPreferenceDto> Conflicts { get; init; } = new();
    public DateTime AnalysisDate { get; init; }
    public string? RecommendedResolution { get; init; }
}

/// <summary>
/// DTO pour un conflit de préférence
/// </summary>
public class ConflictingPreferenceDto
{
    public Guid PreferenceId1 { get; init; }
    public Guid PreferenceId2 { get; init; }
    public required string ConflictType { get; init; }
    public required string Description { get; init; }
    public decimal Severity { get; init; }
    public List<string> ResolutionOptions { get; init; } = new();
}

/// <summary>
/// DTO pour l'analyse comportementale d'un client
/// </summary>
public class CustomerBehaviorAnalysisDto
{
    public Guid CustomerId { get; init; }
    public DateTime AnalysisDate { get; init; }
    public int AnalysisPeriodDays { get; init; }
    
    // Métriques d'engagement
    public decimal EngagementScore { get; init; }
    public int TotalVisits { get; init; }
    public int TotalOrders { get; init; }
    public decimal TotalSpent { get; init; }
    public double AverageOrderValue { get; init; }
    public double AverageVisitsPerMonth { get; init; }
    
    // Patterns comportementaux
    public List<string> IdentifiedPatterns { get; init; } = new();
    public Dictionary<string, decimal> BehaviorScores { get; init; } = new();
    public List<string> PreferredDays { get; init; } = new();
    public List<string> PreferredTimes { get; init; } = new();
    public List<string> FavoriteCategories { get; init; } = new();
    
    // Prédictions et recommandations
    public decimal ChurnProbability { get; init; }
    public decimal LifetimeValuePrediction { get; init; }
    public List<string> RecommendedActions { get; init; } = new();
    public List<string> RiskFactors { get; init; } = new();
    public List<string> Opportunities { get; init; } = new();
    
    // Segmentation
    public List<string> RecommendedSegments { get; init; } = new();
    public Dictionary<string, decimal> SegmentAffinities { get; init; } = new();
}

/// <summary>
/// DTO pour les métriques de performance client
/// </summary>
public class CustomerPerformanceDto
{
    public Guid CustomerId { get; init; }
    public DateTime CalculationDate { get; init; }
    
    // Métriques financières
    public decimal LifetimeValue { get; init; }
    public decimal MonthlyValue { get; init; }
    public decimal AverageOrderValue { get; init; }
    public decimal TotalSpent { get; init; }
    public int TotalOrders { get; init; }
    
    // Métriques d'engagement
    public decimal EngagementScore { get; init; }
    public int VisitFrequency { get; init; }
    public decimal LoyaltyScore { get; init; }
    public decimal SatisfactionScore { get; init; }
    public decimal RetentionProbability { get; init; }
    
    // Métriques de risque
    public decimal ChurnProbability { get; init; }
    public string RiskLevel { get; init; } = string.Empty;
    public List<string> RiskFactors { get; init; } = new();
    
    // Comparaisons
    public decimal PercentileRank { get; init; }
    public string CustomerTier { get; init; } = string.Empty;
    public bool IsTopPerformer { get; init; }
    public bool IsAtRisk { get; init; }
}

/// <summary>
/// DTO pour les recommandations personnalisées
/// </summary>
public class CustomerRecommendationsDto
{
    public Guid CustomerId { get; init; }
    public DateTime GeneratedDate { get; init; }
    
    // Recommandations de produits/plats
    public List<ProductRecommendationDto> ProductRecommendations { get; init; } = new();
    
    // Recommandations d'actions marketing
    public List<MarketingActionDto> MarketingActions { get; init; } = new();
    
    // Recommandations d'engagement
    public List<EngagementActionDto> EngagementActions { get; init; } = new();
    
    // Prochaines meilleures actions
    public List<NextBestActionDto> NextBestActions { get; init; } = new();
}

/// <summary>
/// DTO pour une recommandation de produit
/// </summary>
public class ProductRecommendationDto
{
    public required string ProductName { get; init; }
    public string? Category { get; init; }
    public decimal Confidence { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public bool IsNew { get; init; }
    public bool IsSeasonal { get; init; }
}

/// <summary>
/// DTO pour une action marketing
/// </summary>
public class MarketingActionDto
{
    public required string ActionType { get; init; }
    public required string Description { get; init; }
    public decimal Priority { get; init; }
    public DateTime? RecommendedDate { get; init; }
    public string? Channel { get; init; }
    public string? Message { get; init; }
    public decimal ExpectedImpact { get; init; }
}

/// <summary>
/// DTO pour une action d'engagement
/// </summary>
public class EngagementActionDto
{
    public required string ActionType { get; init; }
    public required string Description { get; init; }
    public decimal Urgency { get; init; }
    public string? ResponsibleTeam { get; init; }
    public DateTime? DeadlineDate { get; init; }
    public List<string> RequiredResources { get; init; } = new();
}

/// <summary>
/// DTO pour la prochaine meilleure action
/// </summary>
public class NextBestActionDto
{
    public required string ActionType { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public decimal Priority { get; init; }
    public decimal SuccessProbability { get; init; }
    public decimal ExpectedValue { get; init; }
    public DateTime? RecommendedTiming { get; init; }
    public List<string> Prerequisites { get; init; } = new();
    public string? Channel { get; init; }
}

/// <summary>
/// DTO pour les métriques globales de la base clients
/// </summary>
public class CustomerAnalyticsOverviewDto
{
    public DateTime AnalysisDate { get; init; }
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int NewCustomersThisMonth { get; init; }
    public int ChurnedCustomersThisMonth { get; init; }
    
    // Métriques financières
    public decimal TotalRevenue { get; init; }
    public decimal AverageCustomerValue { get; init; }
    public decimal MonthlyRecurringRevenue { get; init; }
    public decimal CustomerAcquisitionCost { get; init; }
    
    // Métriques d'engagement
    public double AverageEngagementScore { get; init; }
    public double AverageSatisfactionScore { get; init; }
    public decimal RetentionRate { get; init; }
    public decimal ChurnRate { get; init; }
    
    // Distributions
    public Dictionary<string, int> CustomersByTier { get; init; } = new();
    public Dictionary<string, int> CustomersBySegment { get; init; } = new();
    public Dictionary<string, int> CustomersByRiskLevel { get; init; } = new();
    
    // Tendances
    public List<TrendDataPointDto> EngagementTrend { get; init; } = new();
    public List<TrendDataPointDto> RevenueTrend { get; init; } = new();
    public List<TrendDataPointDto> ChurnTrend { get; init; } = new();
}

/// <summary>
/// DTO pour un point de données de tendance
/// </summary>
public class TrendDataPointDto
{
    public DateTime Date { get; init; }
    public decimal Value { get; init; }
    public string? Label { get; init; }
}