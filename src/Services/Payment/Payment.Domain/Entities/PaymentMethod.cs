using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using NiesPro.Contracts.Primitives;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant un moyen de paiement
/// </summary>
public class PaymentMethod : Entity
{
    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; private set; }

    /// <summary>
    /// Nom d'affichage du moyen de paiement
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Nom du moyen de paiement (alias pour DisplayName)
    /// </summary>
    [NotMapped]
    public string Name => DisplayName;

    /// <summary>
    /// Indique si ce moyen de paiement est actif
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indique si ce moyen de paiement est le moyen par défaut
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Carte de crédit associée (si applicable)
    /// </summary>
    public CreditCard? CreditCard { get; private set; }

    /// <summary>
    /// Métadonnées spécifiques au moyen de paiement
    /// </summary>
    public Dictionary<string, string> Metadata { get; private set; }

    /// <summary>
    /// Limite quotidienne pour ce moyen de paiement
    /// </summary>
    public Money? DailyLimit { get; private set; }

    /// <summary>
    /// Limite par transaction pour ce moyen de paiement
    /// </summary>
    public Money? TransactionLimit { get; private set; }

    /// <summary>
    /// Date d'expiration du moyen de paiement (si applicable)
    /// </summary>
    public DateTime? ExpiryDate { get; private set; }

    /// <summary>
    /// Identifiant du client propriétaire
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Token de sécurité pour les intégrations externes
    /// </summary>
    public string? ExternalToken { get; private set; }

    /// <summary>
    /// Token sécurisé pour les transactions
    /// </summary>
    public string? Token { get; private set; }

    /// <summary>
    /// Dernière date d'utilisation
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    /// <summary>
    /// Constructeur protégé pour Entity Framework
    /// </summary>
    protected PaymentMethod() 
    {
        DisplayName = string.Empty;
        Metadata = new Dictionary<string, string>();
    }

    /// <summary>
    /// Constructeur pour créer un nouveau moyen de paiement
    /// </summary>
    public PaymentMethod(
        PaymentMethodType type,
        string displayName,
        Guid customerId,
        CreditCard? creditCard = null,
        Money? dailyLimit = null,
        Money? transactionLimit = null,
        DateTime? expiryDate = null,
        string? externalToken = null)
    {
        ValidateConstructorParameters(type, displayName, customerId, creditCard);

        Id = Guid.NewGuid();
        Type = type;
        DisplayName = displayName;
        CustomerId = customerId;
        CreditCard = creditCard;
        DailyLimit = dailyLimit;
        TransactionLimit = transactionLimit;
        ExpiryDate = expiryDate;
        ExternalToken = externalToken;
        IsActive = true;
        IsDefault = false;
        Metadata = new Dictionary<string, string>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activer le moyen de paiement
    /// </summary>
    public void Activate()
    {
        if (IsExpired())
            throw new InvalidOperationException("Cannot activate an expired payment method");

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Désactiver le moyen de paiement
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir comme moyen de paiement par défaut
    /// </summary>
    public void SetAsDefault()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot set inactive payment method as default");

        if (IsExpired())
            throw new InvalidOperationException("Cannot set expired payment method as default");

        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retirer le statut de moyen de paiement par défaut
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si le moyen de paiement a expiré
    /// </summary>
    public bool IsExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si le montant respecte les limites
    /// </summary>
    public bool IsWithinLimits(Money amount)
    {
        if (TransactionLimit != null && amount > TransactionLimit)
            return false;

        // Note: La vérification de la limite quotidienne nécessiterait 
        // l'accès aux transactions du jour, ce qui serait fait dans un service
        return true;
    }

    /// <summary>
    /// Mettre à jour la dernière utilisation
    /// </summary>
    public void UpdateLastUsed()
    {
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ajouter ou mettre à jour une métadonnée
    /// </summary>
    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        Metadata[key] = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer le token sécurisé
    /// </summary>
    public void SetToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        Token = token;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Effacer le token sécurisé
    /// </summary>
    public void ClearToken()
    {
        Token = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Supprimer une métadonnée
    /// </summary>
    public void RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        Metadata.Remove(key);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtenir une métadonnée
    /// </summary>
    public string? GetMetadata(string key)
    {
        return Metadata.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Mettre à jour les limites
    /// </summary>
    public void UpdateLimits(Money? dailyLimit, Money? transactionLimit)
    {
        // Validation des limites
        if (dailyLimit != null && transactionLimit != null)
        {
            if (dailyLimit < transactionLimit)
                throw new ArgumentException("Daily limit cannot be less than transaction limit");
        }

        DailyLimit = dailyLimit;
        TransactionLimit = transactionLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si le moyen de paiement peut être utilisé
    /// </summary>
    public bool CanBeUsed()
    {
        return IsActive && !IsExpired();
    }

    /// <summary>
    /// Obtenir le nom d'affichage sécurisé (masque les informations sensibles)
    /// </summary>
    public string GetSecureDisplayName()
    {
        if (Type == PaymentMethodType.CreditCard && CreditCard != null)
        {
            return $"{CreditCard.Brand} •••• {CreditCard.GetMaskedNumber()[^4..]}";
        }

        return DisplayName;
    }

    /// <summary>
    /// Validation des paramètres du constructeur
    /// </summary>
    private static void ValidateConstructorParameters(
        PaymentMethodType type, 
        string displayName, 
        Guid customerId, 
        CreditCard? creditCard)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty", nameof(displayName));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        // Validation spécifique pour les cartes de crédit
        if (type is PaymentMethodType.CreditCard or PaymentMethodType.ContactlessCard)
        {
            if (creditCard == null)
                throw new ArgumentException("Credit card information is required for card payment methods", nameof(creditCard));
        }
    }
}

/// <summary>
/// Classe de base pour les entités
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
}