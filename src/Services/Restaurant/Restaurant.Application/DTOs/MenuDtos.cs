using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

/// <summary>
/// DTO pour créer un menu
/// </summary>
public record CreateMenuDto(
    string Name,
    MenuType MenuType,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    TimeOnly? AvailableFromTime = null,
    TimeOnly? AvailableUntilTime = null,
    List<DayOfWeek>? AvailableDays = null,
    decimal? BasePrice = null,
    bool IsActive = true);

/// <summary>
/// DTO pour mettre à jour un menu
/// </summary>
public record UpdateMenuDto(
    string? Name = null,
    string? Description = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null,
    TimeOnly? AvailableFromTime = null,
    TimeOnly? AvailableUntilTime = null,
    List<DayOfWeek>? AvailableDays = null,
    decimal? BasePrice = null);

/// <summary>
/// DTO de réponse pour un menu
/// </summary>
public record MenuDto(
    Guid Id,
    string Name,
    MenuType MenuType,
    MenuStatus Status,
    string? Description,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    TimeOnly? AvailableFromTime,
    TimeOnly? AvailableUntilTime,
    List<DayOfWeek> AvailableDays,
    decimal? BasePrice,
    bool IsActive,
    bool IsCurrentlyAvailable,
    int TotalItems,
    int AvailableItems,
    int TotalSections,
    List<MenuSectionDto> Sections,
    MenuStatisticsDto Statistics,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO pour section de menu
/// </summary>
public record MenuSectionDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    int ItemCount,
    List<MenuItemSummaryDto> Items);

/// <summary>
/// DTO pour créer une section de menu
/// </summary>
public record CreateMenuSectionDto(
    string Name,
    string? Description = null,
    int DisplayOrder = 0);

/// <summary>
/// DTO pour statistiques de menu
/// </summary>
public record MenuStatisticsDto(
    int TotalOrders,
    decimal TotalRevenue,
    decimal AverageOrderValue,
    decimal PopularityScore,
    MenuItemSummaryDto? MostPopularItem,
    MenuItemSummaryDto? HighestRevenueItem,
    DateTime? LastOrdered,
    Dictionary<MenuCategory, int> CategoryDistribution,
    Dictionary<DayOfWeek, int> OrdersByDay);

/// <summary>
/// DTO pour créer un élément de menu
/// </summary>
public record CreateMenuItemDto(
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
    List<DayOfWeek>? AvailableDays = null);

/// <summary>
/// DTO pour mettre à jour un élément de menu
/// </summary>
public record UpdateMenuItemDto(
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
    bool? IsSpicy = null);

/// <summary>
/// DTO de réponse pour un élément de menu
/// </summary>
public record MenuItemDto(
    Guid Id,
    Guid MenuId,
    Guid? SectionId,
    string Name,
    string? Description,
    MenuCategory Category,
    MenuPrice Price,
    MenuItemStatus Status,
    PreparationTime PreparationTime,
    string? ImageUrl,
    string? Ingredients,
    string? ServingSize,
    List<AllergenType> Allergens,
    List<string> DietaryRestrictions,
    NutritionalInfoDto? NutritionalInfo,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    bool IsSpicy,
    bool IsAvailable,
    bool IsPopular,
    bool IsNew,
    bool RequiresAgeVerification,
    bool IsCurrentlyAvailable,
    TimeOnly? AvailableFromTime,
    TimeOnly? AvailableUntilTime,
    List<DayOfWeek> AvailableDays,
    int OrderCount,
    decimal? AverageRating,
    int ReviewCount,
    decimal Revenue,
    DateTime? LastOrdered,
    List<MenuItemVariationDto> Variations,
    List<MenuItemPromotionDto> Promotions,
    MenuPrice EffectivePrice,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// DTO simplifié pour élément de menu
/// </summary>
public record MenuItemSummaryDto(
    Guid Id,
    string Name,
    MenuCategory Category,
    MenuPrice Price,
    MenuItemStatus Status,
    bool IsAvailable,
    bool IsPopular,
    bool IsNew,
    string? ImageUrl,
    decimal? AverageRating,
    int OrderCount);

/// <summary>
/// DTO pour variation d'élément de menu
/// </summary>
public record MenuItemVariationDto(
    Guid Id,
    string Name,
    string? Description,
    MenuPrice Price,
    bool IsAvailable,
    NutritionalInfoDto? NutritionalInfo);

/// <summary>
/// DTO pour créer une variation
/// </summary>
public record CreateMenuItemVariationDto(
    string Name,
    decimal Price,
    string Currency,
    string? Description = null,
    NutritionalInfoDto? NutritionalInfo = null);

/// <summary>
/// DTO pour promotion d'élément de menu
/// </summary>
public record MenuItemPromotionDto(
    Guid Id,
    string Name,
    string? Description,
    MenuPrice PromotionalPrice,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool IsCurrentlyActive);

/// <summary>
/// DTO pour créer une promotion
/// </summary>
public record CreateMenuItemPromotionDto(
    string Name,
    decimal PromotionalPrice,
    string Currency,
    DateTime StartDate,
    DateTime EndDate,
    string? Description = null);

/// <summary>
/// DTO pour informations nutritionnelles
/// </summary>
public record NutritionalInfoDto(
    int Calories,
    decimal Protein,
    decimal Carbohydrates,
    decimal Fat,
    decimal Fiber,
    decimal Sugar,
    decimal Sodium,
    string Unit = "g");

/// <summary>
/// DTO pour prix de menu
/// </summary>
public record MenuPrice(
    decimal Amount,
    string Currency);

/// <summary>
/// DTO pour temps de préparation
/// </summary>
public record PreparationTime(
    int TotalMinutes,
    int ActiveMinutes);

/// <summary>
/// DTO pour évaluation
/// </summary>
public record AddMenuItemRatingDto(
    decimal Score,
    string? Comment = null);

/// <summary>
/// DTO pour recherche d'éléments de menu
/// </summary>
public record MenuItemSearchDto(
    string? Name = null,
    MenuCategory? Category = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsVegetarian = null,
    bool? IsVegan = null,
    bool? IsGlutenFree = null,
    bool? IsSpicy = null,
    List<AllergenType>? ExcludeAllergens = null,
    bool? IsAvailable = null,
    bool? IsPopular = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageSize = 20,
    int PageNumber = 1);

/// <summary>
/// DTO pour résultat de recherche d'éléments de menu
/// </summary>
public record PagedMenuItemResultDto(
    List<MenuItemSummaryDto> Items,
    int TotalCount,
    int PageSize,
    int PageNumber,
    int TotalPages,
    Dictionary<MenuCategory, int> CategoryCounts,
    Dictionary<string, int> FilterCounts);