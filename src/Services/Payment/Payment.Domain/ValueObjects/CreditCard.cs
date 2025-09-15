using System.Text.RegularExpressions;
using Payment.Domain.Enums;

namespace Payment.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une carte bancaire sécurisée
/// Avec validation et masquage automatique des données sensibles
/// </summary>
public sealed record CreditCard
{
    public string MaskedNumber { get; }
    public string Last4Digits { get; }
    public string CardholderName { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string CardType { get; }
    public CardBrand Brand { get; }
    public string Token { get; } // Token sécurisé pour transactions

    private CreditCard(string maskedNumber, string last4Digits, string cardholderName, 
                      int expiryMonth, int expiryYear, string cardType, CardBrand brand, string token)
    {
        MaskedNumber = maskedNumber;
        Last4Digits = last4Digits;
        CardholderName = cardholderName;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CardType = cardType;
        Brand = brand;
        Token = token;
    }

    /// <summary>
    /// Créer une carte bancaire depuis un numéro complet (pour tokenisation)
    /// Le numéro complet n'est jamais stocké
    /// </summary>
    public static CreditCard Create(string cardNumber, string cardholderName, 
                                   int expiryMonth, int expiryYear, string cvv)
    {
        // Validation stricte
        ValidateCardNumber(cardNumber);
        ValidateCardholderName(cardholderName);
        ValidateExpiryDate(expiryMonth, expiryYear);
        ValidateCvv(cvv);

        // Nettoyage du numéro
        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        // Détection du type de carte
        var cardType = DetectCardType(cleanNumber);
        var cardBrand = DetectCardBrand(cleanNumber);
        
        // Masquage sécurisé
        var maskedNumber = MaskCardNumber(cleanNumber);
        var last4Digits = cleanNumber.Substring(cleanNumber.Length - 4);
        
        // Génération d'un token sécurisé (simulation)
        var token = GenerateSecureToken(cleanNumber, cvv);

        return new CreditCard(maskedNumber, last4Digits, cardholderName.Trim(), 
                             expiryMonth, expiryYear, cardType, cardBrand, token);
    }

    /// <summary>
    /// Créer depuis un token existant (pour récupération depuis base de données)
    /// </summary>
    public static CreditCard FromToken(string token, string maskedNumber, string last4Digits,
                                      string cardholderName, int expiryMonth, int expiryYear, string cardType, CardBrand brand)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Le token ne peut pas être vide", nameof(token));

        return new CreditCard(maskedNumber, last4Digits, cardholderName, 
                             expiryMonth, expiryYear, cardType, brand, token);
    }

    /// <summary>
    /// Vérifier si la carte a expiré
    /// </summary>
    public bool IsExpired()
    {
        var now = DateTime.Now;
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, DateTime.DaysInMonth(ExpiryYear, ExpiryMonth));
        return now > expiryDate;
    }

    /// <summary>
    /// Vérifier si la carte expire bientôt (dans les 30 prochains jours)
    /// </summary>
    public bool ExpiresWithin(TimeSpan timeSpan)
    {
        var now = DateTime.Now;
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, DateTime.DaysInMonth(ExpiryYear, ExpiryMonth));
        return expiryDate <= now.Add(timeSpan);
    }

    /// <summary>
    /// Date d'expiration formatée
    /// </summary>
    public DateTime ExpiryDate => new DateTime(ExpiryYear, ExpiryMonth, DateTime.DaysInMonth(ExpiryYear, ExpiryMonth));

    /// <summary>
    /// Obtenir le numéro masqué
    /// </summary>
    public string GetMaskedNumber() => MaskedNumber;

    /// <summary>
    /// Obtenir un identifiant unique pour la carte (pour déduplication)
    /// </summary>
    public string GetUniqueIdentifier()
    {
        return $"{CardType}_{Last4Digits}_{ExpiryMonth:D2}{ExpiryYear}";
    }

    private static void ValidateCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("Le numéro de carte ne peut pas être vide", nameof(cardNumber));

        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (!Regex.IsMatch(cleanNumber, @"^\d{13,19}$"))
            throw new ArgumentException("Le numéro de carte doit contenir entre 13 et 19 chiffres", nameof(cardNumber));

        if (!IsValidLuhn(cleanNumber))
            throw new ArgumentException("Le numéro de carte n'est pas valide (algorithme de Luhn)", nameof(cardNumber));
    }

    private static void ValidateCardholderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du porteur ne peut pas être vide", nameof(name));

        if (name.Length < 2 || name.Length > 100)
            throw new ArgumentException("Le nom du porteur doit contenir entre 2 et 100 caractères", nameof(name));
    }

    private static void ValidateExpiryDate(int month, int year)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("Le mois d'expiration doit être entre 1 et 12", nameof(month));

        if (year < DateTime.Now.Year || year > DateTime.Now.Year + 20)
            throw new ArgumentException("L'année d'expiration n'est pas valide", nameof(year));

        var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        if (expiryDate < DateTime.Now)
            throw new ArgumentException("La carte a déjà expiré", nameof(year));
    }

    private static void ValidateCvv(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
            throw new ArgumentException("Le CVV ne peut pas être vide", nameof(cvv));

        if (!Regex.IsMatch(cvv, @"^\d{3,4}$"))
            throw new ArgumentException("Le CVV doit contenir 3 ou 4 chiffres", nameof(cvv));
    }

    private static string DetectCardType(string cardNumber)
    {
        return cardNumber.Substring(0, 1) switch
        {
            "4" => "Visa",
            "5" => "Mastercard",
            "3" when cardNumber.StartsWith("34") || cardNumber.StartsWith("37") => "American Express",
            "6" => "Discover",
            _ => "Unknown"
        };
    }

    private static CardBrand DetectCardBrand(string cardNumber)
    {
        return cardNumber switch
        {
            var num when num.StartsWith("4") => CardBrand.Visa,
            var num when num.StartsWith("5") || (num.StartsWith("2") && num.Length >= 6 && int.Parse(num[..6]) >= 222100 && int.Parse(num[..6]) <= 272099) => CardBrand.Mastercard,
            var num when num.StartsWith("34") || num.StartsWith("37") => CardBrand.AmericanExpress,
            var num when num.StartsWith("6011") || num.StartsWith("65") => CardBrand.Discover,
            var num when num.StartsWith("35") => CardBrand.JCB,
            var num when num.StartsWith("30") || num.StartsWith("36") || num.StartsWith("38") => CardBrand.DinersClub,
            _ => CardBrand.Unknown
        };
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length <= 8) return new string('*', cardNumber.Length);
        
        var firstFour = cardNumber.Substring(0, 4);
        var lastFour = cardNumber.Substring(cardNumber.Length - 4);
        var middle = new string('*', cardNumber.Length - 8);
        
        return $"{firstFour}{middle}{lastFour}";
    }

    private static string GenerateSecureToken(string cardNumber, string cvv)
    {
        // En production, utiliser un service de tokenisation sécurisé
        var combined = $"{cardNumber}_{cvv}_{DateTime.UtcNow.Ticks}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hash)[..32];
    }

    private static bool IsValidLuhn(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int digit = int.Parse(cardNumber[i].ToString());
            
            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit = digit / 10 + digit % 10;
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        return sum % 10 == 0;
    }

    public override string ToString() => $"{CardType} ending in {Last4Digits}";
}