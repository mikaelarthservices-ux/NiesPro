using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;
using Customer.Domain.Events;

namespace Customer.Domain.Entities;

/// <summary>
/// Programme de fidélité avec gestion des récompenses et niveaux
/// </summary>
public class LoyaltyProgram : Entity<Guid>
{
    private readonly List<LoyaltyReward> _rewards = new();
    private readonly List<PointTransaction> _pointTransactions = new();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int PointsPerEuroSpent { get; private set; }
    public int BronzeThreshold { get; private set; }
    public int SilverThreshold { get; private set; }
    public int GoldThreshold { get; private set; }
    public int PlatinumThreshold { get; private set; }
    public int DiamondThreshold { get; private set; }
    public int PointExpirationDays { get; private set; }
    public decimal ReferralBonusPoints { get; private set; }
    public string? Terms { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModifiedDate { get; private set; }

    // Collections en lecture seule
    public IReadOnlyCollection<LoyaltyReward> Rewards => _rewards.AsReadOnly();
    public IReadOnlyCollection<PointTransaction> PointTransactions => _pointTransactions.AsReadOnly();

    protected LoyaltyProgram() { }

    public LoyaltyProgram(
        string name,
        string description,
        int pointsPerEuroSpent = 1,
        int pointExpirationDays = 365)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (pointsPerEuroSpent <= 0)
            throw new ArgumentException("Points per euro must be positive", nameof(pointsPerEuroSpent));

        if (pointExpirationDays <= 0)
            throw new ArgumentException("Point expiration days must be positive", nameof(pointExpirationDays));

        Name = name.Trim();
        Description = description.Trim();
        PointsPerEuroSpent = pointsPerEuroSpent;
        PointExpirationDays = pointExpirationDays;
        IsActive = true;
        StartDate = DateTime.UtcNow;
        CreatedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;

        // Seuils par défaut
        BronzeThreshold = 0;
        SilverThreshold = 500;
        GoldThreshold = 2000;
        PlatinumThreshold = 5000;
        DiamondThreshold = 10000;
        ReferralBonusPoints = 100;

        // Récompenses par défaut
        CreateDefaultRewards();
    }

