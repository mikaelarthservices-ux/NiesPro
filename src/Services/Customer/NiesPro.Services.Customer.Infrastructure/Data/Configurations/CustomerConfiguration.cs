using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NiesPro.Services.Customer.Domain.Entities;
using NiesPro.Services.Customer.Domain.ValueObjects;
using NiesPro.Services.Customer.Domain.Enums;

namespace NiesPro.Services.Customer.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration Entity Framework pour l'entité Customer
/// Mapping sophistiqué avec Value Objects, Relations et Performance
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Domain.Entities.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        // ===== TABLE CONFIGURATION =====
        builder.ToTable("Customers", "customer");
        builder.HasKey(c => c.Id);

        // ===== PRIMARY KEY =====
        builder.Property(c => c.Id)
            .HasColumnName("CustomerId")
            .ValueGeneratedNever(); // Guid généré en code

        // ===== BASIC PROPERTIES =====
        builder.Property(c => c.CustomerNumber)
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Numéro unique généré automatiquement");

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(CustomerStatus.Active)
            .HasComment("Statut du client (Active, Inactive, Suspended, Blocked)");

        builder.Property(c => c.RegistrationDate)
            .HasColumnType("datetime2(3)")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()")
            .HasComment("Date d'inscription du client");

        builder.Property(c => c.LastLoginDate)
            .HasColumnType("datetime2(3)")
            .HasComment("Dernière connexion du client");

        builder.Property(c => c.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Email vérifié par le client");

        builder.Property(c => c.IsPhoneVerified)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Téléphone vérifié par le client");

        builder.Property(c => c.LanguagePreference)
            .HasMaxLength(10)
            .HasDefaultValue("fr-FR")
            .HasComment("Langue préférée du client");

        builder.Property(c => c.TimeZone)
            .HasMaxLength(50)
            .HasDefaultValue("Europe/Paris")
            .HasComment("Fuseau horaire du client");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        // ===== VALUE OBJECTS CONFIGURATION =====
        ConfigurePersonalInfo(builder);
        ConfigureContactInfo(builder);
        ConfigureLoyaltyStats(builder);

        // ===== RELATIONSHIPS =====
        ConfigureRelationships(builder);

        // ===== INDEXES =====
        ConfigureIndexes(builder);

        // ===== CONSTRAINTS =====
        ConfigureConstraints(builder);

        // ===== SEED DATA =====
        ConfigureSeedData(builder);
    }

    /// <summary>
    /// Configuration du Value Object PersonalInfo
    /// </summary>
    private void ConfigurePersonalInfo(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        builder.OwnsOne(c => c.PersonalInfo, pi =>
        {
            pi.Property(p => p.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired()
                .HasComment("Prénom du client");

            pi.Property(p => p.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired()
                .HasComment("Nom de famille du client");

            pi.Property(p => p.MiddleName)
                .HasColumnName("MiddleName")
                .HasMaxLength(100)
                .HasComment("Nom du milieu ou deuxième prénom");

            pi.Property(p => p.Title)
                .HasColumnName("Title")
                .HasMaxLength(20)
                .HasComment("Titre de civilité (Mr, Mme, Dr, etc.)");

            pi.Property(p => p.DateOfBirth)
                .HasColumnName("DateOfBirth")
                .HasColumnType("date")
                .HasComment("Date de naissance");

            pi.Property(p => p.Gender)
                .HasColumnName("Gender")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasComment("Genre du client");

            pi.Property(p => p.Nationality)
                .HasColumnName("Nationality")
                .HasMaxLength(100)
                .HasComment("Nationalité du client");

            // Index composé pour recherche par nom
            pi.HasIndex(p => new { p.FirstName, p.LastName })
                .HasDatabaseName("IX_Customer_PersonalInfo_Names")
                .HasFilter("[FirstName] IS NOT NULL AND [LastName] IS NOT NULL");

            // Index pour date de naissance (anniversaires)
            pi.HasIndex(p => p.DateOfBirth)
                .HasDatabaseName("IX_Customer_PersonalInfo_DateOfBirth")
                .HasFilter("[DateOfBirth] IS NOT NULL");
        });
    }

    /// <summary>
    /// Configuration du Value Object ContactInfo
    /// </summary>
    private void ConfigureContactInfo(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        builder.OwnsOne(c => c.ContactInfo, ci =>
        {
            ci.Property(c => c.Email)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired()
                .HasComment("Adresse email principale");

            ci.Property(c => c.AlternativeEmail)
                .HasColumnName("AlternativeEmail")
                .HasMaxLength(255)
                .HasComment("Adresse email alternative");

            ci.Property(c => c.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(20)
                .HasComment("Numéro de téléphone principal");

            ci.Property(c => c.AlternativePhone)
                .HasColumnName("AlternativePhone")
                .HasMaxLength(20)
                .HasComment("Numéro de téléphone alternatif");

            ci.Property(c => c.Address)
                .HasColumnName("Address")
                .HasMaxLength(500)
                .HasComment("Adresse complète");

            ci.Property(c => c.AddressLine2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(500)
                .HasComment("Complément d'adresse");

            ci.Property(c => c.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .HasComment("Ville");

            ci.Property(c => c.State)
                .HasColumnName("State")
                .HasMaxLength(100)
                .HasComment("État/Province/Région");

            ci.Property(c => c.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20)
                .HasComment("Code postal");

            ci.Property(c => c.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .HasComment("Pays");

            // Index unique pour email
            ci.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_Customer_ContactInfo_Email_Unique")
                .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");

            // Index pour téléphone
            ci.HasIndex(c => c.Phone)
                .HasDatabaseName("IX_Customer_ContactInfo_Phone")
                .HasFilter("[Phone] IS NOT NULL");

            // Index géographique pour recherche par localisation
            ci.HasIndex(c => new { c.Country, c.City, c.PostalCode })
                .HasDatabaseName("IX_Customer_ContactInfo_Geographic");
        });
    }

    /// <summary>
    /// Configuration du Value Object LoyaltyStats
    /// </summary>
    private void ConfigureLoyaltyStats(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        builder.OwnsOne(c => c.LoyaltyStats, ls =>
        {
            ls.Property(l => l.TotalPoints)
                .HasColumnName("LoyaltyTotalPoints")
                .HasPrecision(10, 2)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Points de fidélité actuels");

            ls.Property(l => l.LifetimePoints)
                .HasColumnName("LoyaltyLifetimePoints")
                .HasPrecision(12, 2)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Total des points gagnés à vie");

            ls.Property(l => l.CurrentTier)
                .HasColumnName("LoyaltyCurrentTier")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Bronze")
                .HasComment("Niveau de fidélité actuel");

            ls.Property(l => l.PointsToNextTier)
                .HasColumnName("LoyaltyPointsToNextTier")
                .HasPrecision(10, 2)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Points nécessaires pour le niveau suivant");

            ls.Property(l => l.TierExpiryDate)
                .HasColumnName("LoyaltyTierExpiryDate")
                .HasColumnType("date")
                .HasComment("Date d'expiration du niveau actuel");

            ls.Property(l => l.LastEarnDate)
                .HasColumnName("LoyaltyLastEarnDate")
                .HasColumnType("datetime2(3)")
                .HasComment("Dernière date de gain de points");

            ls.Property(l => l.LastRedemptionDate)
                .HasColumnName("LoyaltyLastRedemptionDate")
                .HasColumnType("datetime2(3)")
                .HasComment("Dernière date d'utilisation de points");

            // Index pour recherche par niveau
            ls.HasIndex(l => l.CurrentTier)
                .HasDatabaseName("IX_Customer_LoyaltyStats_CurrentTier");

            // Index pour tri par points
            ls.HasIndex(l => l.TotalPoints)
                .HasDatabaseName("IX_Customer_LoyaltyStats_TotalPoints")
                .IsDescending();

            // Index pour expiration des niveaux
            ls.HasIndex(l => l.TierExpiryDate)
                .HasDatabaseName("IX_Customer_LoyaltyStats_TierExpiry")
                .HasFilter("[LoyaltyTierExpiryDate] IS NOT NULL");
        });
    }

    /// <summary>
    /// Configuration des relations avec autres entités
    /// </summary>
    private void ConfigureRelationships(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        // Relation 1:1 avec CustomerProfile
        builder.HasOne<CustomerProfile>()
            .WithOne()
            .HasForeignKey<CustomerProfile>(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerProfile_Customer");

        // Relation 1:N avec CustomerInteractions
        builder.HasMany<CustomerInteraction>()
            .WithOne()
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerInteraction_Customer");

        // Relation 1:N avec CustomerPreferences
        builder.HasMany<CustomerPreference>()
            .WithOne()
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_CustomerPreference_Customer");

        // Relation N:N avec CustomerSegments (via table de liaison)
        builder.HasMany<CustomerSegment>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "CustomerSegmentMemberships",
                j => j.HasOne<CustomerSegment>()
                      .WithMany()
                      .HasForeignKey("SegmentId")
                      .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Domain.Entities.Customer>()
                      .WithMany()
                      .HasForeignKey("CustomerId")
                      .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("CustomerSegmentMemberships", "customer");
                    j.HasKey("CustomerId", "SegmentId");
                    j.Property<DateTime>("AssignedDate")
                     .HasDefaultValueSql("GETUTCDATE()");
                    j.Property<string>("AssignedBy")
                     .HasMaxLength(255);
                    j.Property<bool>("IsActive")
                     .HasDefaultValue(true);
                    j.HasIndex("SegmentId", "IsActive")
                     .HasDatabaseName("IX_CustomerSegmentMemberships_Segment_Active");
                });
    }

    /// <summary>
    /// Configuration des index pour performance
    /// </summary>
    private void ConfigureIndexes(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        // Index unique pour CustomerNumber
        builder.HasIndex(c => c.CustomerNumber)
            .IsUnique()
            .HasDatabaseName("IX_Customer_CustomerNumber_Unique")
            .HasFilter("[CustomerNumber] IS NOT NULL");

        // Index pour Status
        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Customer_Status");

        // Index pour dates importantes
        builder.HasIndex(c => c.RegistrationDate)
            .HasDatabaseName("IX_Customer_RegistrationDate");

        builder.HasIndex(c => c.LastLoginDate)
            .HasDatabaseName("IX_Customer_LastLoginDate")
            .HasFilter("[LastLoginDate] IS NOT NULL");

        // Index composé pour recherche active
        builder.HasIndex(c => new { c.Status, c.IsDeleted })
            .HasDatabaseName("IX_Customer_Status_IsDeleted");

        // Index composé pour analytics
        builder.HasIndex(c => new { c.RegistrationDate, c.Status, c.IsDeleted })
            .HasDatabaseName("IX_Customer_Analytics_Composite");

        // Index pour vérification
        builder.HasIndex(c => new { c.IsEmailVerified, c.IsPhoneVerified })
            .HasDatabaseName("IX_Customer_Verification_Status");

        // Index covering pour recherche courante
        builder.HasIndex(c => new { c.Status, c.RegistrationDate })
            .HasDatabaseName("IX_Customer_Status_Registration_Covering")
            .IncludeProperties(c => new { c.CustomerNumber, c.LastLoginDate });
    }

    /// <summary>
    /// Configuration des contraintes métier
    /// </summary>
    private void ConfigureConstraints(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        // Contrainte de vérification pour RegistrationDate
        builder.HasCheckConstraint(
            "CK_Customer_RegistrationDate",
            "[RegistrationDate] <= GETUTCDATE()");

        // Contrainte de vérification pour LastLoginDate
        builder.HasCheckConstraint(
            "CK_Customer_LastLoginDate",
            "[LastLoginDate] IS NULL OR [LastLoginDate] >= [RegistrationDate]");

        // Contrainte pour LoyaltyTotalPoints
        builder.HasCheckConstraint(
            "CK_Customer_LoyaltyTotalPoints",
            "[LoyaltyTotalPoints] >= 0");

        // Contrainte pour LoyaltyLifetimePoints
        builder.HasCheckConstraint(
            "CK_Customer_LoyaltyLifetimePoints",
            "[LoyaltyLifetimePoints] >= [LoyaltyTotalPoints]");
    }

    /// <summary>
    /// Configuration des données de test/demo
    /// </summary>
    private void ConfigureSeedData(EntityTypeBuilder<Domain.Entities.Customer> builder)
    {
        // TODO: Implémenter seed data pour développement/test
        // Exemple de customer par défaut pour tests
        /*
        builder.HasData(new
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerNumber = "CUST-000001",
            Status = CustomerStatus.Active,
            RegistrationDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IsEmailVerified = true,
            IsPhoneVerified = false,
            LanguagePreference = "fr-FR",
            TimeZone = "Europe/Paris",
            IsDeleted = false
        });
        */
    }
}