using MediatR;
using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands;

/// <summary>
/// Commande pour créer un nouveau moyen de paiement
/// </summary>
public class CreatePaymentMethodCommand : IRequest<CreatePaymentMethodResult>
{
    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nom d'affichage
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Définir comme moyen de paiement par défaut
    /// </summary>
    public bool SetAsDefault { get; set; } = false;

    /// <summary>
    /// Informations de carte de crédit (si applicable)
    /// </summary>
    public CreditCardInfo? CreditCard { get; set; }

    /// <summary>
    /// Limite quotidienne
    /// </summary>
    public decimal? DailyLimit { get; set; }

    /// <summary>
    /// Devise de la limite quotidienne
    /// </summary>
    public string? DailyLimitCurrency { get; set; }

    /// <summary>
    /// Limite par transaction
    /// </summary>
    public decimal? TransactionLimit { get; set; }

    /// <summary>
    /// Devise de la limite de transaction
    /// </summary>
    public string? TransactionLimitCurrency { get; set; }

    /// <summary>
    /// Date d'expiration (si applicable)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Token externe pour intégration
    /// </summary>
    public string? ExternalToken { get; set; }

    /// <summary>
    /// Métadonnées additionnelles
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Informations de carte de crédit pour la création
/// </summary>
public class CreditCardInfo
{
    /// <summary>
    /// Numéro de carte (sera tokenisé)
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Nom du porteur
    /// </summary>
    public string HolderName { get; set; } = string.Empty;

    /// <summary>
    /// Mois d'expiration (1-12)
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Année d'expiration (YYYY)
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Code de vérification (CVV)
    /// </summary>
    public string? Cvv { get; set; }

    /// <summary>
    /// Marque de la carte (détectée automatiquement)
    /// </summary>
    public string? Brand { get; set; }
}

/// <summary>
/// Résultat de la création d'un moyen de paiement
/// </summary>
public class CreatePaymentMethodResult
{
    /// <summary>
    /// Identifiant du moyen de paiement créé
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nom d'affichage sécurisé
    /// </summary>
    public string SecureDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Indique si c'est le moyen par défaut
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Token de la carte (si applicable)
    /// </summary>
    public string? CardToken { get; set; }

    /// <summary>
    /// Derniers chiffres de la carte (si applicable)
    /// </summary>
    public string? LastFourDigits { get; set; }

    /// <summary>
    /// Marque de la carte (si applicable)
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Date d'expiration de la carte (si applicable)
    /// </summary>
    public DateTime? CardExpiryDate { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Commande pour mettre à jour un moyen de paiement
/// </summary>
public class UpdatePaymentMethodCommand : IRequest<UpdatePaymentMethodResult>
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Nouveau nom d'affichage
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Nouvelle limite quotidienne
    /// </summary>
    public decimal? DailyLimit { get; set; }

    /// <summary>
    /// Devise de la limite quotidienne
    /// </summary>
    public string? DailyLimitCurrency { get; set; }

    /// <summary>
    /// Nouvelle limite par transaction
    /// </summary>
    public decimal? TransactionLimit { get; set; }

    /// <summary>
    /// Devise de la limite de transaction
    /// </summary>
    public string? TransactionLimitCurrency { get; set; }

    /// <summary>
    /// Métadonnées à mettre à jour
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Résultat de la mise à jour d'un moyen de paiement
/// </summary>
public class UpdatePaymentMethodResult
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Commande pour désactiver un moyen de paiement
/// </summary>
public class DeactivatePaymentMethodCommand : IRequest<DeactivatePaymentMethodResult>
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Raison de la désactivation
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Résultat de la désactivation d'un moyen de paiement
/// </summary>
public class DeactivatePaymentMethodResult
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Commande pour définir un moyen de paiement par défaut
/// </summary>
public class SetDefaultPaymentMethodCommand : IRequest<SetDefaultPaymentMethodResult>
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Identifiant du moyen de paiement à définir par défaut
    /// </summary>
    public Guid PaymentMethodId { get; set; }
}

/// <summary>
/// Résultat de la définition d'un moyen de paiement par défaut
/// </summary>
public class SetDefaultPaymentMethodResult
{
    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Identifiant du nouveau moyen de paiement par défaut
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Identifiant de l'ancien moyen de paiement par défaut (si applicable)
    /// </summary>
    public Guid? PreviousDefaultPaymentMethodId { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Commande pour valider un moyen de paiement
/// </summary>
public class ValidatePaymentMethodCommand : IRequest<ValidatePaymentMethodResult>
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Montant de validation (généralement 1€)
    /// </summary>
    public decimal ValidationAmount { get; set; } = 1.00m;

    /// <summary>
    /// Devise de validation
    /// </summary>
    public string ValidationCurrency { get; set; } = "EUR";

    /// <summary>
    /// Code de vérification (pour cartes)
    /// </summary>
    public string? VerificationCode { get; set; }
}

/// <summary>
/// Résultat de la validation d'un moyen de paiement
/// </summary>
public class ValidatePaymentMethodResult
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Identifiant de la transaction de validation
    /// </summary>
    public Guid? ValidationTransactionId { get; set; }

    /// <summary>
    /// Indique si la validation est réussie
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Code d'autorisation de la validation
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Raison de l'échec (si applicable)
    /// </summary>
    public PaymentDeclineReason? DeclineReason { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indication de succès de l'opération
    /// </summary>
    public bool IsSuccess { get; set; }
}