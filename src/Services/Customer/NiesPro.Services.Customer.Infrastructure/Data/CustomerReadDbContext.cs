using Microsoft.EntityFrameworkCore;
using NiesPro.Services.Customer.Domain.Entities;

namespace NiesPro.Services.Customer.Infrastructure.Data;

/// <summary>
/// DbContext optimisé pour les requêtes en lecture seule (CQRS Query Side)
/// Configuration spécialisée pour la performance des lectures
/// </summary>
public class CustomerReadDbContext : DbContext
{
    public CustomerReadDbContext(DbContextOptions<CustomerReadDbContext> options) : base(options)
    {
        // Optimisations pour lecture seule
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    // ===== READ-ONLY ENTITIES =====
    public DbSet<Domain.Entities.Customer> Customers { get; set; } = null!;
    public DbSet<CustomerProfile> CustomerProfiles { get; set; } = null!;
    public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; } = null!;
    public DbSet<LoyaltyReward> LoyaltyRewards { get; set; } = null!;
    public DbSet<CustomerSegment> CustomerSegments { get; set; } = null!;
    public DbSet<CustomerInteraction> CustomerInteractions { get; set; } = null!;
    public DbSet<CustomerPreference> CustomerPreferences { get; set; } = null!;

    // ===== ANALYTICS VIEWS =====
    public DbSet<CustomerAnalyticsView> CustomerAnalytics { get; set; } = null!;
    public DbSet<LoyaltyAnalyticsView> LoyaltyAnalytics { get; set; } = null!;
    public DbSet<SegmentAnalyticsView> SegmentAnalytics { get; set; } = null!;
    public DbSet<InteractionAnalyticsView> InteractionAnalytics { get; set; } = null!;
    public DbSet<PreferenceAnalyticsView> PreferenceAnalytics { get; set; } = null!;

    /// <summary>
    /// Configuration du modèle pour lecture optimisée
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== SCHEMA CONFIGURATION =====
        modelBuilder.HasDefaultSchema("customer");

