using BuildingBlocks.Domain.ValueObjects;
using Customer.Domain.Enums;

namespace Customer.Domain.ValueObjects;

/// <summary>
/// Préférences de communication du client
/// </summary>
public class CommunicationSettings : ValueObject
{
    public CommunicationPreference PreferredChannel { get; private set; }
    public CommunicationFrequency MarketingFrequency { get; private set; }
    public bool AcceptsMarketing { get; private set; }
    public bool AcceptsPromotions { get; private set; }
    public bool AcceptsReservationReminders { get; private set; }
    public bool AcceptsEventNotifications { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public TimeZoneInfo? TimeZone { get; private set; }
    public TimeSpan? PreferredContactTime { get; private set; }

    protected CommunicationSettings() { }

    public CommunicationSettings(
        CommunicationPreference preferredChannel,
        CommunicationFrequency marketingFrequency = CommunicationFrequency.Monthly,
        bool acceptsMarketing = true,
        bool acceptsPromotions = true,
        bool acceptsReservationReminders = true,
        bool acceptsEventNotifications = true,
        string? preferredLanguage = "fr-FR",
        TimeZoneInfo? timeZone = null,
        TimeSpan? preferredContactTime = null)
    {
        PreferredChannel = preferredChannel;
        MarketingFrequency = marketingFrequency;
        AcceptsMarketing = acceptsMarketing;
        AcceptsPromotions = acceptsPromotions;
        AcceptsReservationReminders = acceptsReservationReminders;
        AcceptsEventNotifications = acceptsEventNotifications;
        PreferredLanguage = preferredLanguage;
        TimeZone = timeZone ?? TimeZoneInfo.Local;
        PreferredContactTime = preferredContactTime;
    }

    public bool CanReceiveMarketing => AcceptsMarketing && MarketingFrequency != CommunicationFrequency.Never;
    public bool CanReceivePromotions => AcceptsPromotions && PreferredChannel != CommunicationPreference.None;
    public bool CanReceiveNotifications => PreferredChannel != CommunicationPreference.None;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PreferredChannel;
        yield return MarketingFrequency;
        yield return AcceptsMarketing;
        yield return AcceptsPromotions;
        yield return AcceptsReservationReminders;
        yield return AcceptsEventNotifications;
        yield return PreferredLanguage ?? string.Empty;
        yield return TimeZone?.Id ?? string.Empty;
        yield return PreferredContactTime ?? TimeSpan.Zero;
    }

    public CommunicationSettings UpdatePreferredChannel(CommunicationPreference channel)
    {
        return new CommunicationSettings(
            channel, MarketingFrequency, AcceptsMarketing, AcceptsPromotions,
            AcceptsReservationReminders, AcceptsEventNotifications, PreferredLanguage, TimeZone, PreferredContactTime);
    }

    public CommunicationSettings UpdateMarketingFrequency(CommunicationFrequency frequency)
    {
        return new CommunicationSettings(
            PreferredChannel, frequency, AcceptsMarketing, AcceptsPromotions,
            AcceptsReservationReminders, AcceptsEventNotifications, PreferredLanguage, TimeZone, PreferredContactTime);
    }

    public CommunicationSettings OptOutOfMarketing()
    {
        return new CommunicationSettings(
            PreferredChannel, CommunicationFrequency.Never, false, AcceptsPromotions,
            AcceptsReservationReminders, AcceptsEventNotifications, PreferredLanguage, TimeZone, PreferredContactTime);
    }

    public CommunicationSettings OptInToMarketing(CommunicationFrequency frequency = CommunicationFrequency.Monthly)
    {
        return new CommunicationSettings(
            PreferredChannel, frequency, true, AcceptsPromotions,
            AcceptsReservationReminders, AcceptsEventNotifications, PreferredLanguage, TimeZone, PreferredContactTime);
    }
}

/// <summary>
/// Métadonnées de satisfaction client
/// </summary>
public class SatisfactionMetrics : ValueObject
{
    public decimal? AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public int PositiveReviews { get; private set; }
    public int NegativeReviews { get; private set; }
    public DateTime? LastReviewDate { get; private set; }
    public decimal? NetPromoterScore { get; private set; }

    protected SatisfactionMetrics() { }

    public SatisfactionMetrics(
        decimal? averageRating = null,
        int totalReviews = 0,
        int positiveReviews = 0,
        int negativeReviews = 0,
        DateTime? lastReviewDate = null,
        decimal? netPromoterScore = null)
    {
        if (averageRating.HasValue && (averageRating.Value < 0 || averageRating.Value > 5))
            throw new ArgumentException("Average rating must be between 0 and 5", nameof(averageRating));

        if (totalReviews < 0)
            throw new ArgumentException("Total reviews cannot be negative", nameof(totalReviews));

        if (positiveReviews < 0)
            throw new ArgumentException("Positive reviews cannot be negative", nameof(positiveReviews));

        if (negativeReviews < 0)
            throw new ArgumentException("Negative reviews cannot be negative", nameof(negativeReviews));

        if (netPromoterScore.HasValue && (netPromoterScore.Value < -100 || netPromoterScore.Value > 100))
            throw new ArgumentException("Net Promoter Score must be between -100 and 100", nameof(netPromoterScore));

        AverageRating = averageRating;
        TotalReviews = totalReviews;
        PositiveReviews = positiveReviews;
        NegativeReviews = negativeReviews;
        LastReviewDate = lastReviewDate;
        NetPromoterScore = netPromoterScore;
    }

    public bool HasReviews => TotalReviews > 0;
    public decimal PositiveReviewPercentage => TotalReviews > 0 ? (decimal)PositiveReviews / TotalReviews * 100 : 0;
    public decimal NegativeReviewPercentage => TotalReviews > 0 ? (decimal)NegativeReviews / TotalReviews * 100 : 0;
    public bool IsHighlySatisfied => AverageRating.HasValue && AverageRating.Value >= 4.5m;
    public bool IsLowSatisfaction => AverageRating.HasValue && AverageRating.Value <= 2.0m;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AverageRating ?? 0m;
        yield return TotalReviews;
        yield return PositiveReviews;
        yield return NegativeReviews;
        yield return LastReviewDate ?? DateTime.MinValue;
        yield return NetPromoterScore ?? 0m;
    }

    public SatisfactionMetrics AddReview(decimal rating, bool isPositive)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Rating must be between 0 and 5", nameof(rating));

        var newTotalReviews = TotalReviews + 1;
        var newAverageRating = AverageRating.HasValue 
            ? (AverageRating.Value * TotalReviews + rating) / newTotalReviews
            : rating;

        return new SatisfactionMetrics(
            newAverageRating,
            newTotalReviews,
            isPositive ? PositiveReviews + 1 : PositiveReviews,
            !isPositive ? NegativeReviews + 1 : NegativeReviews,
            DateTime.UtcNow,
            NetPromoterScore);
    }

    public SatisfactionMetrics UpdateNetPromoterScore(decimal score)
    {
        return new SatisfactionMetrics(
            AverageRating, TotalReviews, PositiveReviews, NegativeReviews, LastReviewDate, score);
    }
}