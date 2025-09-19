using BuildingBlocks.Domain.Entities;
using Customer.Domain.Enums;

namespace Customer.Domain.Entities;

/// <summary>
/// Récompense disponible dans un programme de fidélité
/// </summary>
public class LoyaltyReward : Entity<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int PointsCost { get; private set; }
    public RewardType Type { get; private set; }
    public LoyaltyTier RequiredTier { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public int? MaxRedemptionsPerCustomer { get; private set; }
    public int? TotalAvailableQuantity { get; private set; }
    public int CurrentRedemptionCount { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? TermsAndConditions { get; private set; }
    public decimal? MonetaryValue { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime LastModifiedDate { get; private set; }

    protected LoyaltyReward() { }

    public LoyaltyReward(
        string name,
        string description,
        int pointsCost,
        RewardType type,
        LoyaltyTier requiredTier = LoyaltyTier.Bronze,
        DateTime? validFrom = null,
        DateTime? validUntil = null,
        int? maxRedemptionsPerCustomer = null,
        int? totalAvailableQuantity = null,
        decimal? monetaryValue = null)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (pointsCost <= 0)
            throw new ArgumentException("Points cost must be positive", nameof(pointsCost));

        if (monetaryValue.HasValue && monetaryValue.Value < 0)
            throw new ArgumentException("Monetary value cannot be negative", nameof(monetaryValue));

        Name = name.Trim();
        Description = description.Trim();
        PointsCost = pointsCost;
        Type = type;
        RequiredTier = requiredTier;
        IsActive = true;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
        TotalAvailableQuantity = totalAvailableQuantity;
        MonetaryValue = monetaryValue;
        CurrentRedemptionCount = 0;
        CreatedDate = DateTime.UtcNow;
        LastModifiedDate = DateTime.UtcNow;
    }

    // Méthodes métier
    public void Update(string name, string description, int pointsCost, RewardType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (pointsCost <= 0)
            throw new ArgumentException("Points cost must be positive", nameof(pointsCost));

        Name = name.Trim();
        Description = description.Trim();
        PointsCost = pointsCost;
        Type = type;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateValidityPeriod(DateTime? validFrom, DateTime? validUntil)
    {
        if (validFrom.HasValue && validUntil.HasValue && validFrom.Value >= validUntil.Value)
            throw new ArgumentException("Valid from date must be before valid until date");

        ValidFrom = validFrom;
        ValidUntil = validUntil;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateLimitations(int? maxRedemptionsPerCustomer, int? totalAvailableQuantity)
    {
        if (maxRedemptionsPerCustomer.HasValue && maxRedemptionsPerCustomer.Value <= 0)
            throw new ArgumentException("Max redemptions per customer must be positive", nameof(maxRedemptionsPerCustomer));

        if (totalAvailableQuantity.HasValue && totalAvailableQuantity.Value <= 0)
            throw new ArgumentException("Total available quantity must be positive", nameof(totalAvailableQuantity));

        MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
        TotalAvailableQuantity = totalAvailableQuantity;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateImageAndTerms(string? imageUrl, string? termsAndConditions)
    {
        ImageUrl = imageUrl?.Trim();
        TermsAndConditions = termsAndConditions?.Trim();
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UpdateMonetaryValue(decimal? monetaryValue)
    {
        if (monetaryValue.HasValue && monetaryValue.Value < 0)
            throw new ArgumentException("Monetary value cannot be negative", nameof(monetaryValue));

        MonetaryValue = monetaryValue;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;
    }

    public bool CanBeRedeemed(LoyaltyTier customerTier, int customerRedemptionCount)
    {
        if (!IsActive)
            return false;

        if (customerTier < RequiredTier)
            return false;

        if (ValidFrom.HasValue && DateTime.UtcNow < ValidFrom.Value)
            return false;

        if (ValidUntil.HasValue && DateTime.UtcNow > ValidUntil.Value)
            return false;

        if (MaxRedemptionsPerCustomer.HasValue && customerRedemptionCount >= MaxRedemptionsPerCustomer.Value)
            return false;

        if (TotalAvailableQuantity.HasValue && CurrentRedemptionCount >= TotalAvailableQuantity.Value)
            return false;

        return true;
    }

    public void RecordRedemption()
    {
        if (TotalAvailableQuantity.HasValue && CurrentRedemptionCount >= TotalAvailableQuantity.Value)
            throw new InvalidOperationException("No more redemptions available for this reward");

        CurrentRedemptionCount++;
        LastModifiedDate = DateTime.UtcNow;
    }

    // Propriétés calculées
    public bool IsCurrentlyValid
    {
        get
        {
            var now = DateTime.UtcNow;
            return (!ValidFrom.HasValue || now >= ValidFrom.Value) &&
                   (!ValidUntil.HasValue || now <= ValidUntil.Value);
        }
    }

    public bool IsLimitedQuantity => TotalAvailableQuantity.HasValue;
    public bool IsLimitedPerCustomer => MaxRedemptionsPerCustomer.HasValue;
    public int RemainingQuantity => TotalAvailableQuantity.HasValue 
        ? Math.Max(0, TotalAvailableQuantity.Value - CurrentRedemptionCount) 
        : int.MaxValue;

    public bool IsAvailable => IsActive && IsCurrentlyValid && RemainingQuantity > 0;
}

/// <summary>
/// Transaction de points dans le programme de fidélité
/// </summary>
public class PointTransaction : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public Guid LoyaltyProgramId { get; private set; }
    public int Points { get; private set; }
    public PointTransactionType Type { get; private set; }
    public string Description { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public bool IsExpired { get; private set; }
    public string? Metadata { get; private set; }

    protected PointTransaction() { }

    public PointTransaction(
        Guid customerId,
        Guid loyaltyProgramId,
        int points,
        PointTransactionType type,
        string description,
        string? referenceId = null,
        DateTime? expirationDate = null,
        string? metadata = null)
        : base(Guid.NewGuid())
    {
        if (points <= 0)
            throw new ArgumentException("Points must be positive", nameof(points));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        CustomerId = customerId;
        LoyaltyProgramId = loyaltyProgramId;
        Points = points;
        Type = type;
        Description = description.Trim();
        ReferenceId = referenceId?.Trim();
        TransactionDate = DateTime.UtcNow;
        ExpirationDate = expirationDate;
        IsExpired = false;
        Metadata = metadata?.Trim();
    }

    public void MarkAsExpired()
    {
        if (Type != PointTransactionType.Earned && Type != PointTransactionType.Bonus)
            throw new InvalidOperationException("Only earned and bonus points can expire");

        IsExpired = true;
    }

    public bool ShouldExpire => ExpirationDate.HasValue && 
                               DateTime.UtcNow >= ExpirationDate.Value && 
                               !IsExpired &&
                               (Type == PointTransactionType.Earned || Type == PointTransactionType.Bonus);

    public int DaysUntilExpiration
    {
        get
        {
            if (!ExpirationDate.HasValue)
                return int.MaxValue;

            var days = (ExpirationDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, days);
        }
    }

    public bool IsExpiringWithinDays(int days)
    {
        return ExpirationDate.HasValue && 
               DaysUntilExpiration <= days && 
               DaysUntilExpiration > 0 &&
               !IsExpired;
    }
}