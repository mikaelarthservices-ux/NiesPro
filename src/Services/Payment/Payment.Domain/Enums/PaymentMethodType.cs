namespace Payment.Domain.Enums;

/// <summary>
/// Types de moyens de paiement supportés
/// </summary>
public enum PaymentMethodType
{
    /// <summary>
    /// Espèces
    /// </summary>
    Cash = 0,

    /// <summary>
    /// Carte bancaire (débit/crédit)
    /// </summary>
    CreditCard = 1,

    /// <summary>
    /// Carte bancaire sans contact (NFC)
    /// </summary>
    ContactlessCard = 2,

    /// <summary>
    /// Paiement mobile (Apple Pay, Google Pay)
    /// </summary>
    MobilePayment = 3,

    /// <summary>
    /// Virement bancaire
    /// </summary>
    BankTransfer = 4,

    /// <summary>
    /// Chèque
    /// </summary>
    Check = 5,

    /// <summary>
    /// Titre restaurant/ticket restaurant
    /// </summary>
    MealVoucher = 6,

    /// <summary>
    /// Carte cadeau/avoir
    /// </summary>
    GiftCard = 7,

    /// <summary>
    /// Points de fidélité
    /// </summary>
    LoyaltyPoints = 8,

    /// <summary>
    /// Paiement différé/crédit
    /// </summary>
    DeferredPayment = 9,

    /// <summary>
    /// Portefeuille numérique (PayPal, etc.)
    /// </summary>
    DigitalWallet = 10,

    /// <summary>
    /// Cryptomonnaie
    /// </summary>
    Cryptocurrency = 11
}

/// <summary>
/// Extensions pour PaymentMethodType
/// </summary>
public static class PaymentMethodTypeExtensions
{
    /// <summary>
    /// Vérifier si le moyen de paiement nécessite une validation en ligne
    /// </summary>
    public static bool RequiresOnlineValidation(this PaymentMethodType type)
    {
        return type is PaymentMethodType.CreditCard or PaymentMethodType.ContactlessCard or 
               PaymentMethodType.MobilePayment or PaymentMethodType.BankTransfer or 
               PaymentMethodType.DigitalWallet or PaymentMethodType.Cryptocurrency;
    }

    /// <summary>
    /// Vérifier si le moyen de paiement est instantané
    /// </summary>
    public static bool IsInstant(this PaymentMethodType type)
    {
        return type is PaymentMethodType.Cash or PaymentMethodType.CreditCard or 
               PaymentMethodType.ContactlessCard or PaymentMethodType.MobilePayment or 
               PaymentMethodType.GiftCard or PaymentMethodType.LoyaltyPoints;
    }

    /// <summary>
    /// Vérifier si le moyen de paiement nécessite un terminal physique
    /// </summary>
    public static bool RequiresPhysicalTerminal(this PaymentMethodType type)
    {
        return type is PaymentMethodType.CreditCard or PaymentMethodType.ContactlessCard or 
               PaymentMethodType.MobilePayment;
    }

    /// <summary>
    /// Obtenir la limite maximale recommandée pour ce type de paiement
    /// </summary>
    public static decimal? GetRecommendedMaxLimit(this PaymentMethodType type, string currency = "EUR")
    {
        return type switch
        {
            PaymentMethodType.Cash => 500m,
            PaymentMethodType.ContactlessCard => 50m,
            PaymentMethodType.MobilePayment => 300m,
            PaymentMethodType.Check => 1000m,
            PaymentMethodType.MealVoucher => 25m,
            _ => null // Pas de limite spécifique
        };
    }

    /// <summary>
    /// Obtenir le nom d'affichage localisé
    /// </summary>
    public static string GetDisplayName(this PaymentMethodType type)
    {
        return type switch
        {
            PaymentMethodType.Cash => "Espèces",
            PaymentMethodType.CreditCard => "Carte bancaire",
            PaymentMethodType.ContactlessCard => "Sans contact",
            PaymentMethodType.MobilePayment => "Paiement mobile",
            PaymentMethodType.BankTransfer => "Virement",
            PaymentMethodType.Check => "Chèque",
            PaymentMethodType.MealVoucher => "Ticket restaurant",
            PaymentMethodType.GiftCard => "Carte cadeau",
            PaymentMethodType.LoyaltyPoints => "Points fidélité",
            PaymentMethodType.DeferredPayment => "Paiement différé",
            PaymentMethodType.DigitalWallet => "Portefeuille numérique",
            PaymentMethodType.Cryptocurrency => "Cryptomonnaie",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Vérifier si le type de paiement supporte les remboursements
    /// </summary>
    public static bool SupportsRefunds(this PaymentMethodType type)
    {
        return type is not (PaymentMethodType.Cash or PaymentMethodType.LoyaltyPoints or PaymentMethodType.MealVoucher);
    }
}