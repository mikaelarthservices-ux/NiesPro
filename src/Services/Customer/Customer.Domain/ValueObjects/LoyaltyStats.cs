using BuildingBlocks.Domain.ValueObjects;
using Customer.Domain.Enums;

namespace Customer.Domain.ValueObjects;

/// <summary>
/// Statistiques de fidélité d'un client
/// </summary>
public class LoyaltyStats : ValueObject
{
    public int TotalPoints { get; private set; }
    public int LifetimePoints { get; private set; }
    public LoyaltyTier CurrentTier { get; private set; }
    public DateTime TierAchievedDate { get; private set; }
    public int PointsToNextTier { get; private set; }
    public DateTime? PointsExpirationDate { get; private set; }
    public decimal TotalSpent { get; private set; }
    public int TotalVisits { get; private set; }
    public DateTime? LastVisitDate { get; private set; }

    protected LoyaltyStats() { }

    public LoyaltyStats(
        int totalPoints = 0,
        int lifetimePoints = 0,
        LoyaltyTier currentTier = LoyaltyTier.Bronze,
        DateTime? tierAchievedDate = null,
        int pointsToNextTier = 0,
        DateTime? pointsExpirationDate = null,
        decimal totalSpent = 0m,
        int totalVisits = 0,
        DateTime? lastVisitDate = null)
    {
        if (totalPoints < 0)
            throw new ArgumentException("Total points cannot be negative", nameof(totalPoints));

        if (lifetimePoints < totalPoints)
            throw new ArgumentException("Lifetime points cannot be less than total points", nameof(lifetimePoints));

        if (totalSpent < 0)
            throw new ArgumentException("Total spent cannot be negative", nameof(totalSpent));

        if (totalVisits < 0)
            throw new ArgumentException("Total visits cannot be negative", nameof(totalVisits));

        TotalPoints = totalPoints;
        LifetimePoints = lifetimePoints;
        CurrentTier = currentTier;
        TierAchievedDate = tierAchievedDate ?? DateTime.UtcNow;
        PointsToNextTier = pointsToNextTier;
        PointsExpirationDate = pointsExpirationDate;
        TotalSpent = totalSpent;
        TotalVisits = totalVisits;
        LastVisitDate = lastVisitDate;
    }

    public bool HasExpiredPoints => PointsExpirationDate.HasValue && PointsExpirationDate.Value <= DateTime.UtcNow;
    public decimal AverageSpentPerVisit => TotalVisits > 0 ? TotalSpent / TotalVisits : 0m;
    public bool IsActiveCustomer => LastVisitDate.HasValue && LastVisitDate.Value >= DateTime.UtcNow.AddDays(-90);
    public bool IsVIPTier => CurrentTier >= LoyaltyTier.Platinum;
    public int DaysSinceLastVisit => LastVisitDate.HasValue ? (DateTime.UtcNow - LastVisitDate.Value).Days : int.MaxValue;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalPoints;
        yield return LifetimePoints;
        yield return CurrentTier;
        yield return TierAchievedDate;
        yield return PointsToNextTier;
        yield return PointsExpirationDate ?? DateTime.MinValue;
        yield return TotalSpent;
        yield return TotalVisits;
        yield return LastVisitDate ?? DateTime.MinValue;
    }

    public LoyaltyStats AddPoints(int points, DateTime? expiration = null)
    {
        if (points <= 0)
            throw new ArgumentException("Points to add must be positive", nameof(points));

        return new LoyaltyStats(
            TotalPoints + points,
            LifetimePoints + points,
            CurrentTier,
            TierAchievedDate,
            Math.Max(0, PointsToNextTier - points),
            expiration ?? PointsExpirationDate,
            TotalSpent,
            TotalVisits,
            LastVisitDate);
    }

    public LoyaltyStats RedeemPoints(int points)
    {
        if (points <= 0)
            throw new ArgumentException("Points to redeem must be positive", nameof(points));

        if (points > TotalPoints)
            throw new ArgumentException("Cannot redeem more points than available", nameof(points));

        return new LoyaltyStats(
            TotalPoints - points,
            LifetimePoints,
            CurrentTier,
            TierAchievedDate,
            PointsToNextTier,
            PointsExpirationDate,
            TotalSpent,
            TotalVisits,
            LastVisitDate);
    }

    public LoyaltyStats UpgradeTier(LoyaltyTier newTier)
    {
        if (newTier <= CurrentTier)
            throw new ArgumentException("New tier must be higher than current tier", nameof(newTier));

        var newPointsToNextTier = CalculatePointsToNextTier(newTier);

        return new LoyaltyStats(
            TotalPoints,
            LifetimePoints,
            newTier,
            DateTime.UtcNow,
            newPointsToNextTier,
            PointsExpirationDate,
            TotalSpent,
            TotalVisits,
            LastVisitDate);
    }

    public LoyaltyStats RecordVisit(decimal amountSpent)
    {
        if (amountSpent < 0)
            throw new ArgumentException("Amount spent cannot be negative", nameof(amountSpent));

        return new LoyaltyStats(
            TotalPoints,
            LifetimePoints,
            CurrentTier,
            TierAchievedDate,
            PointsToNextTier,
            PointsExpirationDate,
            TotalSpent + amountSpent,
            TotalVisits + 1,
            DateTime.UtcNow);
    }

    private static int CalculatePointsToNextTier(LoyaltyTier currentTier)
    {
        return currentTier switch
        {
            LoyaltyTier.Bronze => 500,  // Points needed for Silver
            LoyaltyTier.Silver => 1000, // Points needed for Gold
            LoyaltyTier.Gold => 2000,   // Points needed for Platinum
            LoyaltyTier.Platinum => 5000, // Points needed for Diamond
            LoyaltyTier.Diamond => 0,   // Max tier reached
            _ => 0
        };
    }
}

