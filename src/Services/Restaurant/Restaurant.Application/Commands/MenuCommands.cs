using MediatR;
using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Commands;

/// <summary>
/// Commande pour créer un menu
/// </summary>
public record CreateMenuCommand(
    string Name,
    MenuType MenuType,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    TimeOnly? AvailableFromTime = null,
    TimeOnly? AvailableUntilTime = null,
    List<DayOfWeek>? AvailableDays = null,
    decimal? BasePrice = null,
    bool IsActive = true) : IRequest<MenuDto>;

/// <summary>
/// Commande pour mettre à jour un menu
/// </summary>
public record UpdateMenuCommand(
    Guid MenuId,
    string? Name = null,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    TimeOnly? AvailableFromTime = null,
    TimeOnly? AvailableUntilTime = null,
    List<DayOfWeek>? AvailableDays = null,
    decimal? BasePrice = null) : IRequest<MenuDto>;

/// <summary>
/// Commande pour activer/désactiver un menu
/// </summary>
public record ToggleMenuStatusCommand(
    Guid MenuId,
    bool IsActive,
    string? Reason = null) : IRequest<MenuDto>;

/// <summary>
/// Commande pour dupliquer un menu
/// </summary>
public record DuplicateMenuCommand(
    Guid MenuId,
    string NewName,
    MenuType? NewMenuType = null) : IRequest<MenuDto>;

/// <summary>
/// Commande pour supprimer un menu
/// </summary>
public record DeleteMenuCommand(Guid MenuId) : IRequest<bool>;

/// <summary>
/// Commande pour créer une section de menu
/// </summary>
public record CreateMenuSectionCommand(
    Guid MenuId,
    string Name,
    string? Description = null,
    int DisplayOrder = 0) : IRequest<MenuSectionDto>;

/// <summary>
/// Commande pour mettre à jour une section de menu
/// </summary>
public record UpdateMenuSectionCommand(
    Guid MenuId,
    Guid SectionId,
    string? Name = null,
    string? Description = null,
    int? DisplayOrder = null) : IRequest<MenuSectionDto>;

/// <summary>
/// Commande pour supprimer une section de menu
/// </summary>
public record DeleteMenuSectionCommand(
    Guid MenuId,
    Guid SectionId) : IRequest<bool>;

