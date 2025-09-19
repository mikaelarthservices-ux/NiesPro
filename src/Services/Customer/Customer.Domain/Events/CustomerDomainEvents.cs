using BuildingBlocks.Domain.Events;

namespace Customer.Domain.Events;

/// <summary>
/// Événement déclenché lors de l'enregistrement d'un nouveau client
/// </summary>
public class CustomerRegisteredEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string? Phone { get; }
    public DateTime RegistrationDate { get; }
    public string RegistrationSource { get; }
    public bool IsEmailVerified { get; }
    public bool HasPhoneNumber { get; }
    public string? ReferralCode { get; }
    public Guid? ReferredByCustomerId { get; }

    public CustomerRegisteredEvent(
        Guid customerId,
        string email,
        string firstName,
        string lastName,
        string? phone,
        DateTime registrationDate,
        string registrationSource,
        bool isEmailVerified,
        string? referralCode = null,
        Guid? referredByCustomerId = null)
    {
        CustomerId = customerId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        RegistrationDate = registrationDate;
        RegistrationSource = registrationSource;
        IsEmailVerified = isEmailVerified;
        HasPhoneNumber = !string.IsNullOrWhiteSpace(phone);
        ReferralCode = referralCode;
        ReferredByCustomerId = referredByCustomerId;
    }
}

/// <summary>
/// Événement déclenché lors de la mise à jour du profil client
/// </summary>
public class CustomerProfileUpdatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Dictionary<string, object> ChangedFields { get; }
    public DateTime UpdateDate { get; }
    public string UpdatedBy { get; }
    public string? UpdateReason { get; }

    public CustomerProfileUpdatedEvent(
        Guid customerId,
        Dictionary<string, object> changedFields,
        string updatedBy,
        string? updateReason = null)
    {
        CustomerId = customerId;
        ChangedFields = changedFields ?? new Dictionary<string, object>();
        UpdateDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdateReason = updateReason;
    }
}

/// <summary>
/// Événement déclenché lors de la désactivation d'un client
/// </summary>
public class CustomerDeactivatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public string Reason { get; }
    public DateTime DeactivationDate { get; }
    public string DeactivatedBy { get; }
    public bool IsTemporary { get; }
    public DateTime? ReactivationDate { get; }

    public CustomerDeactivatedEvent(
        Guid customerId,
        string reason,
        string deactivatedBy,
        bool isTemporary = false,
        DateTime? reactivationDate = null)
    {
        CustomerId = customerId;
        Reason = reason;
        DeactivationDate = DateTime.UtcNow;
        DeactivatedBy = deactivatedBy;
        IsTemporary = isTemporary;
        ReactivationDate = reactivationDate;
    }
}

/// <summary>
/// Événement déclenché lors de la réactivation d'un client
/// </summary>
public class CustomerReactivatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public DateTime ReactivationDate { get; }
    public string ReactivatedBy { get; }
    public string? ReactivationReason { get; }
    public int DaysDeactivated { get; }

    public CustomerReactivatedEvent(
        Guid customerId,
        string reactivatedBy,
        string? reactivationReason = null,
        int daysDeactivated = 0)
    {
        CustomerId = customerId;
        ReactivationDate = DateTime.UtcNow;
        ReactivatedBy = reactivatedBy;
        ReactivationReason = reactivationReason;
        DaysDeactivated = daysDeactivated;
    }
}

/// <summary>
/// Événement déclenché lors du gain de points de fidélité
/// </summary>
public class LoyaltyPointsEarnedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid LoyaltyProgramId { get; }
    public int PointsEarned { get; }
    public int TotalPoints { get; }
    public string Source { get; }
    public string? SourceReference { get; }
    public DateTime EarnedDate { get; }
    public decimal? TransactionAmount { get; }
    public string? Notes { get; }

    public LoyaltyPointsEarnedEvent(
        Guid customerId,
        Guid loyaltyProgramId,
        int pointsEarned,
        int totalPoints,
        string source,
        string? sourceReference = null,
        decimal? transactionAmount = null,
        string? notes = null)
    {
        CustomerId = customerId;
        LoyaltyProgramId = loyaltyProgramId;
        PointsEarned = pointsEarned;
        TotalPoints = totalPoints;
        Source = source;
        SourceReference = sourceReference;
        EarnedDate = DateTime.UtcNow;
        TransactionAmount = transactionAmount;
        Notes = notes;
    }
}

