using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.Enums;

namespace NiesPro.Services.Customer.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'entité CustomerSegment
/// Mapping sophistiqué pour la segmentation automatique des clients
/// </summary>
public class CustomerSegmentConfiguration : IEntityTypeConfiguration<CustomerSegment>
{
    public void Configure(EntityTypeBuilder<CustomerSegment> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("CustomerSegments", "customer");
        builder.HasKey(cs => cs.Id);

        // ===== PRIMARY KEY =====
        builder.Property(cs => cs.Id)
            .HasColumnName("SegmentId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== BASIC PROPERTIES =====
        builder.Property(cs => cs.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nom du segment de clientèle");

        builder.Property(cs => cs.Description)
            .HasMaxLength(2000)
            .HasComment("Description détaillée du segment");

        builder.Property(cs => cs.SegmentType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Type de segment (Demographic, Behavioral, Geographic, etc.)");

        builder.Property(cs => cs.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Segment actif");

        builder.Property(cs => cs.IsAutomatic)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Assignation automatique basée sur les critères");

        builder.Property(cs => cs.Priority)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Priorité d'évaluation (plus élevé = prioritaire)");

        builder.Property(cs => cs.Color)
            .HasMaxLength(7)
            .HasComment("Couleur d'affichage (format hex)");

        builder.Property(cs => cs.Icon)
            .HasMaxLength(50)
            .HasComment("Icône d'affichage");

        // ===== CRITERIA CONFIGURATION =====
        builder.Property(cs => cs.Criteria)
            .HasMaxLength(4000)
            .HasComment("Critères d'assignation (JSON)");

        builder.Property(cs => cs.MinAge)
            .HasComment("Âge minimum");

        builder.Property(cs => cs.MaxAge)
            .HasComment("Âge maximum");

        builder.Property(cs => cs.MinSpending)
            .HasColumnType("decimal(12,2)")
            .HasComment("Dépense minimum");

        builder.Property(cs => cs.MaxSpending)
            .HasColumnType("decimal(12,2)")
            .HasComment("Dépense maximum");

        builder.Property(cs => cs.MinOrders)
            .HasComment("Nombre minimum de commandes");

        builder.Property(cs => cs.MaxOrders)
            .HasComment("Nombre maximum de commandes");

        builder.Property(cs => cs.MinRegistrationDays)
            .HasComment("Nombre minimum de jours depuis l'inscription");

        builder.Property(cs => cs.MaxRegistrationDays)
            .HasComment("Nombre maximum de jours depuis l'inscription");

        builder.Property(cs => cs.MinLastOrderDays)
            .HasComment("Nombre minimum de jours depuis la dernière commande");

        builder.Property(cs => cs.MaxLastOrderDays)
            .HasComment("Nombre maximum de jours depuis la dernière commande");

        builder.Property(cs => cs.RequiredGender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Genre requis");

        builder.Property(cs => cs.RequiredCountries)
            .HasMaxLength(1000)
            .HasComment("Pays requis (JSON array)");

        builder.Property(cs => cs.RequiredCities)
            .HasMaxLength(2000)
            .HasComment("Villes requises (JSON array)");

        builder.Property(cs => cs.RequiredLoyaltyTiers)
            .HasMaxLength(500)
            .HasComment("Niveaux de fidélité requis (JSON array)");

        builder.Property(cs => cs.ExcludedCustomerStatuses)
            .HasMaxLength(500)
            .HasComment("Statuts clients exclus (JSON array)");

        // ===== MARKETING CONFIGURATION =====
        builder.Property(cs => cs.MarketingCampaignId)
            .HasComment("ID de campagne marketing associée");

        builder.Property(cs => cs.DefaultDiscountPercentage)
            .HasColumnType("decimal(5,2)")
            .HasComment("Pourcentage de remise par défaut");

        builder.Property(cs => cs.PreferredCommunicationChannel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Canal de communication préféré");

        builder.Property(cs => cs.CommunicationFrequency)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Fréquence de communication");

        builder.Property(cs => cs.PersonalizationLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue("Medium")
            .HasComment("Niveau de personnalisation");

        // ===== STATISTICS =====
        builder.Property(cs => cs.CurrentMemberCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre actuel de membres");

        builder.Property(cs => cs.LastEvaluationDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Dernière évaluation automatique");

        builder.Property(cs => cs.NextEvaluationDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Prochaine évaluation automatique");

        builder.Property(cs => cs.EvaluationFrequencyDays)
            .HasDefaultValue(7)
            .HasComment("Fréquence d'évaluation en jours");

        builder.Property(cs => cs.AverageCustomerValue)
            .HasColumnType("decimal(12,2)")
            .HasComment("Valeur moyenne des clients du segment");

        builder.Property(cs => cs.ChurnRate)
            .HasColumnType("decimal(5,4)")
            .HasComment("Taux d'attrition du segment");

        builder.Property(cs => cs.ConversionRate)
            .HasColumnType("decimal(5,4)")
            .HasComment("Taux de conversion du segment");

        // ===== METADATA =====
        builder.Property(cs => cs.CreatedBy)
            .HasMaxLength(255)
            .HasComment("Créé par");

        builder.Property(cs => cs.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de création");

        builder.Property(cs => cs.LastModifiedBy)
            .HasMaxLength(255)
            .HasComment("Modifié par");

        builder.Property(cs => cs.LastModifiedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de modification");

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
    private void ConfigureRelationships(EntityTypeBuilder<CustomerSegment> builder)
    {
        // Relation N:N avec Customer via table de liaison configurée dans CustomerConfiguration
        // Pas de configuration additionnelle nécessaire ici
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<CustomerSegment> builder)
    {
        // Index unique pour nom
        builder.HasIndex(cs => cs.Name)
            .IsUnique()
            .HasDatabaseName("IX_CustomerSegment_Name_Unique")
            .HasFilter("[Name] IS NOT NULL");

        // Index pour type et statut
        builder.HasIndex(cs => new { cs.SegmentType, cs.IsActive })
            .HasDatabaseName("IX_CustomerSegment_Type_Active");

        // Index pour segments automatiques
        builder.HasIndex(cs => cs.IsAutomatic)
            .HasDatabaseName("IX_CustomerSegment_IsAutomatic")
            .HasFilter("[IsAutomatic] = 1 AND [IsActive] = 1");

        // Index pour priorité
        builder.HasIndex(cs => cs.Priority)
            .HasDatabaseName("IX_CustomerSegment_Priority")
            .IsDescending();

        // Index pour évaluations
        builder.HasIndex(cs => cs.NextEvaluationDate)
            .HasDatabaseName("IX_CustomerSegment_NextEvaluation")
            .HasFilter("[NextEvaluationDate] IS NOT NULL AND [IsAutomatic] = 1");

        // Index pour statistiques
        builder.HasIndex(cs => cs.CurrentMemberCount)
            .HasDatabaseName("IX_CustomerSegment_MemberCount")
            .IsDescending();

        // Index composé pour recherche
        builder.HasIndex(cs => new { cs.IsActive, cs.SegmentType, cs.Priority })
            .HasDatabaseName("IX_CustomerSegment_Search_Composite");

        // Index covering pour dashboard
        builder.HasIndex(cs => new { cs.IsActive, cs.CreatedDate })
            .HasDatabaseName("IX_CustomerSegment_Dashboard_Covering")
            .IncludeProperties(cs => new { 
                cs.Name, 
                cs.SegmentType, 
                cs.CurrentMemberCount,
                cs.AverageCustomerValue,
                cs.ChurnRate 
            });
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<CustomerSegment> builder)
    {
        // Contrainte pour âges cohérents
        builder.HasCheckConstraint(
            "CK_CustomerSegment_Ages",
            "[MaxAge] IS NULL OR [MinAge] IS NULL OR [MaxAge] >= [MinAge]");

        // Contrainte pour dépenses cohérentes
        builder.HasCheckConstraint(
            "CK_CustomerSegment_Spending",
            "[MaxSpending] IS NULL OR [MinSpending] IS NULL OR [MaxSpending] >= [MinSpending]");

        // Contrainte pour commandes cohérentes
        builder.HasCheckConstraint(
            "CK_CustomerSegment_Orders",
            "[MaxOrders] IS NULL OR [MinOrders] IS NULL OR [MaxOrders] >= [MinOrders]");

        // Contrainte pour jours cohérents
        builder.HasCheckConstraint(
            "CK_CustomerSegment_RegistrationDays",
            "[MaxRegistrationDays] IS NULL OR [MinRegistrationDays] IS NULL OR [MaxRegistrationDays] >= [MinRegistrationDays]");

        builder.HasCheckConstraint(
            "CK_CustomerSegment_LastOrderDays",
            "[MaxLastOrderDays] IS NULL OR [MinLastOrderDays] IS NULL OR [MaxLastOrderDays] >= [MinLastOrderDays]");

        // Contrainte pour valeurs positives
        builder.HasCheckConstraint(
            "CK_CustomerSegment_PositiveValues",
            "[CurrentMemberCount] >= 0 AND " +
            "([MinAge] IS NULL OR [MinAge] >= 0) AND " +
            "([MinSpending] IS NULL OR [MinSpending] >= 0) AND " +
            "([MinOrders] IS NULL OR [MinOrders] >= 0) AND " +
            "([EvaluationFrequencyDays] > 0)");

        // Contrainte pour pourcentages
        builder.HasCheckConstraint(
            "CK_CustomerSegment_Percentages",
            "([DefaultDiscountPercentage] IS NULL OR ([DefaultDiscountPercentage] >= 0 AND [DefaultDiscountPercentage] <= 100)) AND " +
            "([ChurnRate] IS NULL OR ([ChurnRate] >= 0 AND [ChurnRate] <= 1)) AND " +
            "([ConversionRate] IS NULL OR ([ConversionRate] >= 0 AND [ConversionRate] <= 1))");

        // Contrainte pour couleur hex
        builder.HasCheckConstraint(
            "CK_CustomerSegment_Color",
            "[Color] IS NULL OR ([Color] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité CustomerInteraction
/// Mapping sophistiqué pour le suivi des interactions client
/// </summary>
public class CustomerInteractionConfiguration : IEntityTypeConfiguration<CustomerInteraction>
{
    public void Configure(EntityTypeBuilder<CustomerInteraction> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("CustomerInteractions", "customer");
        builder.HasKey(ci => ci.Id);

        // ===== PRIMARY KEY =====
        builder.Property(ci => ci.Id)
            .HasColumnName("InteractionId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== FOREIGN KEY =====
        builder.Property(ci => ci.CustomerId)
            .IsRequired()
            .HasComment("Référence vers le client");

        // ===== BASIC PROPERTIES =====
        builder.Property(ci => ci.InteractionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Type d'interaction (Email, Phone, Chat, Visit, etc.)");

        builder.Property(ci => ci.Channel)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Canal d'interaction (Website, Mobile, Store, Call Center, etc.)");

        builder.Property(ci => ci.Direction)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Direction (Inbound, Outbound)");

        builder.Property(ci => ci.Subject)
            .HasMaxLength(500)
            .HasComment("Sujet ou titre de l'interaction");

        builder.Property(ci => ci.Description)
            .HasMaxLength(4000)
            .HasComment("Description détaillée de l'interaction");

        builder.Property(ci => ci.InteractionDate)
            .HasColumnType("datetime2(3)")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date et heure de l'interaction");

        builder.Property(ci => ci.Duration)
            .HasComment("Durée en minutes");

        builder.Property(ci => ci.Outcome)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("Résultat de l'interaction (Resolved, Pending, Escalated, etc.)");

        builder.Property(ci => ci.Sentiment)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Sentiment détecté (Positive, Negative, Neutral)");

        builder.Property(ci => ci.SatisfactionScore)
            .HasPrecision(3, 1)
            .HasComment("Score de satisfaction (1-5)");

        builder.Property(ci => ci.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue("Medium")
            .HasComment("Priorité (Low, Medium, High, Critical)");

        builder.Property(ci => ci.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("Open")
            .HasComment("Statut (Open, In Progress, Closed, Cancelled)");

        // ===== AGENT INFORMATION =====
        builder.Property(ci => ci.AgentId)
            .HasMaxLength(255)
            .HasComment("ID de l'agent qui a traité l'interaction");

        builder.Property(ci => ci.AgentName)
            .HasMaxLength(200)
            .HasComment("Nom de l'agent");

        builder.Property(ci => ci.Department)
            .HasMaxLength(100)
            .HasComment("Département responsable");

        builder.Property(ci => ci.Team)
            .HasMaxLength(100)
            .HasComment("Équipe responsable");

        // ===== FOLLOW-UP INFORMATION =====
        builder.Property(ci => ci.RequiresFollowUp)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Nécessite un suivi");

        builder.Property(ci => ci.FollowUpDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de suivi prévue");

        builder.Property(ci => ci.FollowUpNotes)
            .HasMaxLength(2000)
            .HasComment("Notes pour le suivi");

        builder.Property(ci => ci.FollowUpCompletedDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de réalisation du suivi");

        builder.Property(ci => ci.FollowUpBy)
            .HasMaxLength(255)
            .HasComment("Personne responsable du suivi");

        // ===== REFERENCE INFORMATION =====
        builder.Property(ci => ci.ReferenceNumber)
            .HasMaxLength(50)
            .HasComment("Numéro de référence externe");

        builder.Property(ci => ci.RelatedOrderId)
            .HasComment("ID de commande associée");

        builder.Property(ci => ci.RelatedTicketId)
            .HasComment("ID de ticket associé");

        builder.Property(ci => ci.RelatedCampaignId)
            .HasComment("ID de campagne associée");

        builder.Property(ci => ci.ParentInteractionId)
            .HasComment("ID de l'interaction parent");

        // ===== TECHNICAL INFORMATION =====
        builder.Property(ci => ci.Source)
            .HasMaxLength(100)
            .HasComment("Source de l'interaction");

        builder.Property(ci => ci.UserAgent)
            .HasMaxLength(500)
            .HasComment("User agent du navigateur");

        builder.Property(ci => ci.IpAddress)
            .HasMaxLength(45)
            .HasComment("Adresse IP");

        builder.Property(ci => ci.SessionId)
            .HasMaxLength(100)
            .HasComment("ID de session");

        builder.Property(ci => ci.DeviceType)
            .HasMaxLength(50)
            .HasComment("Type d'appareil");

        builder.Property(ci => ci.Location)
            .HasMaxLength(200)
            .HasComment("Localisation géographique");

        // ===== METADATA =====
        builder.Property(ci => ci.Tags)
            .HasMaxLength(1000)
            .HasComment("Tags séparés par des virgules");

        builder.Property(ci => ci.Metadata)
            .HasMaxLength(4000)
            .HasComment("Métadonnées additionnelles (JSON)");

        builder.Property(ci => ci.CreatedBy)
            .HasMaxLength(255)
            .HasComment("Créé par");

        builder.Property(ci => ci.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de création");

        builder.Property(ci => ci.LastModifiedBy)
            .HasMaxLength(255)
            .HasComment("Modifié par");

        builder.Property(ci => ci.LastModifiedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de modification");

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
    private void ConfigureRelationships(EntityTypeBuilder<CustomerInteraction> builder)
    {
        // Relation avec Customer
        builder.HasOne<Domain.Entities.Customer>()
            .WithMany()
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerInteraction_Customer");

        // Auto-relation pour interactions parent/enfant
        builder.HasOne<CustomerInteraction>()
            .WithMany()
            .HasForeignKey(ci => ci.ParentInteractionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_CustomerInteraction_Parent");

        // Index pour les foreign keys
        builder.HasIndex(ci => ci.CustomerId)
            .HasDatabaseName("IX_CustomerInteraction_CustomerId");

        builder.HasIndex(ci => ci.ParentInteractionId)
            .HasDatabaseName("IX_CustomerInteraction_ParentId")
            .HasFilter("[ParentInteractionId] IS NOT NULL");
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<CustomerInteraction> builder)
    {
        // Index principal pour recherche par client et date
        builder.HasIndex(ci => new { ci.CustomerId, ci.InteractionDate })
            .HasDatabaseName("IX_CustomerInteraction_Customer_Date")
            .IsDescending();

        // Index pour type d'interaction
        builder.HasIndex(ci => ci.InteractionType)
            .HasDatabaseName("IX_CustomerInteraction_Type");

        // Index pour canal
        builder.HasIndex(ci => ci.Channel)
            .HasDatabaseName("IX_CustomerInteraction_Channel");

        // Index pour statut et priorité
        builder.HasIndex(ci => new { ci.Status, ci.Priority })
            .HasDatabaseName("IX_CustomerInteraction_Status_Priority");

        // Index pour suivi requis
        builder.HasIndex(ci => ci.RequiresFollowUp)
            .HasDatabaseName("IX_CustomerInteraction_RequiresFollowUp")
            .HasFilter("[RequiresFollowUp] = 1 AND [FollowUpCompletedDate] IS NULL");

        // Index pour date de suivi
        builder.HasIndex(ci => ci.FollowUpDate)
            .HasDatabaseName("IX_CustomerInteraction_FollowUpDate")
            .HasFilter("[FollowUpDate] IS NOT NULL AND [FollowUpCompletedDate] IS NULL");

        // Index pour agent
        builder.HasIndex(ci => ci.AgentId)
            .HasDatabaseName("IX_CustomerInteraction_AgentId")
            .HasFilter("[AgentId] IS NOT NULL");

        // Index pour références externes
        builder.HasIndex(ci => ci.ReferenceNumber)
            .HasDatabaseName("IX_CustomerInteraction_ReferenceNumber")
            .HasFilter("[ReferenceNumber] IS NOT NULL");

        builder.HasIndex(ci => ci.RelatedOrderId)
            .HasDatabaseName("IX_CustomerInteraction_RelatedOrderId")
            .HasFilter("[RelatedOrderId] IS NOT NULL");

        // Index pour sentiment et satisfaction
        builder.HasIndex(ci => new { ci.Sentiment, ci.SatisfactionScore })
            .HasDatabaseName("IX_CustomerInteraction_Sentiment_Satisfaction");

        // Index composé pour recherche avancée
        builder.HasIndex(ci => new { ci.InteractionType, ci.Channel, ci.Status, ci.InteractionDate })
            .HasDatabaseName("IX_CustomerInteraction_Search_Composite");

        // Index covering pour dashboard agent
        builder.HasIndex(ci => new { ci.AgentId, ci.Status, ci.InteractionDate })
            .HasDatabaseName("IX_CustomerInteraction_Agent_Dashboard")
            .IncludeProperties(ci => new { 
                ci.CustomerId, 
                ci.Subject, 
                ci.Priority,
                ci.RequiresFollowUp 
            });

        // Index pour analytics par période
        builder.HasIndex(ci => new { ci.InteractionDate, ci.InteractionType, ci.Channel })
            .HasDatabaseName("IX_CustomerInteraction_Analytics_Period");
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(CustomerInteraction builder)
    {
        // Contrainte pour satisfaction score
        builder.HasCheckConstraint(
            "CK_CustomerInteraction_SatisfactionScore",
            "[SatisfactionScore] IS NULL OR ([SatisfactionScore] >= 1 AND [SatisfactionScore] <= 5)");

        // Contrainte pour durée positive
        builder.HasCheckConstraint(
            "CK_CustomerInteraction_Duration",
            "[Duration] IS NULL OR [Duration] >= 0");

        // Contrainte pour dates cohérentes
        builder.HasCheckConstraint(
            "CK_CustomerInteraction_Dates",
            "[InteractionDate] <= GETUTCDATE() AND " +
            "([FollowUpDate] IS NULL OR [FollowUpDate] >= [InteractionDate]) AND " +
            "([FollowUpCompletedDate] IS NULL OR [FollowUpCompletedDate] >= [InteractionDate])");

        // Contrainte pour suivi cohérent
        builder.HasCheckConstraint(
            "CK_CustomerInteraction_FollowUp",
            "([RequiresFollowUp] = 0) OR ([RequiresFollowUp] = 1 AND [FollowUpDate] IS NOT NULL)");

        // Contrainte pour interaction parent différente
        builder.HasCheckConstraint(
            "CK_CustomerInteraction_ParentDifferent",
            "[ParentInteractionId] IS NULL OR [ParentInteractionId] != [InteractionId]");
    }
}

/// <summary>
/// Configuration Entity Framework pour l'entité CustomerPreference
/// Mapping sophistiqué pour les préférences et comportements clients
/// </summary>
public class CustomerPreferenceConfiguration : IEntityTypeConfiguration<CustomerPreference>
{
    public void Configure(EntityTypeBuilder<CustomerPreference> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("CustomerPreferences", "customer");
        builder.HasKey(cp => cp.Id);

        // ===== PRIMARY KEY =====
        builder.Property(cp => cp.Id)
            .HasColumnName("PreferenceId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== FOREIGN KEY =====
        builder.Property(cp => cp.CustomerId)
            .IsRequired()
            .HasComment("Référence vers le client");

        // ===== BASIC PROPERTIES =====
        builder.Property(cp => cp.PreferenceType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Type de préférence (Product, Service, Communication, etc.)");

        builder.Property(cp => cp.Category)
            .HasMaxLength(100)
            .HasComment("Catégorie de la préférence");

        builder.Property(cp => cp.SubCategory)
            .HasMaxLength(100)
            .HasComment("Sous-catégorie de la préférence");

        builder.Property(cp => cp.PreferenceKey)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Clé unique de la préférence");

        builder.Property(cp => cp.PreferenceValue)
            .HasMaxLength(2000)
            .HasComment("Valeur de la préférence (JSON si complexe)");

        builder.Property(cp => cp.StringValue)
            .HasMaxLength(500)
            .HasComment("Valeur texte simple");

        builder.Property(cp => cp.NumericValue)
            .HasColumnType("decimal(18,6)")
            .HasComment("Valeur numérique");

        builder.Property(cp => cp.BooleanValue)
            .HasComment("Valeur booléenne");

        builder.Property(cp => cp.DateValue)
            .HasColumnType("datetime2(3)")
            .HasComment("Valeur date");

        builder.Property(cp => cp.Priority)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Priorité de la préférence (plus élevé = plus important)");

        builder.Property(cp => cp.Confidence)
            .HasColumnType("decimal(5,4)")
            .HasDefaultValue(1.0000m)
            .HasComment("Niveau de confiance (0-1)");

        builder.Property(cp => cp.Source)
            .HasMaxLength(100)
            .HasComment("Source de la préférence (Explicit, Implicit, Inferred)");

        builder.Property(cp => cp.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Préférence active");

        builder.Property(cp => cp.IsInferred)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Préférence inférée par analyse comportementale");

        // ===== TIMING INFORMATION =====
        builder.Property(cp => cp.StartDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de début de validité");

        builder.Property(cp => cp.EndDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date de fin de validité");

        builder.Property(cp => cp.LastUpdatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Dernière mise à jour");

        builder.Property(cp => cp.LastUsedDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Dernière utilisation pour recommandation");

        builder.Property(cp => cp.ExpiryDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Date d'expiration de la préférence");

        // ===== CONTEXT INFORMATION =====
        builder.Property(cp => cp.Context)
            .HasMaxLength(2000)
            .HasComment("Contexte d'application (JSON)");

        builder.Property(cp => cp.Conditions)
            .HasMaxLength(2000)
            .HasComment("Conditions d'application (JSON)");

        builder.Property(cp => cp.Seasonality)
            .HasMaxLength(100)
            .HasComment("Saisonnalité (Summer, Winter, Holiday, etc.)");

        builder.Property(cp => cp.TimeOfDay)
            .HasMaxLength(50)
            .HasComment("Moment de la journée (Morning, Afternoon, Evening)");

        builder.Property(cp => cp.DayOfWeek)
            .HasMaxLength(100)
            .HasComment("Jours de la semaine préférés (JSON array)");

        builder.Property(cp => cp.Frequency)
            .HasMaxLength(50)
            .HasComment("Fréquence d'application");

        // ===== USAGE STATISTICS =====
        builder.Property(cp => cp.UsageCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Nombre d'utilisations de la préférence");

        builder.Property(cp => cp.SuccessRate)
            .HasColumnType("decimal(5,4)")
            .HasComment("Taux de succès des recommandations basées sur cette préférence");

        builder.Property(cp => cp.ClickThroughRate)
            .HasColumnType("decimal(5,4)")
            .HasComment("Taux de clic pour les recommandations");

        builder.Property(cp => cp.ConversionRate)
            .HasColumnType("decimal(5,4)")
            .HasComment("Taux de conversion pour les recommandations");

        builder.Property(cp => cp.AverageOrderValue)
            .HasColumnType("decimal(10,2)")
            .HasComment("Panier moyen pour cette préférence");

        // ===== METADATA =====
        builder.Property(cp => cp.Tags)
            .HasMaxLength(1000)
            .HasComment("Tags associés (séparés par des virgules)");

        builder.Property(cp => cp.Notes)
            .HasMaxLength(2000)
            .HasComment("Notes libres");

        builder.Property(cp => cp.Metadata)
            .HasMaxLength(4000)
            .HasComment("Métadonnées additionnelles (JSON)");

        builder.Property(cp => cp.CreatedBy)
            .HasMaxLength(255)
            .HasComment("Créé par");

        builder.Property(cp => cp.CreatedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de création");

        builder.Property(cp => cp.LastModifiedBy)
            .HasMaxLength(255)
            .HasComment("Modifié par");

        builder.Property(cp => cp.LastModifiedDate)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date de modification");

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
    private void ConfigureRelationships(EntityTypeBuilder<CustomerPreference> builder)
    {
        // Relation avec Customer
        builder.HasOne<Domain.Entities.Customer>()
            .WithMany()
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerPreference_Customer");

        // Index pour la foreign key
        builder.HasIndex(cp => cp.CustomerId)
            .HasDatabaseName("IX_CustomerPreference_CustomerId");
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<CustomerPreference> builder)
    {
        // Index principal pour recherche par client et type
        builder.HasIndex(cp => new { cp.CustomerId, cp.PreferenceType })
            .HasDatabaseName("IX_CustomerPreference_Customer_Type");

        // Index unique pour éviter les doublons
        builder.HasIndex(cp => new { cp.CustomerId, cp.PreferenceKey, cp.Category })
            .IsUnique()
            .HasDatabaseName("IX_CustomerPreference_Customer_Key_Category_Unique")
            .HasFilter("[Category] IS NOT NULL");

        // Index pour type de préférence
        builder.HasIndex(cp => cp.PreferenceType)
            .HasDatabaseName("IX_CustomerPreference_Type");

        // Index pour catégorie
        builder.HasIndex(cp => new { cp.Category, cp.SubCategory })
            .HasDatabaseName("IX_CustomerPreference_Category_SubCategory");

        // Index pour préférences actives
        builder.HasIndex(cp => cp.IsActive)
            .HasDatabaseName("IX_CustomerPreference_IsActive")
            .HasFilter("[IsActive] = 1");

        // Index pour préférences inférées
        builder.HasIndex(cp => cp.IsInferred)
            .HasDatabaseName("IX_CustomerPreference_IsInferred")
            .HasFilter("[IsInferred] = 1");

        // Index pour priorité
        builder.HasIndex(cp => cp.Priority)
            .HasDatabaseName("IX_CustomerPreference_Priority")
            .IsDescending();

        // Index pour confiance
        builder.HasIndex(cp => cp.Confidence)
            .HasDatabaseName("IX_CustomerPreference_Confidence")
            .IsDescending();

        // Index pour dates de validité
        builder.HasIndex(cp => new { cp.StartDate, cp.EndDate })
            .HasDatabaseName("IX_CustomerPreference_ValidityPeriod");

        // Index pour expiration
        builder.HasIndex(cp => cp.ExpiryDate)
            .HasDatabaseName("IX_CustomerPreference_ExpiryDate")
            .HasFilter("[ExpiryDate] IS NOT NULL AND [ExpiryDate] > GETUTCDATE()");

        // Index pour source
        builder.HasIndex(cp => cp.Source)
            .HasDatabaseName("IX_CustomerPreference_Source");

        // Index pour dernière utilisation
        builder.HasIndex(cp => cp.LastUsedDate)
            .HasDatabaseName("IX_CustomerPreference_LastUsed")
            .IsDescending()
            .HasFilter("[LastUsedDate] IS NOT NULL");

        // Index composé pour recommandations
        builder.HasIndex(cp => new { cp.CustomerId, cp.IsActive, cp.Priority, cp.Confidence })
            .HasDatabaseName("IX_CustomerPreference_Recommendations_Composite");

        // Index covering pour analytics
        builder.HasIndex(cp => new { cp.PreferenceType, cp.IsActive, cp.CreatedDate })
            .HasDatabaseName("IX_CustomerPreference_Analytics_Covering")
            .IncludeProperties(cp => new { 
                cp.UsageCount, 
                cp.SuccessRate, 
                cp.ConversionRate,
                cp.ClickThroughRate 
            });

        // Index pour recherche textuelle
        builder.HasIndex(cp => cp.StringValue)
            .HasDatabaseName("IX_CustomerPreference_StringValue")
            .HasFilter("[StringValue] IS NOT NULL");

        // Index pour valeurs numériques
        builder.HasIndex(cp => cp.NumericValue)
            .HasDatabaseName("IX_CustomerPreference_NumericValue")
            .HasFilter("[NumericValue] IS NOT NULL");
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<CustomerPreference> builder)
    {
        // Contrainte pour dates cohérentes
        builder.HasCheckConstraint(
            "CK_CustomerPreference_Dates",
            "[EndDate] IS NULL OR [EndDate] >= [StartDate]");

        // Contrainte pour confiance entre 0 et 1
        builder.HasCheckConstraint(
            "CK_CustomerPreference_Confidence",
            "[Confidence] >= 0 AND [Confidence] <= 1");

        // Contrainte pour taux entre 0 et 1
        builder.HasCheckConstraint(
            "CK_CustomerPreference_Rates",
            "([SuccessRate] IS NULL OR ([SuccessRate] >= 0 AND [SuccessRate] <= 1)) AND " +
            "([ClickThroughRate] IS NULL OR ([ClickThroughRate] >= 0 AND [ClickThroughRate] <= 1)) AND " +
            "([ConversionRate] IS NULL OR ([ConversionRate] >= 0 AND [ConversionRate] <= 1))");

        // Contrainte pour valeurs positives
        builder.HasCheckConstraint(
            "CK_CustomerPreference_PositiveValues",
            "[UsageCount] >= 0 AND " +
            "([AverageOrderValue] IS NULL OR [AverageOrderValue] >= 0)");

        // Contrainte pour expiration cohérente
        builder.HasCheckConstraint(
            "CK_CustomerPreference_Expiry",
            "[ExpiryDate] IS NULL OR [ExpiryDate] > [StartDate]");

        // Contrainte pour au moins une valeur définie
        builder.HasCheckConstraint(
            "CK_CustomerPreference_ValueRequired",
            "[PreferenceValue] IS NOT NULL OR [StringValue] IS NOT NULL OR " +
            "[NumericValue] IS NOT NULL OR [BooleanValue] IS NOT NULL OR [DateValue] IS NOT NULL");
    }
}