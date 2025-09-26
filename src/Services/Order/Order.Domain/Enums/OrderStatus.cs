namespace Order.Domain.Enums;

/// <summary>
/// Statuts de commande unifiés pour tous contextes métier Enterprise
/// Architecture multi-contexte alignée sur la vision NiesPro ERP
/// </summary>
public enum OrderStatus
{
    // Statuts communs à tous contextes
    Pending = 1,
    Confirmed = 2,
    Cancelled = 50,
    
    // E-Commerce workflow
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Refunded = 6,
    Failed = 7,
    
    // Restaurant workflow  
    KitchenQueue = 10,
    Cooking = 11,
    Ready = 12,
    Served = 13,
    
    // Boutique workflow
    Scanned = 20,
    Paid = 21,
    Receipted = 22,
    Completed = 23,
    
    // Wholesale workflow (futur)
    QuoteRequested = 30,
    Approved = 31,
    BulkProcessing = 32,
    
    // Statuts d'exception
    OnHold = 40,
    PartiallyFulfilled = 41
}

/// <summary>
/// Extensions Enterprise pour OrderStatus - Logique métier avancée
/// Gestion multi-contexte avec règles de transition sophistiquées
/// </summary>
public static class OrderStatusExtensions
{
    /// <summary>
    /// Détermine si une transition est valide selon le contexte métier
    /// Architecture enterprise avec règles métier complexes
    /// </summary>
    public static bool CanTransitionTo(this OrderStatus current, OrderStatus target, BusinessContext context)
    {
        // Validation des transitions selon le contexte
        var validStatuses = context.GetValidStatuses();
        if (!validStatuses.Contains(target))
            return false;

        return current switch
        {
            // Transitions communes
            OrderStatus.Pending => target is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Cancelled => false, // Terminal status
            
            // E-Commerce workflow
            OrderStatus.Confirmed when context == BusinessContext.ECommerce => 
                target is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => target is OrderStatus.Shipped or OrderStatus.Cancelled or OrderStatus.OnHold,
            OrderStatus.Shipped => target is OrderStatus.Delivered or OrderStatus.Failed,
            OrderStatus.Delivered => target is OrderStatus.Refunded,
            OrderStatus.Failed => target is OrderStatus.Processing or OrderStatus.Cancelled,
            
            // Restaurant workflow
            OrderStatus.Confirmed when context == BusinessContext.Restaurant => 
                target is OrderStatus.KitchenQueue or OrderStatus.Cancelled,
            OrderStatus.KitchenQueue => target is OrderStatus.Cooking or OrderStatus.Cancelled,
            OrderStatus.Cooking => target is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => target is OrderStatus.Served or OrderStatus.Cancelled,
            OrderStatus.Served => false, // Terminal status
            
            // Boutique workflow  
            OrderStatus.Confirmed when context == BusinessContext.Boutique => 
                target is OrderStatus.Scanned or OrderStatus.Cancelled,
            OrderStatus.Scanned => target is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => target is OrderStatus.Receipted or OrderStatus.Cancelled,
            OrderStatus.Receipted => target is OrderStatus.Completed,
            OrderStatus.Completed => false, // Terminal status
            
            // Wholesale workflow
            OrderStatus.Confirmed when context == BusinessContext.Wholesale => 
                target is OrderStatus.QuoteRequested or OrderStatus.Cancelled,
            OrderStatus.QuoteRequested => target is OrderStatus.Approved or OrderStatus.Cancelled,
            OrderStatus.Approved => target is OrderStatus.BulkProcessing or OrderStatus.Cancelled,
            OrderStatus.BulkProcessing => target is OrderStatus.Delivered or OrderStatus.PartiallyFulfilled,
            
            // Exception handling
            OrderStatus.OnHold => target is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.PartiallyFulfilled => target is OrderStatus.Delivered or OrderStatus.Cancelled,
            
            _ => false
        };
    }

