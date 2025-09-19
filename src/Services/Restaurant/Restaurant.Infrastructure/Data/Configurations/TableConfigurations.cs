using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Table
/// </summary>
public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        // Configuration de la table
        builder.ToTable("Tables");
        
        // Clé primaire
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(t => t.Number)
            .HasColumnName("Number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.SeatingCapacity)
            .HasColumnName("SeatingCapacity")
            .IsRequired();

        builder.Property(t => t.TableType)
            .HasColumnName("TableType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Section)
            .HasColumnName("Section")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.Property(t => t.IsAvailable)
            .HasColumnName("IsAvailable")
            .HasDefaultValue(true);

        builder.Property(t => t.RequiresCleaning)
            .HasColumnName("RequiresCleaning")
            .HasDefaultValue(false);

        builder.Property(t => t.LastCleanedAt)
            .HasColumnName("LastCleanedAt");

        builder.Property(t => t.CurrentReservationId)
            .HasColumnName("CurrentReservationId");

        builder.Property(t => t.OccupiedSince)
            .HasColumnName("OccupiedSince");

        // Métriques
        builder.Property(t => t.TotalReservations)
            .HasColumnName("TotalReservations")
            .HasDefaultValue(0);

        builder.Property(t => t.TotalCovers)
            .HasColumnName("TotalCovers")
            .HasDefaultValue(0);

        builder.Property(t => t.Revenue)
            .HasColumnName("Revenue")
            .HasPrecision(12, 2)
            .HasDefaultValue(0);

        // Collections
        builder.Property(t => t.Features)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("Features")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(t => t.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Configuration des Value Objects (déjà dans DbContext)
        
        // Index
        builder.HasIndex(t => t.Number)
            .IsUnique()
            .HasDatabaseName("IX_Tables_Number");

        builder.HasIndex(t => t.Section)
            .HasDatabaseName("IX_Tables_Section");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tables_Status");

        builder.HasIndex(t => t.IsAvailable)
            .HasDatabaseName("IX_Tables_IsAvailable");

        builder.HasIndex(t => new { t.Section, t.Status })
            .HasDatabaseName("IX_Tables_Section_Status");

        // Relations
        builder.HasMany<TableReservation>()
            .WithOne()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_TableReservations_Tables");

        // Contraintes
        builder.HasCheckConstraint("CK_Tables_SeatingCapacity", "SeatingCapacity > 0");
        builder.HasCheckConstraint("CK_Tables_Revenue", "Revenue >= 0");
        builder.HasCheckConstraint("CK_Tables_TotalReservations", "TotalReservations >= 0");
        builder.HasCheckConstraint("CK_Tables_TotalCovers", "TotalCovers >= 0");

        // Configuration des données par défaut
        builder.Property(t => t.TotalReservations).HasDefaultValue(0);
        builder.Property(t => t.TotalCovers).HasDefaultValue(0);
        builder.Property(t => t.Revenue).HasDefaultValue(0m);
        builder.Property(t => t.IsAvailable).HasDefaultValue(true);
        builder.Property(t => t.RequiresCleaning).HasDefaultValue(false);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité TableReservation
/// </summary>
public class TableReservationConfiguration : IEntityTypeConfiguration<TableReservation>
{
    public void Configure(EntityTypeBuilder<TableReservation> builder)
    {
        // Configuration de la table
        builder.ToTable("TableReservations");
        
        // Clé primaire
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(r => r.ReservationNumber)
            .HasColumnName("ReservationNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.TableId)
            .HasColumnName("TableId")
            .IsRequired();

        builder.Property(r => r.ReservationDateTime)
            .HasColumnName("ReservationDateTime")
            .IsRequired();

        builder.Property(r => r.EstimatedEndTime)
            .HasColumnName("EstimatedEndTime")
            .IsRequired();

        builder.Property(r => r.PartySize)
            .HasColumnName("PartySize")
            .IsRequired();

        builder.Property(r => r.CustomerName)
            .HasColumnName("CustomerName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.CustomerPhone)
            .HasColumnName("CustomerPhone")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.ReservationType)
            .HasColumnName("ReservationType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(r => r.CustomerEmail)
            .HasColumnName("CustomerEmail")
            .HasMaxLength(250);

        builder.Property(r => r.SpecialRequests)
            .HasColumnName("SpecialRequests")
            .HasMaxLength(1000);

        builder.Property(r => r.IsVip)
            .HasColumnName("IsVip")
            .HasDefaultValue(false);

        builder.Property(r => r.CheckInTime)
            .HasColumnName("CheckInTime");

        builder.Property(r => r.SeatedTime)
            .HasColumnName("SeatedTime");

        builder.Property(r => r.CompletedTime)
            .HasColumnName("CompletedTime");

        builder.Property(r => r.CancelledTime)
            .HasColumnName("CancelledTime");

        builder.Property(r => r.CancellationReason)
            .HasColumnName("CancellationReason")
            .HasMaxLength(500);

        builder.Property(r => r.DepositAmount)
            .HasColumnName("DepositAmount")
            .HasPrecision(10, 2);

        builder.Property(r => r.DepositPaid)
            .HasColumnName("DepositPaid")
            .HasDefaultValue(false);

        builder.Property(r => r.ReminderSent)
            .HasColumnName("ReminderSent")
            .HasDefaultValue(false);

        builder.Property(r => r.ConfirmationSent)
            .HasColumnName("ConfirmationSent")
            .HasDefaultValue(false);

        builder.Property(r => r.ActualPartySize)
            .HasColumnName("ActualPartySize");

        builder.Property(r => r.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasPrecision(12, 2);

        builder.Property(r => r.ActualDuration)
            .HasColumnName("ActualDuration");

        // Collections
        builder.Property(r => r.DietaryRestrictions)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("DietaryRestrictions")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(r => r.ReservationNumber)
            .IsUnique()
            .HasDatabaseName("IX_TableReservations_ReservationNumber");

        builder.HasIndex(r => r.TableId)
            .HasDatabaseName("IX_TableReservations_TableId");

        builder.HasIndex(r => new { r.TableId, r.ReservationDateTime })
            .HasDatabaseName("IX_TableReservations_Table_DateTime");

        builder.HasIndex(r => r.CustomerPhone)
            .HasDatabaseName("IX_TableReservations_CustomerPhone");

        builder.HasIndex(r => r.CustomerEmail)
            .HasDatabaseName("IX_TableReservations_CustomerEmail");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_TableReservations_Status");

        builder.HasIndex(r => r.ReservationDateTime)
            .HasDatabaseName("IX_TableReservations_ReservationDateTime");

        builder.HasIndex(r => new { r.Status, r.ReservationDateTime })
            .HasDatabaseName("IX_TableReservations_Status_DateTime");

        // Relations
        builder.HasOne<Table>()
            .WithMany()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_TableReservations_Tables");

        // Contraintes
        builder.HasCheckConstraint("CK_TableReservations_PartySize", "PartySize > 0");
        builder.HasCheckConstraint("CK_TableReservations_DepositAmount", "DepositAmount >= 0");
        builder.HasCheckConstraint("CK_TableReservations_TotalAmount", "TotalAmount >= 0");
        builder.HasCheckConstraint("CK_TableReservations_ActualPartySize", "ActualPartySize IS NULL OR ActualPartySize > 0");
        builder.HasCheckConstraint("CK_TableReservations_EstimatedEndTime", "EstimatedEndTime > ReservationDateTime");

        // Configuration des données par défaut
        builder.Property(r => r.IsVip).HasDefaultValue(false);
        builder.Property(r => r.DepositPaid).HasDefaultValue(false);
        builder.Property(r => r.ReminderSent).HasDefaultValue(false);
        builder.Property(r => r.ConfirmationSent).HasDefaultValue(false);
    }
}