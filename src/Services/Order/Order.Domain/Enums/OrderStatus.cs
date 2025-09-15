namespace Order.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Refunded = 7,
    Failed = 8
}

public static class OrderStatusExtensions
{
    public static bool CanTransitionTo(this OrderStatus current, OrderStatus target)
    {
        return current switch
        {
            OrderStatus.Pending => target is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => target is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => target is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => target is OrderStatus.Delivered or OrderStatus.Failed,
            OrderStatus.Delivered => target is OrderStatus.Refunded,
            OrderStatus.Cancelled => false,
            OrderStatus.Refunded => false,
            OrderStatus.Failed => target is OrderStatus.Processing or OrderStatus.Cancelled,
            _ => false
        };
    }

    public static bool IsTerminalStatus(this OrderStatus status)
    {
        return status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded;
    }

    public static string GetDisplayName(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "En attente",
            OrderStatus.Confirmed => "Confirmée",
            OrderStatus.Processing => "En préparation",
            OrderStatus.Shipped => "Expédiée",
            OrderStatus.Delivered => "Livrée",
            OrderStatus.Cancelled => "Annulée",
            OrderStatus.Refunded => "Remboursée",
            OrderStatus.Failed => "Échec",
            _ => "Inconnu"
        };
    }
}