using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.ValueObjects;
using NiesPro.Services.Customer.Domain.Enums;

namespace NiesPro.Services.Customer.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'entité CustomerProfile
/// Mapping avancé avec Value Objects et Relations sophistiquées
/// </summary>
public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("CustomerProfiles", "customer");
        builder.HasKey(cp => cp.Id);

        // ===== PRIMARY KEY =====
        builder.Property(cp => cp.Id)
            .HasColumnName("ProfileId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== FOREIGN KEY =====
        builder.Property(cp => cp.CustomerId)
            .IsRequired()
            .HasComment("Référence vers le client propriétaire");

        // ===== BASIC PROPERTIES =====
        builder.Property(cp => cp.ProfileCompleteness)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Pourcentage de complétude du profil (0-100)");

        builder.Property(cp => cp.CustomerLifetimeValue)
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0)
            .HasComment("Valeur vie client calculée");

        builder.Property(cp => cp.AverageOrderValue)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0)
            .HasComment("Panier moyen du client");

        builder.Property(cp => cp.OrderFrequency)
            .HasPrecision(8, 2)
            .HasDefaultValue(0)
            .HasComment("Fréquence de commande (commandes/mois)");

        builder.Property(cp => cp.LastOrderDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de la dernière commande");

        builder.Property(cp => cp.TotalOrders)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre total de commandes");

        builder.Property(cp => cp.TotalSpent)
            .HasColumnType("decimal(12,2)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Montant total dépensé");

        builder.Property(cp => cp.ChurnRiskScore)
            .HasPrecision(5, 2)
            .HasDefaultValue(0)
            .HasComment("Score de risque d'attrition (0-100)");

        builder.Property(cp => cp.SatisfactionScore)
            .HasPrecision(3, 1)
            .HasComment("Score de satisfaction moyen (1-5)");

        builder.Property(cp => cp.NpsScore)
            .HasComment("Net Promoter Score (-100 à +100)");

        builder.Property(cp => cp.PreferredContactTime)
            .HasMaxLength(50)
            .HasComment("Heure préférée de contact");

        builder.Property(cp => cp.Notes)
            .HasMaxLength(2000)
            .HasComment("Notes libres sur le client");

        builder.Property(cp => cp.Tags)
            .HasMaxLength(1000)
            .HasComment("Tags séparés par des virgules");

        builder.Property(cp => cp.IsVip)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Client VIP");

        builder.Property(cp => cp.ReferralCode)
            .HasMaxLength(20)
            .HasComment("Code de parrainage unique");

        builder.Property(cp => cp.SourceChannel)
            .HasMaxLength(100)
            .HasComment("Canal d'acquisition client");

        builder.Property(cp => cp.LastProfileUpdate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Dernière mise à jour du profil");

        // ===== VALUE OBJECTS CONFIGURATION =====
        ConfigureCommunicationSettings(builder);
        ConfigureRiskAssessment(builder);

        // ===== RELATIONSHIPS =====
        ConfigureRelationships(builder);

        // ===== INDEXES =====
        ConfigureIndexes(builder);

        // ===== CONSTRAINTS =====
        ConfigureConstraints(builder);
    }

    /// <summary>
    /// Configuration du Value Object CommunicationSettings
    /// </summary>
    private void ConfigureCommunicationSettings(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.OwnsOne(cp => cp.CommunicationSettings, cs =>
        {
            cs.Property(c => c.PreferredChannel)
                .HasColumnName("CommPreferredChannel")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue("Email")
                .HasComment("Canal de communication préféré");

            cs.Property(c => c.PreferredLanguage)
                .HasColumnName("CommPreferredLanguage")
                .HasMaxLength(10)
                .HasDefaultValue("fr-FR")
                .HasComment("Langue préférée pour les communications");

            cs.Property(c => c.EmailNotifications)
                .HasColumnName("CommEmailNotifications")
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Accepte les notifications par email");

            cs.Property(c => c.SmsNotifications)
                .HasColumnName("CommSmsNotifications")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Accepte les notifications par SMS");

            cs.Property(c => c.PushNotifications)
                .HasColumnName("CommPushNotifications")
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Accepte les notifications push");

            cs.Property(c => c.PhoneNotifications)
                .HasColumnName("CommPhoneNotifications")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Accepte les appels téléphoniques");

            cs.Property(c => c.NewsletterSubscription)
                .HasColumnName("CommNewsletterSubscription")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Abonné à la newsletter");

            cs.Property(c => c.PromotionalOffers)
                .HasColumnName("CommPromotionalOffers")
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Accepte les offres promotionnelles");

            cs.Property(c => c.SurveyParticipation)
                .HasColumnName("CommSurveyParticipation")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Accepte de participer aux enquêtes");

            cs.Property(c => c.EventInvitations)
                .HasColumnName("CommEventInvitations")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Accepte les invitations aux événements");

            cs.Property(c => c.BirthdayOffers)
                .HasColumnName("CommBirthdayOffers")
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Accepte les offres d'anniversaire");

            cs.Property(c => c.FrequencyPreference)
                .HasColumnName("CommFrequencyPreference")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue("Weekly")
                .HasComment("Fréquence préférée des communications");

            cs.Property(c => c.DoNotContactBefore)
                .HasColumnName("CommDoNotContactBefore")
                .HasColumnType("time")
                .HasComment("Ne pas contacter avant cette heure");

            cs.Property(c => c.DoNotContactAfter)
                .HasColumnName("CommDoNotContactAfter")
                .HasColumnType("time")
                .HasComment("Ne pas contacter après cette heure");

            cs.Property(c => c.PreferredDays)
                .HasColumnName("CommPreferredDays")
                .HasMaxLength(100)
                .HasComment("Jours préférés pour le contact (JSON)");

            cs.Property(c => c.OptOutDate)
                .HasColumnName("CommOptOutDate")
                .HasColumnType("datetime2(3)")
                .HasComment("Date de désabonnement général");

            cs.Property(c => c.LastContactDate)
                .HasColumnName("CommLastContactDate")
                .HasColumnType("datetime2(3)")
                .HasComment("Dernière date de contact");

            cs.Property(c => c.ContactAttempts)
                .HasColumnName("CommContactAttempts")
                .HasDefaultValue(0)
                .HasComment("Nombre de tentatives de contact");

            // Index pour recherche par préférences
            cs.HasIndex(c => c.PreferredChannel)
                .HasDatabaseName("IX_CustomerProfile_CommSettings_PreferredChannel");

            cs.HasIndex(c => new { c.EmailNotifications, c.NewsletterSubscription })
                .HasDatabaseName("IX_CustomerProfile_CommSettings_EmailPrefs");
        });
    }

    /// <summary>
    /// Configuration du Value Object RiskAssessment
    /// </summary>
    private void ConfigureRiskAssessment(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.OwnsOne(cp => cp.RiskAssessment, ra =>
        {
            ra.Property(r => r.CreditScore)
                .HasColumnName("RiskCreditScore")
                .HasComment("Score de crédit du client");

            ra.Property(r => r.PaymentBehaviorScore)
                .HasColumnName("RiskPaymentBehaviorScore")
                .HasPrecision(5, 2)
                .HasDefaultValue(0)
                .HasComment("Score de comportement de paiement (0-100)");

            ra.Property(r => r.FraudRiskScore)
                .HasColumnName("RiskFraudRiskScore")
                .HasPrecision(5, 2)
                .HasDefaultValue(0)
                .HasComment("Score de risque de fraude (0-100)");

            ra.Property(r => r.ChurnProbability)
                .HasColumnName("RiskChurnProbability")
                .HasPrecision(5, 4)
                .HasDefaultValue(0)
                .HasComment("Probabilité d'attrition (0-1)");

            ra.Property(r => r.RiskLevel)
                .HasColumnName("RiskLevel")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue("Low")
                .HasComment("Niveau de risque global");

            ra.Property(r => r.LastRiskUpdate)
                .HasColumnName("RiskLastUpdate")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Dernière mise à jour de l'évaluation");

            ra.Property(r => r.RiskFactors)
                .HasColumnName("RiskFactors")
                .HasMaxLength(1000)
                .HasComment("Facteurs de risque identifiés (JSON)");

            ra.Property(r => r.MitigationActions)
                .HasColumnName("RiskMitigationActions")
                .HasMaxLength(2000)
                .HasComment("Actions de mitigation recommandées");

            ra.Property(r => r.BlacklistStatus)
                .HasColumnName("RiskBlacklistStatus")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Client en liste noire");

            ra.Property(r => r.WatchlistStatus)
                .HasColumnName("RiskWatchlistStatus")
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Client en liste de surveillance");

            // Index pour surveillance des risques
            ra.HasIndex(r => r.RiskLevel)
                .HasDatabaseName("IX_CustomerProfile_RiskAssessment_RiskLevel");

            ra.HasIndex(r => new { r.BlacklistStatus, r.WatchlistStatus })
                .HasDatabaseName("IX_CustomerProfile_RiskAssessment_Lists");

            ra.HasIndex(r => r.ChurnProbability)
                .HasDatabaseName("IX_CustomerProfile_RiskAssessment_ChurnProbability")
                .IsDescending();
        });
    }

    /// <summary>
    /// Configuration des relations
    /// </summary>
    private void ConfigureRelationships(EntityTypeBuilder<CustomerProfile> builder)
    {
        // Relation avec Customer (1:1)
        builder.HasOne<Domain.Entities.Customer>()
            .WithOne()
            .HasForeignKey<CustomerProfile>(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerProfile_Customer");

        // Index pour la foreign key
        builder.HasIndex(cp => cp.CustomerId)
            .IsUnique()
            .HasDatabaseName("IX_CustomerProfile_CustomerId_Unique");
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<CustomerProfile> builder)
    {
        // Index pour VIP
        builder.HasIndex(cp => cp.IsVip)
            .HasDatabaseName("IX_CustomerProfile_IsVip")
            .HasFilter("[IsVip] = 1");

        // Index pour CLV
        builder.HasIndex(cp => cp.CustomerLifetimeValue)
            .HasDatabaseName("IX_CustomerProfile_CLV")
            .IsDescending()
            .HasFilter("[CustomerLifetimeValue] > 0");

        // Index pour churn risk
        builder.HasIndex(cp => cp.ChurnRiskScore)
            .HasDatabaseName("IX_CustomerProfile_ChurnRisk")
            .IsDescending()
            .HasFilter("[ChurnRiskScore] > 0");

        // Index pour dernière commande
        builder.HasIndex(cp => cp.LastOrderDate)
            .HasDatabaseName("IX_CustomerProfile_LastOrderDate")
            .IsDescending()
            .HasFilter("[LastOrderDate] IS NOT NULL");

        // Index pour code de parrainage
        builder.HasIndex(cp => cp.ReferralCode)
            .IsUnique()
            .HasDatabaseName("IX_CustomerProfile_ReferralCode_Unique")
            .HasFilter("[ReferralCode] IS NOT NULL");

        // Index composé pour analytics
        builder.HasIndex(cp => new { cp.IsVip, cp.CustomerLifetimeValue, cp.ChurnRiskScore })
            .HasDatabaseName("IX_CustomerProfile_Analytics_Composite");

        // Index pour profil complet
        builder.HasIndex(cp => cp.ProfileCompleteness)
            .HasDatabaseName("IX_CustomerProfile_Completeness")
            .IsDescending();

        // Index pour satisfaction
        builder.HasIndex(cp => cp.SatisfactionScore)
            .HasDatabaseName("IX_CustomerProfile_SatisfactionScore")
            .IsDescending()
            .HasFilter("[SatisfactionScore] IS NOT NULL");

        // Index covering pour dashboard
        builder.HasIndex(cp => new { cp.IsVip, cp.LastOrderDate })
            .HasDatabaseName("IX_CustomerProfile_Dashboard_Covering")
            .IncludeProperties(cp => new { 
                cp.CustomerLifetimeValue, 
                cp.TotalOrders, 
                cp.TotalSpent,
                cp.ChurnRiskScore 
            });
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<CustomerProfile> builder)
    {
        // Contrainte pour ProfileCompleteness
        builder.HasCheckConstraint(
            "CK_CustomerProfile_ProfileCompleteness",
            "[ProfileCompleteness] >= 0 AND [ProfileCompleteness] <= 100");

        // Contrainte pour ChurnRiskScore
        builder.HasCheckConstraint(
            "CK_CustomerProfile_ChurnRiskScore",
            "[ChurnRiskScore] >= 0 AND [ChurnRiskScore] <= 100");

        // Contrainte pour SatisfactionScore
        builder.HasCheckConstraint(
            "CK_CustomerProfile_SatisfactionScore",
            "[SatisfactionScore] IS NULL OR ([SatisfactionScore] >= 1 AND [SatisfactionScore] <= 5)");

        // Contrainte pour NpsScore
        builder.HasCheckConstraint(
            "CK_CustomerProfile_NpsScore",
            "[NpsScore] IS NULL OR ([NpsScore] >= -100 AND [NpsScore] <= 100)");

        // Contrainte pour valeurs positives
        builder.HasCheckConstraint(
            "CK_CustomerProfile_PositiveValues",
            "[TotalOrders] >= 0 AND [TotalSpent] >= 0 AND [CustomerLifetimeValue] >= 0");

        // Contrainte pour LastOrderDate
        builder.HasCheckConstraint(
            "CK_CustomerProfile_LastOrderDate",
            "[LastOrderDate] IS NULL OR [LastOrderDate] <= GETUTCDATE()");

        // Contrainte pour dates cohérentes
        builder.HasCheckConstraint(
            "CK_CustomerProfile_Dates",
            "[LastProfileUpdate] <= GETUTCDATE()");
    }
}