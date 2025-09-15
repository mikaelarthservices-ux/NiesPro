namespace Payment.Domain.Enums;

/// <summary>
/// Marques de cartes bancaires supportées
/// </summary>
public enum CardBrand
{
    /// <summary>
    /// Marque inconnue
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Visa
    /// </summary>
    Visa = 1,

    /// <summary>
    /// Mastercard
    /// </summary>
    Mastercard = 2,

    /// <summary>
    /// American Express
    /// </summary>
    AmericanExpress = 3,

    /// <summary>
    /// Discover
    /// </summary>
    Discover = 4,

    /// <summary>
    /// JCB
    /// </summary>
    JCB = 5,

    /// <summary>
    /// Diners Club
    /// </summary>
    DinersClub = 6
}

/// <summary>
/// Extensions pour CardBrand
/// </summary>
public static class CardBrandExtensions
{
    /// <summary>
    /// Obtenir le nom affiché de la marque
    /// </summary>
    public static string GetDisplayName(this CardBrand brand) => brand switch
    {
        CardBrand.Visa => "Visa",
        CardBrand.Mastercard => "Mastercard",
        CardBrand.AmericanExpress => "American Express",
        CardBrand.Discover => "Discover",
        CardBrand.JCB => "JCB",
        CardBrand.DinersClub => "Diners Club",
        _ => "Unknown"
    };

    /// <summary>
    /// Vérifier si la marque supporte CVV
    /// </summary>
    public static bool SupportsCvv(this CardBrand brand) => brand switch
    {
        CardBrand.AmericanExpress => false, // Utilise CID à 4 chiffres
        _ => true // CVV à 3 chiffres pour les autres
    };

    /// <summary>
    /// Obtenir la longueur attendue du numéro de carte
    /// </summary>
    public static int GetExpectedLength(this CardBrand brand) => brand switch
    {
        CardBrand.AmericanExpress => 15,
        CardBrand.DinersClub => 14,
        _ => 16
    };
}