/// <summary>
/// Événement déclenché lors de l'utilisation de points de fidélité
/// </summary>
public class LoyaltyPointsRedeemedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid LoyaltyProgramId { get; }
    public Guid RewardId { get; }
    public int PointsRedeemed { get; }
    public int RemainingPoints { get; }
    public string RewardName { get; }
    public decimal? RewardValue { get; }
    public DateTime RedeemedDate { get; }
    public string? TransactionReference { get; }

    public LoyaltyPointsRedeemedEvent(
        Guid customerId,
        Guid loyaltyProgramId,
        Guid rewardId,
        int pointsRedeemed,
        int remainingPoints,
        string rewardName,
        decimal? rewardValue = null,
        string? transactionReference = null)
    {
        CustomerId = customerId;
        LoyaltyProgramId = loyaltyProgramId;
        RewardId = rewardId;
        PointsRedeemed = pointsRedeemed;
        RemainingPoints = remainingPoints;
        RewardName = rewardName;
        RewardValue = rewardValue;
        RedeemedDate = DateTime.UtcNow;
        TransactionReference = transactionReference;
    }
}

/// <summary>
/// Événement déclenché lors d'un changement de niveau de fidélité
/// </summary>
public class LoyaltyTierChangedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid LoyaltyProgramId { get; }
    public string PreviousTier { get; }
    public string NewTier { get; }
    public DateTime ChangeDate { get; }
    public int TotalPoints { get; }
    public bool IsUpgrade { get; }
    public string? Reason { get; }
    public List<string> UnlockedBenefits { get; }

    public LoyaltyTierChangedEvent(
        Guid customerId,
        Guid loyaltyProgramId,
        string previousTier,
        string newTier,
        int totalPoints,
        string? reason = null,
        List<string>? unlockedBenefits = null)
    {
        CustomerId = customerId;
        LoyaltyProgramId = loyaltyProgramId;
        PreviousTier = previousTier;
        NewTier = newTier;
        ChangeDate = DateTime.UtcNow;
        TotalPoints = totalPoints;
        IsUpgrade = string.Compare(newTier, previousTier, StringComparison.OrdinalIgnoreCase) > 0;
        Reason = reason;
        UnlockedBenefits = unlockedBenefits ?? new List<string>();
    }
}

/// <summary>
/// Événement déclenché lors de l'attribution d'un segment client
/// </summary>
public class CustomerSegmentAssignedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid SegmentId { get; }
    public string SegmentName { get; }
    public DateTime AssignedDate { get; }
    public string AssignmentMethod { get; }
    public decimal MatchScore { get; }
    public List<string> MatchingCriteria { get; }
    public bool IsAutomatic { get; }

    public CustomerSegmentAssignedEvent(
        Guid customerId,
        Guid segmentId,
        string segmentName,
        string assignmentMethod,
        decimal matchScore,
        List<string> matchingCriteria,
        bool isAutomatic = true)
    {
        CustomerId = customerId;
        SegmentId = segmentId;
        SegmentName = segmentName;
        AssignedDate = DateTime.UtcNow;
        AssignmentMethod = assignmentMethod;
        MatchScore = matchScore;
        MatchingCriteria = matchingCriteria ?? new List<string>();
        IsAutomatic = isAutomatic;
    }
}

/// <summary>
/// Événement déclenché lors de la suppression d'un segment client
/// </summary>
public class CustomerSegmentRemovedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid SegmentId { get; }
    public string SegmentName { get; }
    public DateTime RemovedDate { get; }
    public string RemovalReason { get; }
    public bool IsAutomatic { get; }

    public CustomerSegmentRemovedEvent(
        Guid customerId,
        Guid segmentId,
        string segmentName,
        string removalReason,
        bool isAutomatic = true)
    {
        CustomerId = customerId;
        SegmentId = segmentId;
        SegmentName = segmentName;
        RemovedDate = DateTime.UtcNow;
        RemovalReason = removalReason;
        IsAutomatic = isAutomatic;
    }
}

