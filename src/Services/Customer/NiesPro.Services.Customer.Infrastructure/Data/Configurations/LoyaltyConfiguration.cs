using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.Enums;

namespace NiesPro.Services.Customer.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'entité LoyaltyProgram
/// Mapping sophistiqué pour la gestion des programmes de fidélité
/// </summary>
public class LoyaltyProgramConfiguration : IEntityTypeConfiguration<LoyaltyProgram>
{
    public void Configure(EntityTypeBuilder<LoyaltyProgram> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("LoyaltyPrograms", "customer");
        builder.HasKey(lp => lp.Id);

        // ===== PRIMARY KEY =====
        builder.Property(lp => lp.Id)
            .HasColumnName("ProgramId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== BASIC PROPERTIES =====
        builder.Property(lp => lp.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nom du programme de fidélité");

        builder.Property(lp => lp.Description)
            .HasMaxLength(2000)
            .HasComment("Description détaillée du programme");

        builder.Property(lp => lp.ProgramType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Type de programme (Points, Tiers, Cashback, etc.)");

        builder.Property(lp => lp.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Programme actif");

        builder.Property(lp => lp.StartDate)
            .HasColumnType("datetime2(3)")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de début du programme");

        builder.Property(lp => lp.EndDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de fin du programme (null = permanent)");

        builder.Property(lp => lp.MaxMembers)
            .HasComment("Nombre maximum de membres (null = illimité)");

        builder.Property(lp => lp.CurrentMembers)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre actuel de membres");

        builder.Property(lp => lp.MinimumAge)
            .HasComment("Âge minimum requis");

        builder.Property(lp => lp.MaximumAge)
            .HasComment("Âge maximum autorisé");

        builder.Property(lp => lp.RequiredSpending)
            .HasColumnType("decimal(12,2)")
            .HasComment("Montant de dépense requis pour adhérer");

        builder.Property(lp => lp.GeographicRestrictions)
            .HasMaxLength(1000)
            .HasComment("Restrictions géographiques (JSON)");

        // ===== POINT SYSTEM CONFIGURATION =====
        builder.Property(lp => lp.PointsPerEuro)
            .HasColumnType("decimal(8,4)")
            .HasDefaultValue(1.0000m)
            .HasComment("Points gagnés par euro dépensé");

        builder.Property(lp => lp.EuroPerPoint)
            .HasColumnType("decimal(8,4)")
            .HasDefaultValue(0.0100m)
            .HasComment("Valeur en euros d'un point");

        builder.Property(lp => lp.MinimumPointsForRedemption)
            .HasDefaultValue(100)
            .HasComment("Points minimum pour utilisation");

        builder.Property(lp => lp.PointsExpiryMonths)
            .HasComment("Durée d'expiration des points en mois (null = jamais)");

        builder.Property(lp => lp.BonusMultiplier)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(1.00m)
            .HasComment("Multiplicateur de bonus pour événements spéciaux");

        builder.Property(lp => lp.WelcomeBonus)
            .HasDefaultValue(0)
            .HasComment("Points bonus à l'inscription");

        builder.Property(lp => lp.BirthdayBonus)
            .HasDefaultValue(0)
            .HasComment("Points bonus d'anniversaire");

        builder.Property(lp => lp.ReferralBonus)
            .HasDefaultValue(0)
            .HasComment("Points bonus de parrainage");

        // ===== TIER SYSTEM CONFIGURATION =====
        builder.Property(lp => lp.TierSystemEnabled)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Système de niveaux activé");

        builder.Property(lp => lp.BronzeTierThreshold)
            .HasDefaultValue(0)
            .HasComment("Seuil pour niveau Bronze");

        builder.Property(lp => lp.SilverTierThreshold)
            .HasDefaultValue(1000)
            .HasComment("Seuil pour niveau Silver");

        builder.Property(lp => lp.GoldTierThreshold)
            .HasDefaultValue(5000)
            .HasComment("Seuil pour niveau Gold");

        builder.Property(lp => lp.PlatinumTierThreshold)
            .HasDefaultValue(15000)
            .HasComment("Seuil pour niveau Platinum");

        builder.Property(lp => lp.DiamondTierThreshold)
            .HasDefaultValue(50000)
            .HasComment("Seuil pour niveau Diamond");

        builder.Property(lp => lp.TierEvaluationPeriod)
            .HasDefaultValue(12)
            .HasComment("Période d'évaluation des niveaux en mois");

        // ===== BUSINESS RULES =====
        builder.Property(lp => lp.AllowPointSharing)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Autoriser le partage de points");

        builder.Property(lp => lp.AllowPointTransfer)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Autoriser le transfert de points");

        builder.Property(lp => lp.AutoEnrollment)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Inscription automatique des nouveaux clients");

        builder.Property(lp => lp.RequireOptIn)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Require explicit opt-in");

        builder.Property(lp => lp.SendWelcomeEmail)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Envoyer email de bienvenue");

        builder.Property(lp => lp.SendPointsNotifications)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Envoyer notifications de points");

        builder.Property(lp => lp.SendTierNotifications)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Envoyer notifications de niveau");

        builder.Property(lp => lp.SendExpiryWarnings)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Envoyer alertes d'expiration");

        // ===== METADATA =====
        builder.Property(lp => lp.CreatedBy)
            .HasMaxLength(255)
            .HasComment("Créé par");

        builder.Property(lp => lp.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de création");

        builder.Property(lp => lp.LastModifiedBy)
            .HasMaxLength(255)
            .HasComment("Modifié par");

        builder.Property(lp => lp.LastModifiedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de modification");

        builder.Property(lp => lp.Version)
            .IsRowVersion()
            .HasComment("Version pour concurrence optimiste");

        // ===== RELATIONSHIPS =====
        ConfigureRelationships(builder);

        // ===== INDEXES =====
        ConfigureIndexes(builder);

        // ===== CONSTRAINTS =====
        ConfigureConstraints(builder);
    }

    /// <summary>
    /// Configuration des relations
    /// </summary>
    private void ConfigureRelationships(EntityTypeBuilder<LoyaltyProgram> builder)
    {
        // Relation 1:N avec LoyaltyRewards
        builder.HasMany<LoyaltyReward>()
            .WithOne()
            .HasForeignKey(lr => lr.ProgramId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_LoyaltyReward_LoyaltyProgram");
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<LoyaltyProgram> builder)
    {
        // Index unique pour nom
        builder.HasIndex(lp => lp.Name)
            .IsUnique()
            .HasDatabaseName("IX_LoyaltyProgram_Name_Unique")
            .HasFilter("[Name] IS NOT NULL");

        // Index pour type et statut
        builder.HasIndex(lp => new { lp.ProgramType, lp.IsActive })
            .HasDatabaseName("IX_LoyaltyProgram_Type_Active");

        // Index pour dates
        builder.HasIndex(lp => new { lp.StartDate, lp.EndDate })
            .HasDatabaseName("IX_LoyaltyProgram_DateRange");

        // Index pour statut actif
        builder.HasIndex(lp => lp.IsActive)
            .HasDatabaseName("IX_LoyaltyProgram_IsActive")
            .HasFilter("[IsActive] = 1");

        // Index pour auto-enrollment
        builder.HasIndex(lp => lp.AutoEnrollment)
            .HasDatabaseName("IX_LoyaltyProgram_AutoEnrollment")
            .HasFilter("[AutoEnrollment] = 1 AND [IsActive] = 1");

        // Index pour système de niveaux
        builder.HasIndex(lp => lp.TierSystemEnabled)
            .HasDatabaseName("IX_LoyaltyProgram_TierSystem")
            .HasFilter("[TierSystemEnabled] = 1 AND [IsActive] = 1");

        // Index composé pour recherche
        builder.HasIndex(lp => new { lp.IsActive, lp.ProgramType, lp.StartDate })
            .HasDatabaseName("IX_LoyaltyProgram_Search_Composite");

        // Index covering pour dashboard
        builder.HasIndex(lp => new { lp.IsActive, lp.CreatedDate })
            .HasDatabaseName("IX_LoyaltyProgram_Dashboard_Covering")
            .IncludeProperties(lp => new { 
                lp.Name, 
                lp.ProgramType, 
                lp.CurrentMembers,
                lp.MaxMembers 
            });
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<LoyaltyProgram> builder)
    {
        // Contrainte pour dates cohérentes
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_Dates",
            "[EndDate] IS NULL OR [EndDate] > [StartDate]");

        // Contrainte pour membres
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_Members",
            "[CurrentMembers] >= 0 AND ([MaxMembers] IS NULL OR [CurrentMembers] <= [MaxMembers])");

        // Contrainte pour âges
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_Ages",
            "[MinimumAge] IS NULL OR [MinimumAge] >= 0");

        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_AgeRange",
            "[MaximumAge] IS NULL OR [MinimumAge] IS NULL OR [MaximumAge] > [MinimumAge]");

        // Contrainte pour montants positifs
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_PositiveAmounts",
            "[RequiredSpending] IS NULL OR [RequiredSpending] >= 0");

        // Contrainte pour système de points
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_PointsRates",
            "[PointsPerEuro] > 0 AND [EuroPerPoint] > 0");

        // Contrainte pour seuils de niveaux croissants
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_TierThresholds",
            "[SilverTierThreshold] >= [BronzeTierThreshold] AND " +
            "[GoldTierThreshold] >= [SilverTierThreshold] AND " +
            "[PlatinumTierThreshold] >= [GoldTierThreshold] AND " +
            "[DiamondTierThreshold] >= [PlatinumTierThreshold]");

        // Contrainte pour bonus positifs
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_BonusValues",
            "[WelcomeBonus] >= 0 AND [BirthdayBonus] >= 0 AND [ReferralBonus] >= 0");

        // Contrainte pour multiplicateur de bonus
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_BonusMultiplier",
            "[BonusMultiplier] > 0");

