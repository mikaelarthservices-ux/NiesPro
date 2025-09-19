using BuildingBlocks.Domain.Repositories;
using Customer.Domain.Entities;
using Customer.Domain.Enums;

namespace Customer.Domain.Repositories;

/// <summary>
/// Repository pour la gestion des clients
/// </summary>
public interface ICustomerRepository : IRepository<Entities.Customer>
{
    // Recherche de base
    Task<Entities.Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Entities.Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<List<Entities.Customer>> GetByEmailsAsync(List<string> emails, CancellationToken cancellationToken = default);
    Task<List<Entities.Customer>> GetByIdsAsync(List<Guid> customerIds, CancellationToken cancellationToken = default);

    // Recherche avancée
    Task<List<Entities.Customer>> SearchAsync(
        string? searchTerm = null,
        CustomerStatus? status = null,
        DateTime? registeredAfter = null,
        DateTime? registeredBefore = null,
        bool? hasPhone = null,
        bool? isEmailVerified = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        CustomerStatus? status = null,
        DateTime? registeredAfter = null,
        DateTime? registeredBefore = null,
        CancellationToken cancellationToken = default);

    // Méthodes spécialisées
    Task<List<Entities.Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
    Task<List<Entities.Customer>> GetInactiveCustomersAsync(int daysInactive, CancellationToken cancellationToken = default);
    Task<List<Entities.Customer>> GetRecentRegistrationsAsync(int days, CancellationToken cancellationToken = default);
    Task<List<Entities.Customer>> GetCustomersWithBirthdayAsync(int daysFromNow = 0, CancellationToken cancellationToken = default);