/// <summary>
/// Commande pour créer un élément de menu
/// </summary>
public record CreateMenuItemCommand(
    Guid MenuId,
    string Name,
    MenuCategory Category,
    decimal Price,
    string Currency,
    int PreparationTimeMinutes,
    int ActiveTimeMinutes,
    string? Description = null,
    Guid? SectionId = null,
    string? ImageUrl = null,
    string? Ingredients = null,
    string? ServingSize = null,
    List<AllergenType>? Allergens = null,
    List<string>? DietaryRestrictions = null,
    NutritionalInfoDto? NutritionalInfo = null,
    bool IsVegetarian = false,
    bool IsVegan = false,
    bool IsGlutenFree = false,
    bool IsSpicy = false,
    TimeOnly? AvailableFromTime = null,
    TimeOnly? AvailableUntilTime = null,
    List<DayOfWeek>? AvailableDays = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour mettre à jour un élément de menu
/// </summary>
public record UpdateMenuItemCommand(
    Guid MenuItemId,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    string? Currency = null,
    string? ImageUrl = null,
    string? Ingredients = null,
    string? ServingSize = null,
    List<AllergenType>? Allergens = null,
    List<string>? DietaryRestrictions = null,
    NutritionalInfoDto? NutritionalInfo = null,
    bool? IsVegetarian = null,
    bool? IsVegan = null,
    bool? IsGlutenFree = null,
    bool? IsSpicy = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour changer le statut d'un élément de menu
/// </summary>
public record ChangeMenuItemStatusCommand(
    Guid MenuItemId,
    MenuItemStatus Status,
    string? Reason = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour mettre à jour le prix d'un élément
/// </summary>
public record UpdateMenuItemPriceCommand(
    Guid MenuItemId,
    decimal NewPrice,
    string Currency,
    string? Reason = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour supprimer un élément de menu
/// </summary>
public record DeleteMenuItemCommand(Guid MenuItemId) : IRequest<bool>;

/// <summary>
/// Commande pour créer une variation d'élément
/// </summary>
public record CreateMenuItemVariationCommand(
    Guid MenuItemId,
    string Name,
    decimal Price,
    string Currency,
    string? Description = null,
    NutritionalInfoDto? NutritionalInfo = null) : IRequest<MenuItemVariationDto>;

/// <summary>
/// Commande pour supprimer une variation
/// </summary>
public record DeleteMenuItemVariationCommand(
    Guid MenuItemId,
    Guid VariationId) : IRequest<bool>;

/// <summary>
/// Commande pour créer une promotion
/// </summary>
public record CreateMenuItemPromotionCommand(
    Guid MenuItemId,
    string Name,
    decimal PromotionalPrice,
    string Currency,
    DateTime StartDate,
    DateTime EndDate,
    string? Description = null) : IRequest<MenuItemPromotionDto>;

/// <summary>
/// Commande pour terminer une promotion
/// </summary>
public record EndMenuItemPromotionCommand(
    Guid MenuItemId,
    Guid PromotionId) : IRequest<bool>;

/// <summary>
/// Commande pour marquer un élément comme nouveau
/// </summary>
public record MarkMenuItemAsNewCommand(
    Guid MenuItemId,
    bool IsNew = true) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour évaluer un élément de menu
/// </summary>
public record RateMenuItemCommand(
    Guid MenuItemId,
    decimal Score,
    string? Comment = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour enregistrer une commande d'élément
/// </summary>
public record RecordMenuItemOrderCommand(
    Guid MenuItemId,
    decimal OrderValue) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour configurer la gestion des stocks
/// </summary>
public record ConfigureMenuItemStockCommand(
    Guid MenuItemId,
    List<Guid> RequiredStockItems,
    bool RequiresStockCheck = true,
    int? MinimumStockLevel = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour configurer les horaires de disponibilité
/// </summary>
public record ConfigureMenuItemAvailabilityCommand(
    Guid MenuItemId,
    TimeOnly? FromTime = null,
    TimeOnly? UntilTime = null,
    List<DayOfWeek>? AvailableDays = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour configurer la personnalisation
/// </summary>
public record ConfigureMenuItemCustomizationCommand(
    Guid MenuItemId,
    bool AllowsCustomization = true,
    string? CustomizationInstructions = null,
    decimal? CustomizationUpcharge = null) : IRequest<MenuItemDto>;

/// <summary>
/// Commande pour importer un menu depuis un fichier
/// </summary>
public record ImportMenuCommand(
    MenuType MenuType,
    byte[] FileContent,
    string FileName,
    ImportFormat Format) : IRequest<MenuDto>;

/// <summary>
/// Commande pour exporter un menu
/// </summary>
public record ExportMenuCommand(
    Guid MenuId,
    ExportFormat Format) : IRequest<byte[]>;

/// <summary>
/// Commande pour analyser la performance du menu
/// </summary>
public record AnalyzeMenuPerformanceCommand(
    Guid MenuId,
    DateTime StartDate,
    DateTime EndDate) : IRequest<MenuPerformanceAnalysisDto>;

/// <summary>
/// Format d'import/export
/// </summary>
public enum ImportFormat
{
    Json,
    Csv,
    Excel,
    Xml
}

/// <summary>
/// Format d'export
/// </summary>
public enum ExportFormat
{
    Json,
    Csv,
    Excel,
    Pdf
}

/// <summary>
/// DTO pour analyse de performance de menu
/// </summary>
public record MenuPerformanceAnalysisDto(
    Guid MenuId,
    string MenuName,
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    List<MenuItemPerformanceDto> TopPerformingItems,
    List<MenuItemPerformanceDto> UnderperformingItems,
    Dictionary<MenuCategory, decimal> RevenueByCategory,
    Dictionary<DateTime, decimal> DailyRevenue,
    List<string> Recommendations);

/// <summary>
/// DTO pour performance d'élément de menu
/// </summary>
public record MenuItemPerformanceDto(
    Guid Id,
    string Name,
    int OrderCount,
    decimal Revenue,
    decimal Margin,
    decimal PopularityScore,
    string PerformanceCategory);