    /// <summary>
    /// Surcharge de compatibilité pour l'existant (sans contexte)
    /// </summary>
    public static bool CanTransitionTo(this OrderStatus current, OrderStatus target)
    {
        return CanTransitionTo(current, target, BusinessContext.ECommerce);
    }

    /// <summary>
    /// Détermine si un statut est terminal pour un contexte donné
    /// </summary>
    public static bool IsTerminalStatus(this OrderStatus status, BusinessContext context)
    {
        return context switch
        {
            BusinessContext.ECommerce => status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded,
            BusinessContext.Restaurant => status is OrderStatus.Served or OrderStatus.Cancelled,
            BusinessContext.Boutique => status is OrderStatus.Completed or OrderStatus.Cancelled,
            BusinessContext.Wholesale => status is OrderStatus.Delivered or OrderStatus.Cancelled,
            _ => status == OrderStatus.Cancelled
        };
    }

    /// <summary>
    /// Surcharge de compatibilité pour l'existant
    /// </summary>
    public static bool IsTerminalStatus(this OrderStatus status)
    {
        return IsTerminalStatus(status, BusinessContext.ECommerce);
    }

    /// <summary>
    /// Obtient les statuts suivants possibles selon le contexte
    /// </summary>
    public static IEnumerable<OrderStatus> GetNextValidStatuses(this OrderStatus current, BusinessContext context)
    {
        return context.GetValidStatuses()
            .Where(status => current.CanTransitionTo(status, context));
    }

    /// <summary>
    /// Nom d'affichage localisé selon le contexte
    /// </summary>
    public static string GetDisplayName(this OrderStatus status, BusinessContext? context = null)
    {
        return status switch
        {
            // Statuts communs
            OrderStatus.Pending => "En attente",
            OrderStatus.Confirmed => "Confirmée",
            OrderStatus.Cancelled => "Annulée",
            
            // E-Commerce
            OrderStatus.Processing => "En traitement",
            OrderStatus.Shipped => "Expédiée",
            OrderStatus.Delivered => "Livrée",
            OrderStatus.Refunded => "Remboursée",
            OrderStatus.Failed => "Échec",
            
            // Restaurant
            OrderStatus.KitchenQueue => "File d'attente cuisine",
            OrderStatus.Cooking => "En préparation",
            OrderStatus.Ready => "Prête à servir",
            OrderStatus.Served => "Servie",
            
            // Boutique
            OrderStatus.Scanned => "Articles scannés",
            OrderStatus.Paid => "Payée",
            OrderStatus.Receipted => "Ticket émis",
            OrderStatus.Completed => "Terminée",
            
            // Wholesale
            OrderStatus.QuoteRequested => "Devis demandé",
            OrderStatus.Approved => "Approuvée",
            OrderStatus.BulkProcessing => "Traitement en gros",
            
            // Exception
            OrderStatus.OnHold => "En attente",
            OrderStatus.PartiallyFulfilled => "Partiellement traitée",
            
            _ => "Statut inconnu"
        };
    }

    /// <summary>
    /// Obtient la couleur d'affichage pour l'UI selon le statut
    /// </summary>
    public static string GetStatusColor(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending or OrderStatus.KitchenQueue => "orange",
            OrderStatus.Confirmed or OrderStatus.Scanned => "blue",
            OrderStatus.Processing or OrderStatus.Cooking or OrderStatus.BulkProcessing => "purple",
            OrderStatus.Ready => "cyan",
            OrderStatus.Shipped or OrderStatus.Paid => "green",
            OrderStatus.Delivered or OrderStatus.Served or OrderStatus.Completed => "success",
            OrderStatus.Cancelled => "red",
            OrderStatus.Failed => "danger",
            OrderStatus.Refunded => "warning",
            OrderStatus.OnHold => "gray",
            _ => "default"
        };
    }

    /// <summary>
    /// Détermine si le statut nécessite une action utilisateur
    /// </summary>
    public static bool RequiresUserAction(this OrderStatus status)
    {
        return status is OrderStatus.Pending or OrderStatus.KitchenQueue or 
               OrderStatus.Ready or OrderStatus.QuoteRequested;
    }
}