/// <summary>
/// Événement déclenché lors de l'enregistrement d'une interaction client
/// </summary>
public class CustomerInteractionRecordedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid InteractionId { get; }
    public string InteractionType { get; }
    public string Channel { get; }
    public string Subject { get; }
    public DateTime InteractionDate { get; }
    public string? Staff { get; }
    public int? SatisfactionRating { get; }
    public bool RequiresFollowUp { get; }
    public DateTime? FollowUpDate { get; }

    public CustomerInteractionRecordedEvent(
        Guid customerId,
        Guid interactionId,
        string interactionType,
        string channel,
        string subject,
        string? staff = null,
        int? satisfactionRating = null,
        bool requiresFollowUp = false,
        DateTime? followUpDate = null)
    {
        CustomerId = customerId;
        InteractionId = interactionId;
        InteractionType = interactionType;
        Channel = channel;
        Subject = subject;
        InteractionDate = DateTime.UtcNow;
        Staff = staff;
        SatisfactionRating = satisfactionRating;
        RequiresFollowUp = requiresFollowUp;
        FollowUpDate = followUpDate;
    }
}

/// <summary>
/// Événement déclenché lors de l'ajout d'une préférence client
/// </summary>
public class CustomerPreferenceAddedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid PreferenceId { get; }
    public string PreferenceType { get; }
    public string Key { get; }
    public string Value { get; }
    public string Source { get; }
    public decimal? Confidence { get; }
    public DateTime AddedDate { get; }

    public CustomerPreferenceAddedEvent(
        Guid customerId,
        Guid preferenceId,
        string preferenceType,
        string key,
        string value,
        string source,
        decimal? confidence = null)
    {
        CustomerId = customerId;
        PreferenceId = preferenceId;
        PreferenceType = preferenceType;
        Key = key;
        Value = value;
        Source = source;
        Confidence = confidence;
        AddedDate = DateTime.UtcNow;
    }
}

/// <summary>
/// Événement déclenché lors de la mise à jour d'une préférence client
/// </summary>
public class CustomerPreferenceUpdatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public Guid PreferenceId { get; }
    public string PreferenceType { get; }
    public string Key { get; }
    public string OldValue { get; }
    public string NewValue { get; }
    public DateTime UpdatedDate { get; }
    public string UpdateReason { get; }

    public CustomerPreferenceUpdatedEvent(
        Guid customerId,
        Guid preferenceId,
        string preferenceType,
        string key,
        string oldValue,
        string newValue,
        string updateReason)
    {
        CustomerId = customerId;
        PreferenceId = preferenceId;
        PreferenceType = preferenceType;
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
        UpdatedDate = DateTime.UtcNow;
        UpdateReason = updateReason;
    }
}

/// <summary>
/// Événement déclenché lors de l'analyse du comportement client
/// </summary>
public class CustomerBehaviorAnalyzedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public DateTime AnalysisDate { get; }
    public Dictionary<string, object> BehaviorMetrics { get; }
    public List<string> IdentifiedPatterns { get; }
    public List<string> Recommendations { get; }
    public decimal EngagementScore { get; }
    public string RiskLevel { get; }

    public CustomerBehaviorAnalyzedEvent(
        Guid customerId,
        Dictionary<string, object> behaviorMetrics,
        List<string> identifiedPatterns,
        List<string> recommendations,
        decimal engagementScore,
        string riskLevel)
    {
        CustomerId = customerId;
        AnalysisDate = DateTime.UtcNow;
        BehaviorMetrics = behaviorMetrics ?? new Dictionary<string, object>();
        IdentifiedPatterns = identifiedPatterns ?? new List<string>();
        Recommendations = recommendations ?? new List<string>();
        EngagementScore = engagementScore;
        RiskLevel = riskLevel;
    }
}