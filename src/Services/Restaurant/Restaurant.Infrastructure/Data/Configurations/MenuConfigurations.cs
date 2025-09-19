using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Menu
/// </summary>
public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        // Configuration de la table
        builder.ToTable("Menus");
        
        // Clé primaire
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(m => m.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.MenuType)
            .HasColumnName("MenuType")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(m => m.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000);

        builder.Property(m => m.ValidFrom)
            .HasColumnName("ValidFrom");

        builder.Property(m => m.ValidUntil)
            .HasColumnName("ValidUntil");

        builder.Property(m => m.AvailableFromTime)
            .HasColumnName("AvailableFromTime");

        builder.Property(m => m.AvailableUntilTime)
            .HasColumnName("AvailableUntilTime");

        builder.Property(m => m.BasePrice)
            .HasColumnName("BasePrice")
            .HasPrecision(10, 2);

        builder.Property(m => m.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        // Collections
        builder.Property(m => m.AvailableDays)
            .HasConversion(
                v => string.Join(',', v.Select(d => (int)d)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (DayOfWeek)int.Parse(s))
                      .ToList())
            .HasColumnName("AvailableDays")
            .HasMaxLength(50);

        // Timestamps
        builder.Property(m => m.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(m => m.Name)
            .HasDatabaseName("IX_Menus_Name");

        builder.HasIndex(m => m.MenuType)
            .HasDatabaseName("IX_Menus_MenuType");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_Menus_Status");

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_Menus_IsActive");

        builder.HasIndex(m => new { m.MenuType, m.IsActive })
            .HasDatabaseName("IX_Menus_MenuType_IsActive");

        builder.HasIndex(m => new { m.ValidFrom, m.ValidUntil })
            .HasDatabaseName("IX_Menus_ValidPeriod");

        // Relations
        builder.HasMany<MenuItem>()
            .WithOne()
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItems_Menus");

        // Contraintes
        builder.HasCheckConstraint("CK_Menus_BasePrice", "BasePrice IS NULL OR BasePrice >= 0");
        builder.HasCheckConstraint("CK_Menus_ValidPeriod", "ValidUntil IS NULL OR ValidFrom IS NULL OR ValidUntil > ValidFrom");

        // Configuration des données par défaut
        builder.Property(m => m.IsActive).HasDefaultValue(true);
        builder.Property(m => m.Status).HasDefaultValue(MenuStatus.Draft);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité MenuItem
/// </summary>
public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        // Configuration de la table
        builder.ToTable("MenuItems");
        
        // Clé primaire
        builder.HasKey(mi => mi.Id);
        builder.Property(mi => mi.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(mi => mi.MenuId)
            .HasColumnName("MenuId")
            .IsRequired();

        builder.Property(mi => mi.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mi => mi.Category)
            .HasColumnName("Category")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(mi => mi.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        // Propriétés optionnelles
        builder.Property(mi => mi.SectionId)
            .HasColumnName("SectionId");

        builder.Property(mi => mi.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000);

        builder.Property(mi => mi.ImageUrl)
            .HasColumnName("ImageUrl")
            .HasMaxLength(500);

        builder.Property(mi => mi.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .HasDefaultValue(0);

        // Propriétés de disponibilité
        builder.Property(mi => mi.IsAvailable)
            .HasColumnName("IsAvailable")
            .HasDefaultValue(true);

        builder.Property(mi => mi.IsPopular)
            .HasColumnName("IsPopular")
            .HasDefaultValue(false);

        builder.Property(mi => mi.IsNew)
            .HasColumnName("IsNew")
            .HasDefaultValue(false);

        builder.Property(mi => mi.IsSpicy)
            .HasColumnName("IsSpicy")
            .HasDefaultValue(false);

        builder.Property(mi => mi.IsVegetarian)
            .HasColumnName("IsVegetarian")
            .HasDefaultValue(false);

        builder.Property(mi => mi.IsVegan)
            .HasColumnName("IsVegan")
            .HasDefaultValue(false);

        builder.Property(mi => mi.IsGlutenFree)
            .HasColumnName("IsGlutenFree")
            .HasDefaultValue(false);

        builder.Property(mi => mi.RequiresAgeVerification)
            .HasColumnName("RequiresAgeVerification")
            .HasDefaultValue(false);

        // Allergènes et régimes
        builder.Property(mi => mi.Allergens)
            .HasConversion(
                v => string.Join(',', v.Select(a => (int)a)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (AllergenType)int.Parse(s))
                      .ToList())
            .HasColumnName("Allergens")
            .HasMaxLength(500);

        builder.Property(mi => mi.DietaryRestrictions)
            .HasConversion(
                v => string.Join(';', v),
                v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("DietaryRestrictions")
            .HasMaxLength(1000);

        builder.Property(mi => mi.ServingSize)
            .HasColumnName("ServingSize")
            .HasMaxLength(100);

        builder.Property(mi => mi.Ingredients)
            .HasColumnName("Ingredients")
            .HasMaxLength(2000);

        // Horaires de disponibilité
        builder.Property(mi => mi.AvailableFromTime)
            .HasColumnName("AvailableFromTime");

        builder.Property(mi => mi.AvailableUntilTime)
            .HasColumnName("AvailableUntilTime");

        builder.Property(mi => mi.AvailableDays)
            .HasConversion(
                v => string.Join(',', v.Select(d => (int)d)),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => (DayOfWeek)int.Parse(s))
                      .ToList())
            .HasColumnName("AvailableDays")
            .HasMaxLength(50);

        // Métriques et popularité
        builder.Property(mi => mi.OrderCount)
            .HasColumnName("OrderCount")
            .HasDefaultValue(0);

        builder.Property(mi => mi.AverageRating)
            .HasColumnName("AverageRating")
            .HasPrecision(3, 2);

        builder.Property(mi => mi.ReviewCount)
            .HasColumnName("ReviewCount")
            .HasDefaultValue(0);

        builder.Property(mi => mi.LastOrdered)
            .HasColumnName("LastOrdered");

        builder.Property(mi => mi.Revenue)
            .HasColumnName("Revenue")
            .HasPrecision(12, 2)
            .HasDefaultValue(0);

        // Gestion des stocks et ingrédients
        builder.Property(mi => mi.RequiredStockItems)
            .HasConversion(
                v => string.Join(',', v.Select(id => id.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => Guid.Parse(s))
                      .ToList())
            .HasColumnName("RequiredStockItems")
            .HasMaxLength(2000);

        builder.Property(mi => mi.RequiresStockCheck)
            .HasColumnName("RequiresStockCheck")
            .HasDefaultValue(true);

        builder.Property(mi => mi.MinimumStockLevel)
            .HasColumnName("MinimumStockLevel");

        // Personnalisation
        builder.Property(mi => mi.AllowsCustomization)
            .HasColumnName("AllowsCustomization")
            .HasDefaultValue(true);

        builder.Property(mi => mi.CustomizationInstructions)
            .HasColumnName("CustomizationInstructions")
            .HasMaxLength(1000);

        builder.Property(mi => mi.CustomizationUpcharge)
            .HasColumnName("CustomizationUpcharge")
            .HasPrecision(8, 2);

        // Timestamps
        builder.Property(mi => mi.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(mi => mi.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(mi => mi.MenuId)
            .HasDatabaseName("IX_MenuItems_MenuId");

        builder.HasIndex(mi => mi.SectionId)
            .HasDatabaseName("IX_MenuItems_SectionId");

        builder.HasIndex(mi => mi.Category)
            .HasDatabaseName("IX_MenuItems_Category");

        builder.HasIndex(mi => mi.Status)
            .HasDatabaseName("IX_MenuItems_Status");

        builder.HasIndex(mi => mi.IsAvailable)
            .HasDatabaseName("IX_MenuItems_IsAvailable");

        builder.HasIndex(mi => mi.IsPopular)
            .HasDatabaseName("IX_MenuItems_IsPopular");

        builder.HasIndex(mi => mi.IsNew)
            .HasDatabaseName("IX_MenuItems_IsNew");

        builder.HasIndex(mi => new { mi.MenuId, mi.Category })
            .HasDatabaseName("IX_MenuItems_Menu_Category");

        builder.HasIndex(mi => new { mi.MenuId, mi.SectionId })
            .HasDatabaseName("IX_MenuItems_Menu_Section");

        builder.HasIndex(mi => new { mi.Status, mi.IsAvailable })
            .HasDatabaseName("IX_MenuItems_Status_Available");

        // Relations
        builder.HasOne<Menu>()
            .WithMany()
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItems_Menus");

        builder.HasMany<MenuItemVariation>()
            .WithOne()
            .HasForeignKey(v => v.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItemVariations_MenuItems");

        builder.HasMany<MenuItemPromotion>()
            .WithOne()
            .HasForeignKey(p => p.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItemPromotions_MenuItems");

        // Contraintes
        builder.HasCheckConstraint("CK_MenuItems_OrderCount", "OrderCount >= 0");
        builder.HasCheckConstraint("CK_MenuItems_ReviewCount", "ReviewCount >= 0");
        builder.HasCheckConstraint("CK_MenuItems_Revenue", "Revenue >= 0");
        builder.HasCheckConstraint("CK_MenuItems_AverageRating", "AverageRating IS NULL OR (AverageRating >= 0 AND AverageRating <= 5)");
        builder.HasCheckConstraint("CK_MenuItems_MinimumStockLevel", "MinimumStockLevel IS NULL OR MinimumStockLevel >= 0");
        builder.HasCheckConstraint("CK_MenuItems_CustomizationUpcharge", "CustomizationUpcharge IS NULL OR CustomizationUpcharge >= 0");

        // Configuration des données par défaut
        builder.Property(mi => mi.IsAvailable).HasDefaultValue(true);
        builder.Property(mi => mi.IsPopular).HasDefaultValue(false);
        builder.Property(mi => mi.IsNew).HasDefaultValue(false);
        builder.Property(mi => mi.IsSpicy).HasDefaultValue(false);
        builder.Property(mi => mi.IsVegetarian).HasDefaultValue(false);
        builder.Property(mi => mi.IsVegan).HasDefaultValue(false);
        builder.Property(mi => mi.IsGlutenFree).HasDefaultValue(false);
        builder.Property(mi => mi.RequiresAgeVerification).HasDefaultValue(false);
        builder.Property(mi => mi.RequiresStockCheck).HasDefaultValue(true);
        builder.Property(mi => mi.AllowsCustomization).HasDefaultValue(true);
        builder.Property(mi => mi.OrderCount).HasDefaultValue(0);
        builder.Property(mi => mi.ReviewCount).HasDefaultValue(0);
        builder.Property(mi => mi.Revenue).HasDefaultValue(0m);
        builder.Property(mi => mi.DisplayOrder).HasDefaultValue(0);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité MenuItemVariation
/// </summary>
public class MenuItemVariationConfiguration : IEntityTypeConfiguration<MenuItemVariation>
{
    public void Configure(EntityTypeBuilder<MenuItemVariation> builder)
    {
        // Configuration de la table
        builder.ToTable("MenuItemVariations");
        
        // Clé primaire
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(v => v.MenuItemId)
            .HasColumnName("MenuItemId")
            .IsRequired();

        builder.Property(v => v.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(v => v.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.Property(v => v.IsAvailable)
            .HasColumnName("IsAvailable")
            .HasDefaultValue(true);

        builder.Property(v => v.DisplayOrder)
            .HasColumnName("DisplayOrder")
            .HasDefaultValue(0);

        // Timestamps
        builder.Property(v => v.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(v => v.MenuItemId)
            .HasDatabaseName("IX_MenuItemVariations_MenuItemId");

        builder.HasIndex(v => new { v.MenuItemId, v.Name })
            .IsUnique()
            .HasDatabaseName("IX_MenuItemVariations_MenuItem_Name");

        // Relations
        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(v => v.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItemVariations_MenuItems");

        // Configuration des données par défaut
        builder.Property(v => v.IsAvailable).HasDefaultValue(true);
        builder.Property(v => v.DisplayOrder).HasDefaultValue(0);
    }
}

/// <summary>
/// Configuration EF Core pour l'entité MenuItemPromotion
/// </summary>
public class MenuItemPromotionConfiguration : IEntityTypeConfiguration<MenuItemPromotion>
{
    public void Configure(EntityTypeBuilder<MenuItemPromotion> builder)
    {
        // Configuration de la table
        builder.ToTable("MenuItemPromotions");
        
        // Clé primaire
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .IsRequired();

        // Propriétés requises
        builder.Property(p => p.MenuItemId)
            .HasColumnName("MenuItemId")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.StartDate)
            .HasColumnName("StartDate")
            .IsRequired();

        builder.Property(p => p.EndDate)
            .HasColumnName("EndDate")
            .IsRequired();

        // Propriétés optionnelles
        builder.Property(p => p.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(p => p.EndedAt)
            .HasColumnName("EndedAt");

        // Timestamps
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Index
        builder.HasIndex(p => p.MenuItemId)
            .HasDatabaseName("IX_MenuItemPromotions_MenuItemId");

        builder.HasIndex(p => new { p.StartDate, p.EndDate })
            .HasDatabaseName("IX_MenuItemPromotions_Period");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_MenuItemPromotions_IsActive");

        // Relations
        builder.HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(p => p.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_MenuItemPromotions_MenuItems");

        // Contraintes
        builder.HasCheckConstraint("CK_MenuItemPromotions_Period", "EndDate > StartDate");

        // Configuration des données par défaut
        builder.Property(p => p.IsActive).HasDefaultValue(true);
    }
}