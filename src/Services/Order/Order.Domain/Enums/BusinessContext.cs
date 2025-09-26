namespace Order.Domain.Enums;

/// <summary>
/// Contextes métier supportés par le système Order Enterprise
/// Aligné sur la vision NiesPro ERP de très haut standing
/// </summary>
public enum BusinessContext
{
    /// <summary>
    /// Commerce électronique - Commandes en ligne avec livraison
    /// Workflow: Order → Payment → Fulfillment → Shipping → Delivery
    /// </summary>
    ECommerce = 1,

    /// <summary>
    /// Restaurant & Food Service - Service sur place et à emporter  
    /// Workflow: Reception → Kitchen → Preparation → Ready → Served
    /// </summary>
    Restaurant = 2,

    /// <summary>
    /// Boutique & Retail - Vente comptoir et POS
    /// Workflow: Scan → Payment → Receipt → Completion
    /// </summary>
    Boutique = 3,

    /// <summary>
    /// Commerce de gros - Commandes B2B volumineuses (futur)
    /// Workflow: Quote → Approval → Bulk_Fulfillment → Delivery
    /// </summary>
    Wholesale = 4
}

/// <summary>
/// Extensions pour BusinessContext - Logic métier Enterprise
/// </summary>
public static class BusinessContextExtensions
{
    /// <summary>
    /// Obtient les statuts valides pour un contexte métier donné
    /// </summary>
    public static IReadOnlyList<OrderStatus> GetValidStatuses(this BusinessContext context)
    {
        return context switch
        {
            BusinessContext.ECommerce => new[]
            {
                OrderStatus.Pending,
                OrderStatus.Confirmed,
                OrderStatus.Processing,
                OrderStatus.Shipped,
                OrderStatus.Delivered,
                OrderStatus.Cancelled,
                OrderStatus.Refunded
            },
            
            BusinessContext.Restaurant => new[]
            {
                OrderStatus.Pending,
                OrderStatus.Confirmed,
                OrderStatus.KitchenQueue,
                OrderStatus.Cooking,
                OrderStatus.Ready,
                OrderStatus.Served,
                OrderStatus.Cancelled
            },
            
            BusinessContext.Boutique => new[]
            {
                OrderStatus.Pending,
                OrderStatus.Scanned,
                OrderStatus.Paid,
                OrderStatus.Receipted,
                OrderStatus.Completed,
                OrderStatus.Cancelled
            },
            
            BusinessContext.Wholesale => new[]
            {
                OrderStatus.Pending,
                OrderStatus.QuoteRequested,
                OrderStatus.Approved,
                OrderStatus.BulkProcessing,
                OrderStatus.Delivered,
                OrderStatus.Cancelled
            },
            
            _ => throw new ArgumentOutOfRangeException(nameof(context))
        };
    }

    /// <summary>
    /// Détermine si un contexte nécessite une intégration Kitchen
    /// </summary>
    public static bool RequiresKitchenIntegration(this BusinessContext context)
    {
        return context == BusinessContext.Restaurant;
    }

    /// <summary>
    /// Détermine si un contexte nécessite une intégration POS
    /// </summary>
    public static bool RequiresPOSIntegration(this BusinessContext context)
    {
        return context == BusinessContext.Boutique;
    }

    /// <summary>
    /// Détermine si un contexte nécessite une gestion de livraison
    /// </summary>
    public static bool RequiresShippingManagement(this BusinessContext context)
    {
        return context is BusinessContext.ECommerce or BusinessContext.Wholesale;
    }

    /// <summary>
    /// Obtient le nom d'affichage localisé du contexte
    /// </summary>
    public static string GetDisplayName(this BusinessContext context)
    {
        return context switch
        {
            BusinessContext.ECommerce => "E-Commerce",
            BusinessContext.Restaurant => "Restaurant",
            BusinessContext.Boutique => "Boutique",
            BusinessContext.Wholesale => "Commerce de Gros",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtient la description détaillée du contexte
    /// </summary>
    public static string GetDescription(this BusinessContext context)
    {
        return context switch
        {
            BusinessContext.ECommerce => "Commandes en ligne avec livraison à domicile",
            BusinessContext.Restaurant => "Service de restauration sur place et à emporter",
            BusinessContext.Boutique => "Vente au comptoir et point de vente",
            BusinessContext.Wholesale => "Commerce de gros et ventes B2B",
            _ => "Contexte métier non défini"
        };
    }
}