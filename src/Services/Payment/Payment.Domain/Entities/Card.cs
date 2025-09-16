using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant une carte bancaire persistée
/// Conforme PCI-DSS avec tokenisation
/// </summary>
public class Card
{
    /// <summary>
    /// Identifiant unique de la carte
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Token sécurisé de la carte
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Numéro masqué de la carte
    /// </summary>
    public string MaskedNumber { get; private set; } = string.Empty;

    /// <summary>
    /// 4 derniers chiffres
    /// </summary>
    public string Last4Digits { get; private set; } = string.Empty;

    /// <summary>
    /// Alias pour les 4 derniers chiffres (compatibilité Infrastructure)
    /// </summary>
    public string LastFourDigits => Last4Digits;

    /// <summary>
    /// Nom du porteur
    /// </summary>
    public string CardholderName { get; private set; } = string.Empty;

    /// <summary>
    /// Mois d'expiration
    /// </summary>
    public int ExpiryMonth { get; private set; }

    /// <summary>
    /// Année d'expiration
    /// </summary>
    public int ExpiryYear { get; private set; }

    /// <summary>
    /// Marque de la carte
    /// </summary>
    public CardBrand Brand { get; private set; }

    /// <summary>
    /// Type de carte (pour affichage)
    /// </summary>
    public string CardType { get; private set; } = string.Empty;

    /// <summary>
    /// Identifiant du client propriétaire
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Indique si cette carte est la carte par défaut du client
    /// </summary>
    public bool IsDefault { get; private set; } = false;

    /// <summary>
    /// Numéro complet de la carte (utilisé temporairement pour traitement)
    /// ATTENTION: À ne jamais persister en base
    /// </summary>
    public string? CardNumber { get; private set; }

    /// <summary>
    /// Code de vérification de la carte (utilisé temporairement pour traitement)
    /// ATTENTION: À ne jamais persister en base
    /// </summary>
    public string? Cvv { get; private set; }

    /// <summary>
    /// Indique si la carte est active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Date de dernière utilisation
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    /// <summary>
    /// Empreinte cryptographique de la carte
    /// </summary>
    public string? Fingerprint { get; private set; }

    /// <summary>
    /// Adresse de facturation chiffrée
    /// </summary>
    public string? EncryptedBillingAddress { get; private set; }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    private Card()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Constructeur principal
    /// </summary>
    public Card(string token, string maskedNumber, string last4Digits, string cardholderName,
                int expiryMonth, int expiryYear, CardBrand brand, string cardType, Guid customerId)
        : this()
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        MaskedNumber = maskedNumber ?? throw new ArgumentNullException(nameof(maskedNumber));
        Last4Digits = last4Digits ?? throw new ArgumentNullException(nameof(last4Digits));
        CardholderName = cardholderName ?? throw new ArgumentNullException(nameof(cardholderName));
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Brand = brand;
        CardType = cardType ?? throw new ArgumentNullException(nameof(cardType));
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        
        ValidateCard();
    }

    /// <summary>
    /// Constructeur avec paramètres supplémentaires
    /// </summary>
    public Card(string token, string maskedNumber, string lastFourDigits, string cardholderName,
                int expiryMonth, int expiryYear, CardBrand brand, string cardType, Guid customerId,
                string? fingerprint = null, string? encryptedBillingAddress = null)
        : this(token, maskedNumber, lastFourDigits, cardholderName, expiryMonth, expiryYear, brand, cardType, customerId)
    {
        Fingerprint = fingerprint;
        EncryptedBillingAddress = encryptedBillingAddress;
    }

    /// <summary>
    /// Définir l'empreinte cryptographique
    /// </summary>
    public void SetFingerprint(string fingerprint)
    {
        Fingerprint = fingerprint ?? throw new ArgumentNullException(nameof(fingerprint));
    }

    /// <summary>
    /// Définir l'adresse de facturation chiffrée
    /// </summary>
    public void SetEncryptedBillingAddress(string encryptedAddress)
    {
        EncryptedBillingAddress = encryptedAddress ?? throw new ArgumentNullException(nameof(encryptedAddress));
    }

    /// <summary>
    /// Marquer la dernière utilisation
    /// </summary>
    public void MarkAsUsed()
    {
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Désactiver la carte
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activer la carte
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir comme carte par défaut
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retirer le statut carte par défaut
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les données temporaires de carte (pour traitement uniquement)
    /// ATTENTION: Ces données ne doivent jamais être persistées
    /// </summary>
    public void SetTemporaryCardData(string cardNumber, string cvv)
    {
        CardNumber = cardNumber;
        Cvv = cvv;
    }

    /// <summary>
    /// Effacer les données temporaires de carte après traitement
    /// </summary>
    public void ClearTemporaryCardData()
    {
        CardNumber = null;
        Cvv = null;
    }

    /// <summary>
    /// Vérifier si la carte a expiré
    /// </summary>
    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        return now.Year > ExpiryYear || (now.Year == ExpiryYear && now.Month > ExpiryMonth);
    }

    /// <summary>
    /// Vérifier si la carte est valide pour utilisation
    /// </summary>
    public bool IsValidForUse()
    {
        return IsActive && !IsExpired();
    }

    /// <summary>
    /// Validation de la carte
    /// </summary>
    private void ValidateCard()
    {
        if (string.IsNullOrWhiteSpace(Token))
            throw new ArgumentException("Le token ne peut pas être vide", nameof(Token));
            
        if (string.IsNullOrWhiteSpace(MaskedNumber))
            throw new ArgumentException("Le numéro masqué ne peut pas être vide", nameof(MaskedNumber));
            
        if (string.IsNullOrWhiteSpace(Last4Digits))
            throw new ArgumentException("Les 4 derniers chiffres ne peuvent pas être vides", nameof(Last4Digits));
            
        if (string.IsNullOrWhiteSpace(CardholderName))
            throw new ArgumentException("Le nom du porteur ne peut pas être vide", nameof(CardholderName));
            
        if (ExpiryMonth < 1 || ExpiryMonth > 12)
            throw new ArgumentException("Le mois d'expiration doit être entre 1 et 12", nameof(ExpiryMonth));
            
        if (ExpiryYear < DateTime.UtcNow.Year)
            throw new ArgumentException("L'année d'expiration ne peut pas être dans le passé", nameof(ExpiryYear));
            
        if (CustomerId == Guid.Empty)
            throw new ArgumentException("L'identifiant du client ne peut pas être vide", nameof(CustomerId));
    }
}