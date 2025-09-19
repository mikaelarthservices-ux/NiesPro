using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité KitchenOrder
/// </summary>
public class KitchenOrderConfiguration : IEntityTypeConfiguration<KitchenOrder>
{
    public void Configure(EntityTypeBuilder<KitchenOrder> builder)
    {
        // Configuration de la table
        builder.ToTable("KitchenOrders");
        
        // Clé primaire
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(k => k.OrderNumber)
            .HasColumnName("OrderNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(k => k.TableId)
            .HasColumnName("TableId")
            .IsRequired();

        builder.Property(k => k.OrderType)
            .HasColumnName("OrderType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(k => k.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(k => k.Priority)
            .HasColumnName("Priority")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(k => k.KitchenSection)
            .HasColumnName("KitchenSection")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(k => k.OrderedAt)
            .HasColumnName("OrderedAt")
            .IsRequired();

        builder.Property(k => k.EstimatedReadyTime)
            .HasColumnName("EstimatedReadyTime")
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(k => k.CustomerId)
            .HasColumnName("CustomerId");

        builder.Property(k => k.WaiterId)
            .HasColumnName("WaiterId");

        builder.Property(k => k.ChefId)
            .HasColumnName("ChefId");

        // Informations de timing
        builder.Property(k => k.AcceptedAt)
            .HasColumnName("AcceptedAt");

        builder.Property(k => k.StartedAt)
            .HasColumnName("StartedAt");

        builder.Property(k => k.ReadyAt)
            .HasColumnName("ReadyAt");

        builder.Property(k => k.ServedAt)
            .HasColumnName("ServedAt");

        builder.Property(k => k.CompletedAt)
            .HasColumnName("CompletedAt");

        builder.Property(k => k.CancelledAt)
            .HasColumnName("CancelledAt");

        builder.Property(k => k.ActualPreparationTime)
            .HasColumnName("ActualPreparationTime");

        // Instructions et notes
        builder.Property(k => k.SpecialInstructions)
            .HasColumnName("SpecialInstructions")
            .HasMaxLength(1000);

        builder.Property(k => k.CustomerNotes)
            .HasColumnName("CustomerNotes")
            .HasMaxLength(1000);

        builder.Property(k => k.ChefNotes)
            .HasColumnName("ChefNotes")
            .HasMaxLength(1000);

        builder.Property(k => k.CancellationReason)
            .HasColumnName("CancellationReason")
            .HasMaxLength(500);

        // Métrique et qualité
        builder.Property(k => k.IsRush)
            .HasColumnName("IsRush")
            .HasDefaultValue(false);

        builder.Property(k => k.IsComplicated)
            .HasColumnName("IsComplicated")
            .HasDefaultValue(false);

        builder.Property(k => k.RequiresSpecialAttention)
            .HasColumnName("RequiresSpecialAttention")
            .HasDefaultValue(false);

        builder.Property(k => k.QualityLevel)
            .HasColumnName("QualityLevel")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(k => k.CustomerRating)
            .HasColumnName("CustomerRating");

        builder.Property(k => k.CustomerFeedback)
            .HasColumnName("CustomerFeedback")
            .HasMaxLength(1000);

        // Gestion des allergies et régimes
        builder.Property(k => k.Allergens)
            .HasConversion(
                v => string.Join(',', v.Select(a => (int)a)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (AllergenType)int.Parse(s))
                      .ToList())
            .HasColumnName("Allergens")
            .HasMaxLength(500);

        builder.Property(k => k.DietaryRequirements)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("DietaryRequirements")
            .HasMaxLength(1000);

        builder.Property(k => k.RequiresAllergenAttention)
            .HasColumnName("RequiresAllergenAttention")
            .HasDefaultValue(false);

        // Prix et coûts
        builder.Property(k => k.SubTotal)
            .HasColumnName("SubTotal")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(k => k.DiscountAmount)
            .HasColumnName("DiscountAmount")
            .HasPrecision(10, 2);

        builder.Property(k => k.TotalAmount)
            .HasColumnName("TotalAmount")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(k => k.TipAmount)
            .HasColumnName("TipAmount")
            .HasPrecision(10, 2);

        // Timestamps
        builder.Property(k => k.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(k => k.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(k => k.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_KitchenOrders_OrderNumber");

        builder.HasIndex(k => k.TableId)
            .HasDatabaseName("IX_KitchenOrders_TableId");

        builder.HasIndex(k => k.CustomerId)
            .HasDatabaseName("IX_KitchenOrders_CustomerId");

        builder.HasIndex(k => k.ChefId)
            .HasDatabaseName("IX_KitchenOrders_ChefId");

        builder.HasIndex(k => k.WaiterId)
            .HasDatabaseName("IX_KitchenOrders_WaiterId");

        builder.HasIndex(k => k.Status)
            .HasDatabaseName("IX_KitchenOrders_Status");

        builder.HasIndex(k => k.Priority)
            .HasDatabaseName("IX_KitchenOrders_Priority");

        builder.HasIndex(k => k.KitchenSection)
            .HasDatabaseName("IX_KitchenOrders_KitchenSection");

        builder.HasIndex(k => k.OrderedAt)
            .HasDatabaseName("IX_KitchenOrders_OrderedAt");

        builder.HasIndex(k => k.EstimatedReadyTime)
            .HasDatabaseName("IX_KitchenOrders_EstimatedReadyTime");

        builder.HasIndex(k => new { k.Status, k.OrderedAt })
            .HasDatabaseName("IX_KitchenOrders_Status_OrderedAt");

        builder.HasIndex(k => new { k.KitchenSection, k.Status })
            .HasDatabaseName("IX_KitchenOrders_Section_Status");

        builder.HasIndex(k => new { k.Priority, k.EstimatedReadyTime })
            .HasDatabaseName("IX_KitchenOrders_Priority_EstimatedTime");

        // Relations
        builder.HasOne<Table>()
            .WithMany()
            .HasForeignKey(k => k.TableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_KitchenOrders_Tables");

        builder.HasMany<KitchenOrderItem>()
            .WithOne()
            .HasForeignKey(i => i.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderItems_KitchenOrders");

        builder.HasMany<KitchenOrderLog>()
            .WithOne()
            .HasForeignKey(l => l.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderLogs_KitchenOrders");

        builder.HasMany<KitchenOrderModification>()
            .WithOne()
            .HasForeignKey(m => m.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderModifications_KitchenOrders");

        // Contraintes
        builder.HasCheckConstraint("CK_KitchenOrders_SubTotal", "SubTotal >= 0");
        builder.HasCheckConstraint("CK_KitchenOrders_DiscountAmount", "DiscountAmount IS NULL OR DiscountAmount >= 0");
        builder.HasCheckConstraint("CK_KitchenOrders_TotalAmount", "TotalAmount >= 0");
        builder.HasCheckConstraint("CK_KitchenOrders_TipAmount", "TipAmount IS NULL OR TipAmount >= 0");
        builder.HasCheckConstraint("CK_KitchenOrders_CustomerRating", "CustomerRating IS NULL OR (CustomerRating >= 1 AND CustomerRating <= 5)");
        builder.HasCheckConstraint("CK_KitchenOrders_EstimatedReadyTime", "EstimatedReadyTime > OrderedAt");

        // Configuration des données par défaut
        builder.Property(k => k.IsRush).HasDefaultValue(false);
        builder.Property(k => k.IsComplicated).HasDefaultValue(false);
        builder.Property(k => k.RequiresSpecialAttention).HasDefaultValue(false);
        builder.Property(k => k.RequiresAllergenAttention).HasDefaultValue(false);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité KitchenOrderItem
/// </summary>
public class KitchenOrderItemConfiguration : IEntityTypeConfiguration<KitchenOrderItem>
{
    public void Configure(EntityTypeBuilder<KitchenOrderItem> builder)
    {
        // Configuration de la table
        builder.ToTable("KitchenOrderItems");
        
        // Clé primaire
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(i => i.KitchenOrderId)
            .HasColumnName("KitchenOrderId")
            .IsRequired();

        builder.Property(i => i.MenuItemId)
            .HasColumnName("MenuItemId")
            .IsRequired();

        builder.Property(i => i.MenuItemName)
            .HasColumnName("MenuItemName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnName("UnitPrice")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(i => i.SpecialRequests)
            .HasColumnName("SpecialRequests")
            .HasMaxLength(1000);

        builder.Property(i => i.RequiredKitchenSection)
            .HasColumnName("RequiredKitchenSection")
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(i => i.IsComplicated)
            .HasColumnName("IsComplicated")
            .HasDefaultValue(false);

        builder.Property(i => i.IsUrgent)
            .HasColumnName("IsUrgent")
            .HasDefaultValue(false);

        // Collections
        builder.Property(i => i.Allergens)
            .HasConversion(
                v => string.Join(',', v.Select(a => (int)a)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (AllergenType)int.Parse(s))
                      .ToList())
            .HasColumnName("Allergens")
            .HasMaxLength(500);

        builder.Property(i => i.DietaryRequirements)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("DietaryRequirements")
            .HasMaxLength(1000);

        builder.Property(i => i.Modifications)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("Modifications")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(i => i.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(i => i.KitchenOrderId)
            .HasDatabaseName("IX_KitchenOrderItems_KitchenOrderId");

        builder.HasIndex(i => i.MenuItemId)
            .HasDatabaseName("IX_KitchenOrderItems_MenuItemId");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_KitchenOrderItems_Status");

        builder.HasIndex(i => i.RequiredKitchenSection)
            .HasDatabaseName("IX_KitchenOrderItems_RequiredKitchenSection");

        // Relations
        builder.HasOne<KitchenOrder>()
            .WithMany()
            .HasForeignKey(i => i.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderItems_KitchenOrders");

        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(i => i.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_KitchenOrderItems_MenuItems");

        // Contraintes
        builder.HasCheckConstraint("CK_KitchenOrderItems_Quantity", "Quantity > 0");
        builder.HasCheckConstraint("CK_KitchenOrderItems_UnitPrice", "UnitPrice >= 0");

        // Configuration des données par défaut
        builder.Property(i => i.IsComplicated).HasDefaultValue(false);
        builder.Property(i => i.IsUrgent).HasDefaultValue(false);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité KitchenOrderLog
/// </summary>
public class KitchenOrderLogConfiguration : IEntityTypeConfiguration<KitchenOrderLog>
{
    public void Configure(EntityTypeBuilder<KitchenOrderLog> builder)
    {
        // Configuration de la table
        builder.ToTable("KitchenOrderLogs");
        
        // Clé primaire
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(l => l.KitchenOrderId)
            .HasColumnName("KitchenOrderId")
            .IsRequired();

        builder.Property(l => l.Action)
            .HasColumnName("Action")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(l => l.PerformedAt)
            .HasColumnName("PerformedAt")
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(l => l.PerformedBy)
            .HasColumnName("PerformedBy");

        // Timestamps
        builder.Property(l => l.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Index
        builder.HasIndex(l => l.KitchenOrderId)
            .HasDatabaseName("IX_KitchenOrderLogs_KitchenOrderId");

        builder.HasIndex(l => l.PerformedAt)
            .HasDatabaseName("IX_KitchenOrderLogs_PerformedAt");

        builder.HasIndex(l => l.Action)
            .HasDatabaseName("IX_KitchenOrderLogs_Action");

        builder.HasIndex(l => l.PerformedBy)
            .HasDatabaseName("IX_KitchenOrderLogs_PerformedBy");

        // Relations
        builder.HasOne<KitchenOrder>()
            .WithMany()
            .HasForeignKey(l => l.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderLogs_KitchenOrders");
    }
}

/// <summary>
/// Configuration EF Core pour l'entité KitchenOrderModification
/// </summary>
public class KitchenOrderModificationConfiguration : IEntityTypeConfiguration<KitchenOrderModification>
{
    public void Configure(EntityTypeBuilder<KitchenOrderModification> builder)
    {
        // Configuration de la table
        builder.ToTable("KitchenOrderModifications");
        
        // Clé primaire
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(m => m.KitchenOrderId)
            .HasColumnName("KitchenOrderId")
            .IsRequired();

        builder.Property(m => m.ModifiedAt)
            .HasColumnName("ModifiedAt")
            .IsRequired();

        builder.Property(m => m.Reason)
            .HasColumnName("Reason")
            .HasMaxLength(500)
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(m => m.ItemId)
            .HasColumnName("ItemId");

        builder.Property(m => m.OldQuantity)
            .HasColumnName("OldQuantity");

        builder.Property(m => m.NewQuantity)
            .HasColumnName("NewQuantity");

        builder.Property(m => m.OldSpecialRequests)
            .HasColumnName("OldSpecialRequests")
            .HasMaxLength(1000);

        builder.Property(m => m.NewSpecialRequests)
            .HasColumnName("NewSpecialRequests")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(m => m.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Index
        builder.HasIndex(m => m.KitchenOrderId)
            .HasDatabaseName("IX_KitchenOrderModifications_KitchenOrderId");

        builder.HasIndex(m => m.ItemId)
            .HasDatabaseName("IX_KitchenOrderModifications_ItemId");

        builder.HasIndex(m => m.ModifiedAt)
            .HasDatabaseName("IX_KitchenOrderModifications_ModifiedAt");

        // Relations
        builder.HasOne<KitchenOrder>()
            .WithMany()
            .HasForeignKey(m => m.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_KitchenOrderModifications_KitchenOrders");

        // Contraintes
        builder.HasCheckConstraint("CK_KitchenOrderModifications_OldQuantity", "OldQuantity IS NULL OR OldQuantity > 0");
        builder.HasCheckConstraint("CK_KitchenOrderModifications_NewQuantity", "NewQuantity IS NULL OR NewQuantity > 0");
    }
}