    // Validation
    Task<bool> ExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeCustomerId = null, CancellationToken cancellationToken = default);
    Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeCustomerId = null, CancellationToken cancellationToken = default);

    // Statistiques
    Task<Dictionary<CustomerStatus, int>> GetCustomerCountByStatusAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetRegistrationStatsAsync(int days, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des profils clients
/// </summary>
public interface ICustomerProfileRepository : IRepository<CustomerProfile>
{
    Task<CustomerProfile?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<CustomerProfile>> GetByCustomerIdsAsync(List<Guid> customerIds, CancellationToken cancellationToken = default);
    
    // Recherche par préférences
    Task<List<CustomerProfile>> GetByDietaryRestrictionsAsync(List<DietaryRestriction> restrictions, CancellationToken cancellationToken = default);
    Task<List<CustomerProfile>> GetByAllergiesAsync(List<string> allergies, CancellationToken cancellationToken = default);
    Task<List<CustomerProfile>> GetByAmbiancePreferenceAsync(AmbiancePreference ambiance, CancellationToken cancellationToken = default);
    Task<List<CustomerProfile>> GetByTimeSlotPreferenceAsync(PreferredTimeSlot timeSlot, CancellationToken cancellationToken = default);

    // Statistiques de profil
    Task<Dictionary<DietaryRestriction, int>> GetDietaryRestrictionsStatsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<AmbiancePreference, int>> GetAmbiancePreferencesStatsAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetTopAllergiesAsync(int top = 10, CancellationToken cancellationToken = default);

    // Profils incomplets
    Task<List<CustomerProfile>> GetIncompleteProfilesAsync(decimal minCompletionPercentage = 50m, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des programmes de fidélité
/// </summary>
public interface ILoyaltyProgramRepository : IRepository<LoyaltyProgram>
{
    Task<List<LoyaltyProgram>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<LoyaltyProgram?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<LoyaltyProgram>> GetByTypeAsync(LoyaltyProgramType type, CancellationToken cancellationToken = default);
    
    // Programmes par client
    Task<List<LoyaltyProgram>> GetCustomerProgramsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<LoyaltyProgram?> GetCustomerPrimaryProgramAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Statistiques
    Task<Dictionary<LoyaltyProgramType, int>> GetProgramCountByTypeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetMemberCountByProgramAsync(CancellationToken cancellationToken = default);
    
    // Validation
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeProgramId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des récompenses de fidélité
/// </summary>
public interface ILoyaltyRewardRepository : IRepository<LoyaltyReward>
{
    Task<List<LoyaltyReward>> GetByProgramAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetActiveByProgramAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetByTypeAsync(RewardType type, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetAvailableForCustomerAsync(Guid customerId, Guid programId, CancellationToken cancellationToken = default);
    
    // Recherche par points
    Task<List<LoyaltyReward>> GetByPointRangeAsync(Guid programId, int minPoints, int maxPoints, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetAffordableRewardsAsync(Guid programId, int customerPoints, CancellationToken cancellationToken = default);
    
    // Statistiques
    Task<Dictionary<RewardType, int>> GetRewardCountByTypeAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetMostPopularRewardsAsync(Guid programId, int top = 10, CancellationToken cancellationToken = default);
    Task<List<LoyaltyReward>> GetExpiringRewardsAsync(int daysFromNow, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des segments clients
/// </summary>
public interface ICustomerSegmentRepository : IRepository<CustomerSegment>
{
    Task<List<CustomerSegment>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<CustomerSegment?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<CustomerSegment>> GetByTypeAsync(SegmentType type, CancellationToken cancellationToken = default);
    
    // Segments par client
    Task<List<CustomerSegment>> GetCustomerSegmentsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetSegmentCustomersAsync(Guid segmentId, CancellationToken cancellationToken = default);
    
    // Évaluation et correspondance
    Task<List<CustomerSegment>> GetMatchingSegmentsAsync(Guid customerId, decimal minMatchScore = 0.7m, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, decimal>> EvaluateCustomerForAllSegmentsAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Statistiques
    Task<Dictionary<SegmentType, int>> GetSegmentCountByTypeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetCustomerCountBySegmentAsync(CancellationToken cancellationToken = default);
    Task<List<CustomerSegment>> GetSegmentsNeedingRefreshAsync(CancellationToken cancellationToken = default);
    
    // Validation
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeSegmentId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des interactions clients
/// </summary>
public interface ICustomerInteractionRepository : IRepository<CustomerInteraction>
{
    // Interactions par client
    Task<List<CustomerInteraction>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<CustomerInteraction>> GetByCustomerAsync(
        Guid customerId,
        InteractionType? type = null,
        InteractionChannel? channel = null,
        DateTime? from = null,
        DateTime? to = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    // Interactions par type et canal
    Task<List<CustomerInteraction>> GetByTypeAsync(InteractionType type, CancellationToken cancellationToken = default);
    Task<List<CustomerInteraction>> GetByChannelAsync(InteractionChannel channel, CancellationToken cancellationToken = default);
    
    // Suivi et satisfaction
    Task<List<CustomerInteraction>> GetPendingFollowUpsAsync(CancellationToken cancellationToken = default);
    Task<List<CustomerInteraction>> GetOverdueFollowUpsAsync(CancellationToken cancellationToken = default);
    Task<List<CustomerInteraction>> GetBySatisfactionRangeAsync(int minRating, int maxRating, CancellationToken cancellationToken = default);
    
    // Interactions récentes
    Task<List<CustomerInteraction>> GetRecentInteractionsAsync(int days, CancellationToken cancellationToken = default);
    Task<CustomerInteraction?> GetLastInteractionAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Statistiques
    Task<Dictionary<InteractionType, int>> GetInteractionCountByTypeAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<Dictionary<InteractionChannel, int>> GetInteractionCountByChannelAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<double> GetAverageSatisfactionRatingAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetSatisfactionDistributionAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour la gestion des préférences clients
/// </summary>
public interface ICustomerPreferenceRepository : IRepository<CustomerPreference>
{
    // Préférences par client
    Task<List<CustomerPreference>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<CustomerPreference>> GetByCustomerAndTypeAsync(Guid customerId, PreferenceType type, CancellationToken cancellationToken = default);
    Task<CustomerPreference?> GetByCustomerAndKeyAsync(Guid customerId, string key, CancellationToken cancellationToken = default);
    
    // Préférences actives
    Task<List<CustomerPreference>> GetActiveByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<CustomerPreference>> GetValidByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Préférences par type
    Task<List<CustomerPreference>> GetByTypeAsync(PreferenceType type, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetValueDistributionAsync(PreferenceType type, string key, CancellationToken cancellationToken = default);
    
    // Analyse des préférences
    Task<List<CustomerPreference>> GetHighConfidencePreferencesAsync(Guid customerId, decimal minConfidence = 0.8m, CancellationToken cancellationToken = default);
    Task<List<CustomerPreference>> GetRecentlyUsedAsync(Guid customerId, int days = 30, CancellationToken cancellationToken = default);
    Task<List<CustomerPreference>> GetConflictingPreferencesAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Préférences expirées
    Task<List<CustomerPreference>> GetExpiringPreferencesAsync(int daysFromNow, CancellationToken cancellationToken = default);
    Task<List<CustomerPreference>> GetExpiredPreferencesAsync(CancellationToken cancellationToken = default);
    
    // Statistiques
    Task<Dictionary<PreferenceType, int>> GetPreferenceCountByTypeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTopPreferenceKeysAsync(PreferenceType type, int top = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository pour les requêtes cross-entity complexes
/// </summary>
public interface ICustomerAnalyticsRepository
{
    // Analyse comportementale
    Task<Dictionary<string, object>> GetCustomerBehaviorAnalyticsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetSimilarCustomersAsync(Guid customerId, int top = 10, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCustomerEngagementMetricsAsync(Guid customerId, int days = 90, CancellationToken cancellationToken = default);
    
    // Analyse de segments
    Task<Dictionary<string, object>> GetSegmentPerformanceAsync(Guid segmentId, CancellationToken cancellationToken = default);
    Task<List<string>> GetSegmentRecommendationsAsync(Guid customerId, CancellationToken cancellationToken = default);
    
    // Analyse de fidélité
    Task<Dictionary<string, object>> GetLoyaltyProgramPerformanceAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetLoyaltyRiskCustomersAsync(Guid programId, CancellationToken cancellationToken = default);
    
    // Prédictions
    Task<decimal> GetChurnProbabilityAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<decimal> GetLifetimeValuePredictionAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<string>> GetNextBestActionsAsync(Guid customerId, CancellationToken cancellationToken = default);
}