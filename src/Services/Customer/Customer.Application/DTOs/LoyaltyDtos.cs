using Customer.Domain.Enums;

namespace Customer.Application.DTOs;

/// <summary>
/// DTO pour la création d'un programme de fidélité
/// </summary>
public class CreateLoyaltyProgramDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public LoyaltyProgramType Type { get; init; } = LoyaltyProgramType.Points;
    public bool IsActive { get; init; } = true;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    
    // Configuration des tiers
    public List<CreateLoyaltyTierDto> Tiers { get; init; } = new();
    
    // Règles de gain de points
    public decimal PointsPerEuro { get; init; } = 1.0m;
    public int BonusRegistrationPoints { get; init; } = 0;
    public int BonusBirthdayPoints { get; init; } = 0;
    
    // Configuration d'expiration
    public int? PointsExpirationMonths { get; init; }
    public bool NotifyBeforeExpiration { get; init; } = true;
    public int NotificationDaysBefore { get; init; } = 30;
}

/// <summary>
/// DTO pour un programme de fidélité
/// </summary>
public class LoyaltyProgramDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public LoyaltyProgramType Type { get; init; }
    public bool IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime CreatedDate { get; init; }
    
    // Configuration
    public decimal PointsPerEuro { get; init; }
    public int BonusRegistrationPoints { get; init; }
    public int BonusBirthdayPoints { get; init; }
    public int? PointsExpirationMonths { get; init; }
    public bool NotifyBeforeExpiration { get; init; }
    public int NotificationDaysBefore { get; init; }
    
    // Statistiques
    public int TotalMembers { get; init; }
    public int ActiveMembers { get; init; }
    public long TotalPointsIssued { get; init; }
    public long TotalPointsRedeemed { get; init; }
    
    // Tiers et récompenses
    public List<LoyaltyTierDto> Tiers { get; init; } = new();
    public List<LoyaltyRewardSummaryDto> AvailableRewards { get; init; } = new();
}

/// <summary>
/// DTO pour un tier de fidélité
/// </summary>
public class LoyaltyTierDto
{
    public required string Name { get; init; }
    public int RequiredPoints { get; init; }
    public decimal BonusMultiplier { get; init; }
    public List<string> Benefits { get; init; } = new();
    public string? Color { get; init; }
    public string? Icon { get; init; }
}

/// <summary>
/// DTO pour la création d'un tier
/// </summary>
public class CreateLoyaltyTierDto
{
    public required string Name { get; init; }
    public int RequiredPoints { get; init; }
    public decimal BonusMultiplier { get; init; } = 1.0m;
    public List<string> Benefits { get; init; } = new();
    public string? Color { get; init; }
    public string? Icon { get; init; }
}

/// <summary>
/// DTO pour une récompense de fidélité
/// </summary>
public class LoyaltyRewardDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public RewardType Type { get; init; }
    public int PointsCost { get; init; }
    public decimal? MonetaryValue { get; init; }
    public bool IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? MaxRedemptions { get; init; }
    public int? MaxRedemptionsPerCustomer { get; init; }
    public int CurrentRedemptions { get; init; }
    public List<string> RequiredTiers { get; init; } = new();
    public string? Terms { get; init; }
    public string? ImageUrl { get; init; }
    
    // Disponibilité
    public bool IsAvailable { get; init; }
    public bool IsExpiringSoon { get; init; }
    public string? UnavailabilityReason { get; init; }
}

/// <summary>
/// DTO résumé pour les récompenses
/// </summary>
public class LoyaltyRewardSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public RewardType Type { get; init; }
    public int PointsCost { get; init; }
    public bool IsAvailable { get; init; }
    public string? ImageUrl { get; init; }
}

/// <summary>
/// DTO pour la création d'une récompense
/// </summary>
public class CreateLoyaltyRewardDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public RewardType Type { get; init; }
    public int PointsCost { get; init; }
    public decimal? MonetaryValue { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? MaxRedemptions { get; init; }
    public int? MaxRedemptionsPerCustomer { get; init; }
    public List<string> RequiredTiers { get; init; } = new();
    public string? Terms { get; init; }
    public string? ImageUrl { get; init; }
}

/// <summary>
/// DTO pour la transaction de fidélité
/// </summary>
public class LoyaltyTransactionDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public LoyaltyTransactionType Type { get; init; }
    public int Points { get; init; }
    public string Source { get; init; } = string.Empty;
    public string? SourceReference { get; init; }
    public DateTime TransactionDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public decimal? RelatedAmount { get; init; }
    public string? Description { get; init; }
    public Guid? RewardId { get; init; }
    public string? RewardName { get; init; }
}

/// <summary>
/// DTO pour gagner des points
/// </summary>
public class EarnLoyaltyPointsDto
{
    public Guid CustomerId { get; init; }
    public Guid ProgramId { get; init; }
    public int Points { get; init; }
    public required string Source { get; init; }
    public string? SourceReference { get; init; }
    public decimal? TransactionAmount { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// DTO pour utiliser des points
/// </summary>
public class RedeemLoyaltyPointsDto
{
    public Guid CustomerId { get; init; }
    public Guid RewardId { get; init; }
    public int Quantity { get; init; } = 1;
    public string? TransactionReference { get; init; }
}

/// <summary>
/// DTO pour les statistiques d'un programme de fidélité
/// </summary>
public class LoyaltyProgramStatsDto
{
    public Guid ProgramId { get; init; }
    public required string ProgramName { get; init; }
    
    // Membres
    public int TotalMembers { get; init; }
    public int ActiveMembers { get; init; }
    public int NewMembersThisMonth { get; init; }
    public Dictionary<string, int> MembersByTier { get; init; } = new();
    
    // Points
    public long TotalPointsIssued { get; init; }
    public long TotalPointsRedeemed { get; init; }
    public long TotalPointsExpired { get; init; }
    public long ActivePoints { get; init; }
    public decimal RedemptionRate { get; init; }
    
    // Récompenses
    public int TotalRedemptions { get; init; }
    public int RedemptionsThisMonth { get; init; }
    public List<PopularRewardDto> MostPopularRewards { get; init; } = new();
    
    // Engagement
    public decimal EngagementRate { get; init; }
    public double AveragePointsPerMember { get; init; }
    public double AverageTransactionsPerMember { get; init; }
}

/// <summary>
/// DTO pour les récompenses populaires
/// </summary>
public class PopularRewardDto
{
    public Guid RewardId { get; init; }
    public required string RewardName { get; init; }
    public int RedemptionCount { get; init; }
    public int TotalPointsUsed { get; init; }
}