        // ===== RÉUTILISE LES CONFIGURATIONS EXISTANTES =====
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);

        // ===== VUES ANALYTICS =====
        ConfigureAnalyticsViews(modelBuilder);

        // ===== OPTIMISATIONS LECTURE =====
        ConfigureReadOptimizations(modelBuilder);
    }

    /// <summary>
    /// Configuration des vues pour analytics avancées
    /// </summary>
    private void ConfigureAnalyticsViews(ModelBuilder modelBuilder)
    {
        // ===== CUSTOMER ANALYTICS VIEW =====
        modelBuilder.Entity<CustomerAnalyticsView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_CustomerAnalytics", "customer");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerId");
            entity.Property(e => e.CustomerNumber).HasColumnName("CustomerNumber");
            entity.Property(e => e.FullName).HasColumnName("FullName");
            entity.Property(e => e.Email).HasColumnName("Email");
            entity.Property(e => e.RegistrationDate).HasColumnName("RegistrationDate");
            entity.Property(e => e.LastLoginDate).HasColumnName("LastLoginDate");
            entity.Property(e => e.TotalOrders).HasColumnName("TotalOrders");
            entity.Property(e => e.TotalSpent).HasColumnName("TotalSpent");
            entity.Property(e => e.AverageOrderValue).HasColumnName("AverageOrderValue");
            entity.Property(e => e.DaysSinceLastOrder).HasColumnName("DaysSinceLastOrder");
            entity.Property(e => e.LoyaltyPoints).HasColumnName("LoyaltyPoints");
            entity.Property(e => e.LoyaltyTier).HasColumnName("LoyaltyTier");
            entity.Property(e => e.ChurnRiskScore).HasColumnName("ChurnRiskScore");
            entity.Property(e => e.CustomerLifetimeValue).HasColumnName("CustomerLifetimeValue");
            entity.Property(e => e.PreferredChannel).HasColumnName("PreferredChannel");
            entity.Property(e => e.LastInteractionDate).HasColumnName("LastInteractionDate");
            entity.Property(e => e.TotalInteractions).HasColumnName("TotalInteractions");
            entity.Property(e => e.IsVip).HasColumnName("IsVip");
            entity.Property(e => e.Country).HasColumnName("Country");
            entity.Property(e => e.City).HasColumnName("City");
            entity.Property(e => e.Age).HasColumnName("Age");
            entity.Property(e => e.Gender).HasColumnName("Gender");
        });

        // ===== LOYALTY ANALYTICS VIEW =====
        modelBuilder.Entity<LoyaltyAnalyticsView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_LoyaltyAnalytics", "customer");

            entity.Property(e => e.ProgramId).HasColumnName("ProgramId");
            entity.Property(e => e.ProgramName).HasColumnName("ProgramName");
            entity.Property(e => e.ProgramType).HasColumnName("ProgramType");
            entity.Property(e => e.TotalMembers).HasColumnName("TotalMembers");
            entity.Property(e => e.ActiveMembers).HasColumnName("ActiveMembers");
            entity.Property(e => e.BronzeMembers).HasColumnName("BronzeMembers");
            entity.Property(e => e.SilverMembers).HasColumnName("SilverMembers");
            entity.Property(e => e.GoldMembers).HasColumnName("GoldMembers");
            entity.Property(e => e.PlatinumMembers).HasColumnName("PlatinumMembers");
            entity.Property(e => e.DiamondMembers).HasColumnName("DiamondMembers");
            entity.Property(e => e.TotalPointsIssued).HasColumnName("TotalPointsIssued");
            entity.Property(e => e.TotalPointsRedeemed).HasColumnName("TotalPointsRedeemed");
            entity.Property(e => e.AveragePointsPerMember).HasColumnName("AveragePointsPerMember");
            entity.Property(e => e.RedemptionRate).HasColumnName("RedemptionRate");
            entity.Property(e => e.MemberGrowthRate).HasColumnName("MemberGrowthRate");
            entity.Property(e => e.AverageEngagementScore).HasColumnName("AverageEngagementScore");
        });

        // ===== SEGMENT ANALYTICS VIEW =====
        modelBuilder.Entity<SegmentAnalyticsView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_SegmentAnalytics", "customer");

            entity.Property(e => e.SegmentId).HasColumnName("SegmentId");
            entity.Property(e => e.SegmentName).HasColumnName("SegmentName");
            entity.Property(e => e.SegmentType).HasColumnName("SegmentType");
            entity.Property(e => e.MemberCount).HasColumnName("MemberCount");
            entity.Property(e => e.AverageCustomerValue).HasColumnName("AverageCustomerValue");
            entity.Property(e => e.TotalRevenue).HasColumnName("TotalRevenue");
            entity.Property(e => e.AverageOrderValue).HasColumnName("AverageOrderValue");
            entity.Property(e => e.ConversionRate).HasColumnName("ConversionRate");
            entity.Property(e => e.ChurnRate).HasColumnName("ChurnRate");
            entity.Property(e => e.GrowthRate).HasColumnName("GrowthRate");
            entity.Property(e => e.EngagementScore).HasColumnName("EngagementScore");
            entity.Property(e => e.SatisfactionScore).HasColumnName("SatisfactionScore");
        });

        // ===== INTERACTION ANALYTICS VIEW =====
        modelBuilder.Entity<InteractionAnalyticsView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_InteractionAnalytics", "customer");

            entity.Property(e => e.Period).HasColumnName("Period");
            entity.Property(e => e.InteractionType).HasColumnName("InteractionType");
            entity.Property(e => e.Channel).HasColumnName("Channel");
            entity.Property(e => e.TotalInteractions).HasColumnName("TotalInteractions");
            entity.Property(e => e.UniqueCustomers).HasColumnName("UniqueCustomers");
            entity.Property(e => e.AverageDuration).HasColumnName("AverageDuration");
            entity.Property(e => e.ResolutionRate).HasColumnName("ResolutionRate");
            entity.Property(e => e.AverageSatisfactionScore).HasColumnName("AverageSatisfactionScore");
            entity.Property(e => e.PositiveSentimentRate).HasColumnName("PositiveSentimentRate");
            entity.Property(e => e.FirstContactResolutionRate).HasColumnName("FirstContactResolutionRate");
            entity.Property(e => e.EscalationRate).HasColumnName("EscalationRate");
        });

        // ===== PREFERENCE ANALYTICS VIEW =====
        modelBuilder.Entity<PreferenceAnalyticsView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_PreferenceAnalytics", "customer");

            entity.Property(e => e.PreferenceType).HasColumnName("PreferenceType");
            entity.Property(e => e.Category).HasColumnName("Category");
            entity.Property(e => e.TotalCustomers).HasColumnName("TotalCustomers");
            entity.Property(e => e.AverageConfidence).HasColumnName("AverageConfidence");
            entity.Property(e => e.AverageUsageCount).HasColumnName("AverageUsageCount");
            entity.Property(e => e.AverageSuccessRate).HasColumnName("AverageSuccessRate");
            entity.Property(e => e.AverageClickThroughRate).HasColumnName("AverageClickThroughRate");
            entity.Property(e => e.AverageConversionRate).HasColumnName("AverageConversionRate");
            entity.Property(e => e.TotalRevenue).HasColumnName("TotalRevenue");
            entity.Property(e => e.ExplicitPreferences).HasColumnName("ExplicitPreferences");
            entity.Property(e => e.InferredPreferences).HasColumnName("InferredPreferences");
        });
    }

    /// <summary>
    /// Optimisations spécifiques pour les lectures
    /// </summary>
    private void ConfigureReadOptimizations(ModelBuilder modelBuilder)
    {
        // ===== INDEXES ADDITIONNELS POUR LECTURES =====
        
        // Index covering pour liste clients
        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => new { c.Status, c.RegistrationDate })
            .HasDatabaseName("IX_Customer_Read_List_Covering")
            .IncludeProperties(c => new { 
                c.CustomerNumber,
                c.LastLoginDate,
                c.IsEmailVerified,
                c.IsVip 
            });

        // Index covering pour recherche clients
        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex("FirstName", "LastName", "Email")
            .HasDatabaseName("IX_Customer_Read_Search_Covering")
            .IncludeProperties(c => new { 
                c.CustomerNumber,
                c.Status,
                c.RegistrationDate 
            });

        // Index covering pour dashboard loyalty
        modelBuilder.Entity<LoyaltyProgram>()
            .HasIndex(lp => new { lp.IsActive, lp.ProgramType })
            .HasDatabaseName("IX_LoyaltyProgram_Read_Dashboard_Covering")
            .IncludeProperties(lp => new { 
                lp.Name,
                lp.CurrentMembers,
                lp.MaxMembers 
            });

        // ===== CONFIGURATION POUR PROCÉDURES STOCKÉES =====
        ConfigureStoredProcedures(modelBuilder);
    }

    /// <summary>
    /// Configuration des procédures stockées pour analytics
    /// </summary>
    private void ConfigureStoredProcedures(ModelBuilder modelBuilder)
    {
        // Configuration pour résultats de procédures stockées
        modelBuilder.Entity<CustomerStatsResult>().HasNoKey();
        modelBuilder.Entity<LoyaltyStatsResult>().HasNoKey();
        modelBuilder.Entity<SegmentStatsResult>().HasNoKey();
        modelBuilder.Entity<ChurnPredictionResult>().HasNoKey();
        modelBuilder.Entity<RecommendationResult>().HasNoKey();
    }

    /// <summary>
    /// Méthodes d'extension pour requêtes optimisées
    /// </summary>
    public async Task<List<CustomerStatsResult>> GetCustomerStatsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        return await Set<CustomerStatsResult>()
            .FromSqlRaw("EXEC sp_GetCustomerStats @FromDate = {0}, @ToDate = {1}", fromDate, toDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoyaltyStatsResult>> GetLoyaltyStatsAsync(
        Guid? programId = null, 
        CancellationToken cancellationToken = default)
    {
        return await Set<LoyaltyStatsResult>()
            .FromSqlRaw("EXEC sp_GetLoyaltyStats @ProgramId = {0}", programId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChurnPredictionResult>> GetChurnPredictionsAsync(
        int riskThreshold = 70,
        CancellationToken cancellationToken = default)
    {
        return await Set<ChurnPredictionResult>()
            .FromSqlRaw("EXEC sp_GetChurnPredictions @RiskThreshold = {0}", riskThreshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RecommendationResult>> GetRecommendationsAsync(
        Guid customerId,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        return await Set<RecommendationResult>()
            .FromSqlRaw("EXEC sp_GetCustomerRecommendations @CustomerId = {0}, @MaxResults = {1}", 
                customerId, maxResults)
            .ToListAsync(cancellationToken);
    }
}

// ===== ANALYTICS VIEW MODELS =====

public class CustomerAnalyticsView
{
    public Guid CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int? DaysSinceLastOrder { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public string LoyaltyTier { get; set; } = string.Empty;
    public decimal ChurnRiskScore { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public string PreferredChannel { get; set; } = string.Empty;
    public DateTime? LastInteractionDate { get; set; }
    public int TotalInteractions { get; set; }
    public bool IsVip { get; set; }
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string Gender { get; set; } = string.Empty;
}

public class LoyaltyAnalyticsView
{
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramType { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int BronzeMembers { get; set; }
    public int SilverMembers { get; set; }
    public int GoldMembers { get; set; }
    public int PlatinumMembers { get; set; }
    public int DiamondMembers { get; set; }
    public decimal TotalPointsIssued { get; set; }
    public decimal TotalPointsRedeemed { get; set; }
    public decimal AveragePointsPerMember { get; set; }
    public decimal RedemptionRate { get; set; }
    public decimal MemberGrowthRate { get; set; }
    public decimal AverageEngagementScore { get; set; }
}

public class SegmentAnalyticsView
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public string SegmentType { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal ChurnRate { get; set; }
    public decimal GrowthRate { get; set; }
    public decimal EngagementScore { get; set; }
    public decimal SatisfactionScore { get; set; }
}

public class InteractionAnalyticsView
{
    public string Period { get; set; } = string.Empty;
    public string InteractionType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public int TotalInteractions { get; set; }
    public int UniqueCustomers { get; set; }
    public decimal AverageDuration { get; set; }
    public decimal ResolutionRate { get; set; }
    public decimal AverageSatisfactionScore { get; set; }
    public decimal PositiveSentimentRate { get; set; }
    public decimal FirstContactResolutionRate { get; set; }
    public decimal EscalationRate { get; set; }
}

public class PreferenceAnalyticsView
{
    public string PreferenceType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public decimal AverageConfidence { get; set; }
    public decimal AverageUsageCount { get; set; }
    public decimal AverageSuccessRate { get; set; }
    public decimal AverageClickThroughRate { get; set; }
    public decimal AverageConversionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ExplicitPreferences { get; set; }
    public int InferredPreferences { get; set; }
}

// ===== STORED PROCEDURE RESULTS =====

public class CustomerStatsResult
{
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}

public class LoyaltyStatsResult
{
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class SegmentStatsResult
{
    public Guid SegmentId { get; set; }
    public string SegmentName { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class ChurnPredictionResult
{
    public Guid CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal ChurnProbability { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string PrimaryRiskFactors { get; set; } = string.Empty;
    public string RecommendedActions { get; set; } = string.Empty;
    public DateTime PredictionDate { get; set; }
}

public class RecommendationResult
{
    public Guid CustomerId { get; set; }
    public string RecommendationType { get; set; } = string.Empty;
    public string RecommendationValue { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public decimal ExpectedRevenue { get; set; }
    public DateTime GeneratedAt { get; set; }
}