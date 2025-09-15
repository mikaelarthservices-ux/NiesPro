namespace Order.Domain.Enums;

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    PayPal = 3,
    BankTransfer = 4,
    Cash = 5,
    ApplePay = 6,
    GooglePay = 7,
    Stripe = 8
}

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6,
    PartiallyRefunded = 7
}

public static class PaymentExtensions
{
    public static string GetDisplayName(this PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.CreditCard => "Carte de crédit",
            PaymentMethod.DebitCard => "Carte de débit",
            PaymentMethod.PayPal => "PayPal",
            PaymentMethod.BankTransfer => "Virement bancaire",
            PaymentMethod.Cash => "Espèces",
            PaymentMethod.ApplePay => "Apple Pay",
            PaymentMethod.GooglePay => "Google Pay",
            PaymentMethod.Stripe => "Stripe",
            _ => "Inconnu"
        };
    }

    public static bool RequiresOnlineProcessing(this PaymentMethod method)
    {
        return method is not (PaymentMethod.Cash or PaymentMethod.BankTransfer);
    }

    public static string GetDisplayName(this PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "En attente",
            PaymentStatus.Processing => "En cours",
            PaymentStatus.Completed => "Complété",
            PaymentStatus.Failed => "Échec",
            PaymentStatus.Cancelled => "Annulé",
            PaymentStatus.Refunded => "Remboursé",
            PaymentStatus.PartiallyRefunded => "Partiellement remboursé",
            _ => "Inconnu"
        };
    }
}