/// <summary>
/// Métadonnées comportementales du client
/// </summary>
public class BehavioralMetrics : ValueObject
{
    public decimal AverageOrderValue { get; private set; }
    public int VisitFrequencyDays { get; private set; }
    public TimeSpan AverageVisitDuration { get; private set; }
    public string[] PreferredMenuCategories { get; private set; }
    public string[] PreferredTimeSlots { get; private set; }
    public int CancellationRate { get; private set; }
    public int NoShowRate { get; private set; }
    public bool IsImpulseBuyer { get; private set; }
    public bool IsPriceConscious { get; private set; }

    protected BehavioralMetrics() 
    {
        PreferredMenuCategories = Array.Empty<string>();
        PreferredTimeSlots = Array.Empty<string>();
    }

    public BehavioralMetrics(
        decimal averageOrderValue = 0m,
        int visitFrequencyDays = 0,
        TimeSpan averageVisitDuration = default,
        string[]? preferredMenuCategories = null,
        string[]? preferredTimeSlots = null,
        int cancellationRate = 0,
        int noShowRate = 0,
        bool isImpulseBuyer = false,
        bool isPriceConscious = false)
    {
        if (averageOrderValue < 0)
            throw new ArgumentException("Average order value cannot be negative", nameof(averageOrderValue));

        if (visitFrequencyDays < 0)
            throw new ArgumentException("Visit frequency cannot be negative", nameof(visitFrequencyDays));

        if (cancellationRate < 0 || cancellationRate > 100)
            throw new ArgumentException("Cancellation rate must be between 0 and 100", nameof(cancellationRate));

        if (noShowRate < 0 || noShowRate > 100)
            throw new ArgumentException("No-show rate must be between 0 and 100", nameof(noShowRate));

        AverageOrderValue = averageOrderValue;
        VisitFrequencyDays = visitFrequencyDays;
        AverageVisitDuration = averageVisitDuration;
        PreferredMenuCategories = preferredMenuCategories ?? Array.Empty<string>();
        PreferredTimeSlots = preferredTimeSlots ?? Array.Empty<string>();
        CancellationRate = cancellationRate;
        NoShowRate = noShowRate;
        IsImpulseBuyer = isImpulseBuyer;
        IsPriceConscious = isPriceConscious;
    }

    public bool IsFrequentVisitor => VisitFrequencyDays > 0 && VisitFrequencyDays <= 7;
    public bool IsHighValueCustomer => AverageOrderValue >= 100m;
    public bool IsReliableCustomer => CancellationRate <= 5 && NoShowRate <= 2;
    public bool HasStrongPreferences => PreferredMenuCategories.Length >= 3;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AverageOrderValue;
        yield return VisitFrequencyDays;
        yield return AverageVisitDuration;
        yield return string.Join(",", PreferredMenuCategories);
        yield return string.Join(",", PreferredTimeSlots);
        yield return CancellationRate;
        yield return NoShowRate;
        yield return IsImpulseBuyer;
        yield return IsPriceConscious;
    }

    public BehavioralMetrics UpdateAverageOrderValue(decimal newValue)
    {
        return new BehavioralMetrics(
            newValue, VisitFrequencyDays, AverageVisitDuration, PreferredMenuCategories,
            PreferredTimeSlots, CancellationRate, NoShowRate, IsImpulseBuyer, IsPriceConscious);
    }

    public BehavioralMetrics UpdatePreferredCategories(string[] categories)
    {
        return new BehavioralMetrics(
            AverageOrderValue, VisitFrequencyDays, AverageVisitDuration, categories,
            PreferredTimeSlots, CancellationRate, NoShowRate, IsImpulseBuyer, IsPriceConscious);
    }
}