        // Contrainte pour points minimum
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_MinPoints",
            "[MinimumPointsForRedemption] >= 0");

        // Contrainte pour période d'évaluation
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_EvaluationPeriod",
            "[TierEvaluationPeriod] > 0");

        // Contrainte pour expiration des points
        builder.HasCheckConstraint(
            "CK_LoyaltyProgram_PointsExpiry",
            "[PointsExpiryMonths] IS NULL OR [PointsExpiryMonths] > 0");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité LoyaltyReward
/// Mapping sophistiqué pour la gestion des récompenses de fidélité
/// </summary>
public class LoyaltyRewardConfiguration : IEntityTypeConfiguration<LoyaltyReward>
{
    public void Configure(EntityTypeBuilder<LoyaltyReward> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("LoyaltyRewards", "customer");
        builder.HasKey(lr => lr.Id);

        // ===== PRIMARY KEY =====
        builder.Property(lr => lr.Id)
            .HasColumnName("RewardId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== FOREIGN KEY =====
        builder.Property(lr => lr.ProgramId)
            .IsRequired()
            .HasComment("Référence vers le programme de fidélité");

        // ===== BASIC PROPERTIES =====
        builder.Property(lr => lr.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nom de la récompense");

        builder.Property(lr => lr.Description)
            .HasMaxLength(2000)
            .HasComment("Description détaillée de la récompense");

        builder.Property(lr => lr.RewardType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Type de récompense (Discount, Product, Service, etc.)");

        builder.Property(lr => lr.PointsCost)
            .IsRequired()
            .HasComment("Coût en points de la récompense");

        builder.Property(lr => lr.MonetaryValue)
            .HasColumnType("decimal(10,2)")
            .HasComment("Valeur monétaire de la récompense");

        builder.Property(lr => lr.DiscountPercentage)
            .HasColumnType("decimal(5,2)")
            .HasComment("Pourcentage de réduction (si applicable)");

        builder.Property(lr => lr.FixedDiscount)
            .HasColumnType("decimal(10,2)")
            .HasComment("Montant fixe de réduction (si applicable)");

        builder.Property(lr => lr.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Récompense active");

        builder.Property(lr => lr.StartDate)
            .HasColumnType("datetime2(3)")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de début de disponibilité");

        builder.Property(lr => lr.EndDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de fin de disponibilité");

        builder.Property(lr => lr.MaxRedemptions)
            .HasComment("Nombre maximum d'utilisations (null = illimité)");

        builder.Property(lr => lr.CurrentRedemptions)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre actuel d'utilisations");

        builder.Property(lr => lr.MaxRedemptionsPerCustomer)
            .HasDefaultValue(1)
            .HasComment("Utilisations maximum par client");

        builder.Property(lr => lr.MinimumPurchaseAmount)
            .HasColumnType("decimal(10,2)")
            .HasComment("Montant minimum d'achat requis");

        builder.Property(lr => lr.RequiredTier)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Niveau de fidélité requis");

        builder.Property(lr => lr.IsLimitedTime)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Offre à durée limitée");

        builder.Property(lr => lr.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Récompense mise en avant");

        builder.Property(lr => lr.CategoryRestrictions)
            .HasMaxLength(1000)
            .HasComment("Restrictions par catégorie (JSON)");

        builder.Property(lr => lr.ProductRestrictions)
            .HasMaxLength(2000)
            .HasComment("Restrictions par produit (JSON)");

        builder.Property(lr => lr.GeographicRestrictions)
            .HasMaxLength(1000)
            .HasComment("Restrictions géographiques (JSON)");

        builder.Property(lr => lr.TermsAndConditions)
            .HasMaxLength(4000)
            .HasComment("Conditions générales d'utilisation");

        builder.Property(lr => lr.ImageUrl)
            .HasMaxLength(500)
            .HasComment("URL de l'image de la récompense");

        builder.Property(lr => lr.SortOrder)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Ordre d'affichage");

        // ===== METADATA =====
        builder.Property(lr => lr.CreatedBy)
            .HasMaxLength(255)
            .HasComment("Créé par");

        builder.Property(lr => lr.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de création");

        builder.Property(lr => lr.LastModifiedBy)
            .HasMaxLength(255)
            .HasComment("Modifié par");

        builder.Property(lr => lr.LastModifiedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de modification");

        builder.Property(lr => lr.Version)
            .IsRowVersion()
            .HasComment("Version pour concurrence optimiste");

        // ===== RELATIONSHIPS =====
        ConfigureRelationships(builder);

        // ===== INDEXES =====
        ConfigureIndexes(builder);

        // ===== CONSTRAINTS =====
        ConfigureConstraints(builder);
    }

    /// <summary>
    /// Configuration des relations
    /// </summary>
    private void ConfigureRelationships(EntityTypeBuilder<LoyaltyReward> builder)
    {
        // Relation avec LoyaltyProgram
        builder.HasOne<LoyaltyProgram>()
            .WithMany()
            .HasForeignKey(lr => lr.ProgramId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_LoyaltyReward_LoyaltyProgram");

        // Index pour la foreign key
        builder.HasIndex(lr => lr.ProgramId)
            .HasDatabaseName("IX_LoyaltyReward_ProgramId");
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<LoyaltyReward> builder)
    {
        // Index composé pour programme et statut
        builder.HasIndex(lr => new { lr.ProgramId, lr.IsActive })
            .HasDatabaseName("IX_LoyaltyReward_Program_Active");

        // Index pour type de récompense
        builder.HasIndex(lr => lr.RewardType)
            .HasDatabaseName("IX_LoyaltyReward_Type");

        // Index pour coût en points
        builder.HasIndex(lr => lr.PointsCost)
            .HasDatabaseName("IX_LoyaltyReward_PointsCost");

        // Index pour statut featured
        builder.HasIndex(lr => lr.IsFeatured)
            .HasDatabaseName("IX_LoyaltyReward_Featured")
            .HasFilter("[IsFeatured] = 1 AND [IsActive] = 1");

        // Index pour ordre d'affichage
        builder.HasIndex(lr => new { lr.ProgramId, lr.SortOrder })
            .HasDatabaseName("IX_LoyaltyReward_Program_SortOrder");

        // Index pour dates de validité
        builder.HasIndex(lr => new { lr.StartDate, lr.EndDate })
            .HasDatabaseName("IX_LoyaltyReward_DateRange");

        // Index pour niveau requis
        builder.HasIndex(lr => lr.RequiredTier)
            .HasDatabaseName("IX_LoyaltyReward_RequiredTier")
            .HasFilter("[RequiredTier] IS NOT NULL");

        // Index composé pour recherche
        builder.HasIndex(lr => new { lr.ProgramId, lr.IsActive, lr.RewardType, lr.PointsCost })
            .HasDatabaseName("IX_LoyaltyReward_Search_Composite");

        // Index covering pour affichage catalogue
        builder.HasIndex(lr => new { lr.ProgramId, lr.IsActive, lr.SortOrder })
            .HasDatabaseName("IX_LoyaltyReward_Catalog_Covering")
            .IncludeProperties(lr => new { 
                lr.Name, 
                lr.PointsCost, 
                lr.RewardType,
                lr.IsFeatured,
                lr.ImageUrl 
            });
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<LoyaltyReward> builder)
    {
        // Contrainte pour dates cohérentes
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_Dates",
            "[EndDate] IS NULL OR [EndDate] > [StartDate]");

        // Contrainte pour coût en points positif
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_PointsCost",
            "[PointsCost] > 0");

        // Contrainte pour valeurs positives
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_PositiveValues",
            "([MonetaryValue] IS NULL OR [MonetaryValue] >= 0) AND " +
            "([FixedDiscount] IS NULL OR [FixedDiscount] >= 0) AND " +
            "([MinimumPurchaseAmount] IS NULL OR [MinimumPurchaseAmount] >= 0)");

        // Contrainte pour pourcentage de réduction
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_DiscountPercentage",
            "[DiscountPercentage] IS NULL OR ([DiscountPercentage] >= 0 AND [DiscountPercentage] <= 100)");

        // Contrainte pour utilisations
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_Redemptions",
            "[CurrentRedemptions] >= 0 AND " +
            "([MaxRedemptions] IS NULL OR [CurrentRedemptions] <= [MaxRedemptions]) AND " +
            "[MaxRedemptionsPerCustomer] > 0");

        // Contrainte pour ordre d'affichage
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_SortOrder",
            "[SortOrder] >= 0");

        // Contrainte pour cohérence des réductions
        builder.HasCheckConstraint(
            "CK_LoyaltyReward_DiscountCoherence",
            "([DiscountPercentage] IS NULL AND [FixedDiscount] IS NULL) OR " +
            "([DiscountPercentage] IS NOT NULL AND [FixedDiscount] IS NULL) OR " +
            "([DiscountPercentage] IS NULL AND [FixedDiscount] IS NOT NULL)");
    }
}