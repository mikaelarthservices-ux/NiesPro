using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.DTOs;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Interfaces;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Payment.Application.Commands.V2;

/// <summary>
/// Commande pour créer un nouveau paiement - NiesPro Enterprise Standard
/// </summary>
public class CreatePaymentCommand : BaseCommand<ApiResponse<PaymentResponse>>
{
    /// <summary>
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise du paiement (défaut: EUR)
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Identifiant de la commande associée
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Description du paiement
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Méthode de paiement
    /// </summary>
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.CreditCard;

    /// <summary>
    /// Informations de la carte de crédit (si applicable)
    /// </summary>
    public CreditCardInfo? CreditCardInfo { get; set; }

    /// <summary>
    /// URL de retour après paiement
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour après annulation
    /// </summary>
    public string? CancelUrl { get; set; }

    /// <summary>
    /// URL de notification webhook
    /// </summary>
    public string? NotificationUrl { get; set; }

    /// <summary>
    /// Métadonnées additionnelles
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Commande pour traiter un paiement existant - NiesPro Enterprise Standard
/// </summary>
public class ProcessPaymentCommand : BaseCommand<ApiResponse<TransactionResponse>>
{
    /// <summary>
    /// Identifiant du paiement à traiter
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Token de sécurité du paiement
    /// </summary>
    public string? PaymentToken { get; set; }

    /// <summary>
    /// Authentification 3D Secure
    /// </summary>
    public ThreeDSecureData? ThreeDSecureData { get; set; }

    /// <summary>
    /// Métadonnées de la transaction
    /// </summary>
    public Dictionary<string, object>? TransactionMetadata { get; set; }

    /// <summary>
    /// Adresse IP du client
    /// </summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// User Agent du client
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// Commande pour rembourser une transaction - NiesPro Enterprise Standard
/// </summary>
public class RefundTransactionCommand : BaseCommand<ApiResponse<RefundResponse>>
{
    /// <summary>
    /// Identifiant de la transaction à rembourser
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Montant à rembourser (optionnel, remboursement total si non spécifié)
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Raison du remboursement
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Notes administratives
    /// </summary>
    public string? AdminNotes { get; set; }
}

/// <summary>
/// Commande pour capturer une autorisation - NiesPro Enterprise Standard
/// </summary>
public class CaptureTransactionCommand : BaseCommand<ApiResponse<TransactionResponse>>
{
    /// <summary>
    /// Identifiant de la transaction d'autorisation
    /// </summary>
    public Guid AuthorizationTransactionId { get; set; }

    /// <summary>
    /// Montant à capturer (optionnel, capture totale si non spécifié)
    /// </summary>
    public decimal? CaptureAmount { get; set; }

    /// <summary>
    /// Métadonnées de capture
    /// </summary>
    public Dictionary<string, object>? CaptureMetadata { get; set; }
}

/// <summary>
/// Informations de carte de crédit pour le paiement
/// </summary>
public class CreditCardInfo
{
    /// <summary>
    /// Numéro de carte (masqué ou token)
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Mois d'expiration
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Année d'expiration
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Code de vérification (CVV)
    /// </summary>
    public string? Cvv { get; set; }

    /// <summary>
    /// Nom du porteur
    /// </summary>
    public string? HolderName { get; set; }
}

/// <summary>
/// Données d'authentification 3D Secure
/// </summary>
public class ThreeDSecureData
{
    /// <summary>
    /// Token 3D Secure
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Statut de l'authentification
    /// </summary>
    public string AuthenticationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Transaction ID 3DS
    /// </summary>
    public string? TransactionId { get; set; }
}

/// <summary>
/// Réponse pour les opérations de paiement
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Numéro de paiement
    /// </summary>
    public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID de la commande associée
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// ID du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// ID du marchand
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Statut du paiement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Méthode de paiement
    /// </summary>
    public PaymentMethodType PaymentMethod { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Référence externe
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// URL de retour
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de notification
    /// </summary>
    public string? NotificationUrl { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Réponse pour les opérations de transaction
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID du paiement associé
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Type de transaction
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Statut de la transaction
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Référence externe (processeur de paiement)
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Réponse pour les opérations de remboursement
/// </summary>
public class RefundResponse
{
    /// <summary>
    /// Identifiant du remboursement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID du paiement associé
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Montant remboursé
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Raison du remboursement
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Statut du remboursement
    /// </summary>
    public RefundStatus Status { get; set; }

    /// <summary>
    /// ID de remboursement externe
    /// </summary>
    public string? ExternalRefundId { get; set; }

    /// <summary>
    /// Date du remboursement
    /// </summary>
    public DateTime RefundDate { get; set; }
}

/// <summary>
/// Commande pour annuler un paiement - NiesPro Enterprise Standard
/// </summary>
public class CancelPaymentCommand : BaseCommand<ApiResponse<PaymentResponse>>
{
    /// <summary>
    /// ID du paiement à annuler
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Raison de l'annulation
    /// </summary>
    public string CancellationReason { get; set; } = string.Empty;

    /// <summary>
    /// Notes administratives
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Métadonnées supplémentaires pour l'annulation
    /// </summary>
    public Dictionary<string, object>? CancellationMetadata { get; set; }
}