    // Méthodes métier pour la gestion du programme
    public void UpdateBasicInfo(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Name = name.Trim();
        Description = description.Trim();
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdatePointsConfiguration(int pointsPerEuroSpent, int pointExpirationDays)
    {
        if (pointsPerEuroSpent <= 0)
            throw new ArgumentException("Points per euro must be positive", nameof(pointsPerEuroSpent));

        if (pointExpirationDays <= 0)
            throw new ArgumentException("Point expiration days must be positive", nameof(pointExpirationDays));

        PointsPerEuroSpent = pointsPerEuroSpent;
        PointExpirationDays = pointExpirationDays;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateTierThresholds(
        int silverThreshold,
        int goldThreshold,
        int platinumThreshold,
        int diamondThreshold)
    {
        if (silverThreshold <= BronzeThreshold)
            throw new ArgumentException("Silver threshold must be higher than bronze", nameof(silverThreshold));

        if (goldThreshold <= silverThreshold)
            throw new ArgumentException("Gold threshold must be higher than silver", nameof(goldThreshold));

        if (platinumThreshold <= goldThreshold)
            throw new ArgumentException("Platinum threshold must be higher than gold", nameof(platinumThreshold));

        if (diamondThreshold <= platinumThreshold)
            throw new ArgumentException("Diamond threshold must be higher than platinum", nameof(diamondThreshold));

        SilverThreshold = silverThreshold;
        GoldThreshold = goldThreshold;
        PlatinumThreshold = platinumThreshold;
        DiamondThreshold = diamondThreshold;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateReferralBonus(decimal bonusPoints)
    {
        if (bonusPoints < 0)
            throw new ArgumentException("Referral bonus cannot be negative", nameof(bonusPoints));

        ReferralBonusPoints = bonusPoints;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateTerms(string? terms)
    {
        Terms = terms?.Trim();
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        EndDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les récompenses
    public void AddReward(LoyaltyReward reward)
    {
        if (reward == null)
            throw new ArgumentNullException(nameof(reward));

        _rewards.Add(reward);
        LastModifiedDate = DateTime.UtcNow;
    }

    public void RemoveReward(Guid rewardId)
    {
        var reward = _rewards.FirstOrDefault(r => r.Id == rewardId);
        if (reward != null)
        {
            _rewards.Remove(reward);
            LastModifiedDate = DateTime.UtcNow;
        }
    }

    public void UpdateReward(Guid rewardId, string name, string description, int pointsCost, RewardType type)
    {
        var reward = _rewards.FirstOrDefault(r => r.Id == rewardId);
        if (reward == null)
            throw new ArgumentException("Reward not found", nameof(rewardId));

        reward.Update(name, description, pointsCost, type);
        LastModifiedDate = DateTime.UtcNow;
    }

    // Méthodes métier pour les transactions de points
    public void RecordPointTransaction(PointTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        _pointTransactions.Add(transaction);
        LastModifiedDate = DateTime.UtcNow;
    }

    // Méthodes de calcul
    public int CalculatePointsForAmount(decimal amount)
    {
        if (amount <= 0)
            return 0;

        return (int)(amount * PointsPerEuroSpent);
    }

    public LoyaltyTier CalculateTierFromPoints(int lifetimePoints)
    {
        if (lifetimePoints >= DiamondThreshold)
            return LoyaltyTier.Diamond;
        if (lifetimePoints >= PlatinumThreshold)
            return LoyaltyTier.Platinum;
        if (lifetimePoints >= GoldThreshold)
            return LoyaltyTier.Gold;
        if (lifetimePoints >= SilverThreshold)
            return LoyaltyTier.Silver;
        
        return LoyaltyTier.Bronze;
    }

    public int GetPointsToNextTier(int currentPoints)
    {
        var currentTier = CalculateTierFromPoints(currentPoints);
        
        return currentTier switch
        {
            LoyaltyTier.Bronze => SilverThreshold - currentPoints,
            LoyaltyTier.Silver => GoldThreshold - currentPoints,
            LoyaltyTier.Gold => PlatinumThreshold - currentPoints,
            LoyaltyTier.Platinum => DiamondThreshold - currentPoints,
            LoyaltyTier.Diamond => 0,
            _ => 0
        };
    }

    public DateTime CalculatePointsExpirationDate()
    {
        return DateTime.UtcNow.AddDays(PointExpirationDays);
    }

    // Méthodes d'analyse
    public bool IsCurrentlyActive => IsActive && 
                                    DateTime.UtcNow >= StartDate && 
                                    (!EndDate.HasValue || DateTime.UtcNow <= EndDate.Value);

    public IEnumerable<LoyaltyReward> GetRewardsForTier(LoyaltyTier tier)
    {
        return _rewards.Where(r => r.IsActive && r.RequiredTier <= tier);
    }

    public IEnumerable<LoyaltyReward> GetRewardsWithinPoints(int availablePoints)
    {
        return _rewards.Where(r => r.IsActive && r.PointsCost <= availablePoints);
    }

    public int TotalActiveRewards => _rewards.Count(r => r.IsActive);

    public int GetTotalPointsIssued()
    {
        return _pointTransactions
            .Where(t => t.Type == PointTransactionType.Earned || t.Type == PointTransactionType.Bonus)
            .Sum(t => t.Points);
    }

    public int GetTotalPointsRedeemed()
    {
        return _pointTransactions
            .Where(t => t.Type == PointTransactionType.Redeemed)
            .Sum(t => t.Points);
    }

    // Méthodes privées
    private void CreateDefaultRewards()
    {
        // Récompenses par défaut du programme
        _rewards.Add(new LoyaltyReward(
            "Café Gratuit",
            "Un café ou thé gratuit",
            50,
            RewardType.FreeProduct,
            LoyaltyTier.Bronze));

        _rewards.Add(new LoyaltyReward(
            "Dessert Gratuit",
            "Un dessert de votre choix offert",
            100,
            RewardType.FreeProduct,
            LoyaltyTier.Silver));

        _rewards.Add(new LoyaltyReward(
            "Réduction 10%",
            "10% de réduction sur votre prochaine addition",
            200,
            RewardType.PercentageDiscount,
            LoyaltyTier.Silver));

        _rewards.Add(new LoyaltyReward(
            "Entrée Gratuite",
            "Une entrée gratuite lors de votre prochaine visite",
            300,
            RewardType.FreeProduct,
            LoyaltyTier.Gold));

        _rewards.Add(new LoyaltyReward(
            "Réduction 15%",
            "15% de réduction sur votre prochaine addition",
            400,
            RewardType.PercentageDiscount,
            LoyaltyTier.Gold));

        _rewards.Add(new LoyaltyReward(
            "Menu Dégustation",
            "Un menu dégustation gratuit pour deux personnes",
            800,
            RewardType.FreeService,
            LoyaltyTier.Platinum));

        _rewards.Add(new LoyaltyReward(
            "Accès VIP",
            "Accès à notre espace VIP pour un mois",
            1000,
            RewardType.VIPAccess,
            LoyaltyTier.Diamond));
    }
}