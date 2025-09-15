using MediatR;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;

namespace Payment.Application.Commands;

/// <summary>
/// Commande pour créer un nouveau paiement
/// </summary>
public class CreatePaymentCommand : IRequest<CreatePaymentResult>
{
    /// <summary>
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise du paiement
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
    /// Délai d'expiration en minutes (défaut: 60)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Nombre maximum de tentatives (défaut: 3)
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Autoriser les paiements partiels
    /// </summary>
    public bool AllowPartialPayments { get; set; } = false;

    /// <summary>
    /// Montant minimum pour un paiement partiel
    /// </summary>
    public decimal? MinimumPartialAmount { get; set; }

    /// <summary>
    /// Devise du montant minimum partiel
    /// </summary>
    public string? MinimumPartialCurrency { get; set; }

    /// <summary>
    /// URL de retour en cas de succès
    /// </summary>
    public string? SuccessUrl { get; set; }

    /// <summary>
    /// URL de retour en cas d'échec
    /// </summary>
    public string? FailureUrl { get; set; }

    /// <summary>
    /// URL de webhook pour les notifications
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Métadonnées additionnelles
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Résultat de la création d'un paiement
/// </summary>
public class CreatePaymentResult
{
    /// <summary>
    /// Identifiant du paiement créé
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Numéro de paiement
    /// </summary>
    public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Statut du paiement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Date d'expiration
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// URL de paiement (si applicable)
    /// </summary>
    public string? PaymentUrl { get; set; }

    /// <summary>
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur en cas d'échec
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Commande pour traiter un paiement avec un moyen de paiement spécifique
/// </summary>
public class ProcessPaymentCommand : IRequest<ProcessPaymentResult>
{
    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Montant à traiter (pour paiements partiels)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Devise du montant
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Code de vérification (CVV pour cartes)
    /// </summary>
    public string? VerificationCode { get; set; }

    /// <summary>
    /// Données 3D Secure
    /// </summary>
    public ThreeDSecureData? ThreeDSecure { get; set; }

    /// <summary>
    /// Adresse IP du client
    /// </summary>
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// User Agent du client
    /// </summary>
    public string? ClientUserAgent { get; set; }

    /// <summary>
    /// Données de géolocalisation
    /// </summary>
    public string? GeoLocation { get; set; }

    /// <summary>
    /// Force le traitement même en cas de score de fraude élevé
    /// </summary>
    public bool ForceProcess { get; set; } = false;
}

/// <summary>
/// Données 3D Secure pour l'authentification
/// </summary>
public class ThreeDSecureData
{
    /// <summary>
    /// Indique si 3D Secure est activé
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Version de 3D Secure (1.0, 2.0, 2.1, 2.2)
    /// </summary>
    public string Version { get; set; } = "2.0";

    /// <summary>
    /// Résultat de l'authentification
    /// </summary>
    public string? AuthenticationResult { get; set; }

    /// <summary>
    /// ECI (Electronic Commerce Indicator)
    /// </summary>
    public string? Eci { get; set; }

    /// <summary>
    /// CAVV (Cardholder Authentication Verification Value)
    /// </summary>
    public string? Cavv { get; set; }

    /// <summary>
    /// XID (Transaction Identifier)
    /// </summary>
    public string? Xid { get; set; }
}

/// <summary>
/// Résultat du traitement d'un paiement
/// </summary>
public class ProcessPaymentResult
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Numéro de transaction
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Statut de la transaction
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Code d'autorisation (si applicable)
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Référence externe
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Montant traité
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Frais de traitement
    /// </summary>
    public decimal? Fees { get; set; }

    /// <summary>
    /// Raison du refus (en cas d'échec)
    /// </summary>
    public PaymentDeclineReason? DeclineReason { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Score de fraude
    /// </summary>
    public int? FraudScore { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Indique si une authentification 3D Secure est requise
    /// </summary>
    public bool Requires3DSecure { get; set; }

    /// <summary>
    /// URL pour l'authentification 3D Secure
    /// </summary>
    public string? ThreeDSecureUrl { get; set; }

    /// <summary>
    /// Données additionnelles pour 3D Secure
    /// </summary>
    public Dictionary<string, string>? ThreeDSecureData { get; set; }
}

/// <summary>
/// Commande pour capturer une transaction autorisée
/// </summary>
public class CaptureTransactionCommand : IRequest<CaptureTransactionResult>
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Montant à capturer (pour capture partielle)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Devise du montant
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Raison de la capture
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Résultat de la capture d'une transaction
/// </summary>
public class CaptureTransactionResult
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Numéro de transaction
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Statut après capture
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Montant capturé
    /// </summary>
    public decimal CapturedAmount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Frais appliqués
    /// </summary>
    public decimal? Fees { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime ProcessedAt { get; set; }

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
/// Commande pour rembourser une transaction
/// </summary>
public class RefundTransactionCommand : IRequest<RefundTransactionResult>
{
    /// <summary>
    /// Identifiant de la transaction originale
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Montant à rembourser
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise du remboursement
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Raison du remboursement
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Identifiant de l'utilisateur qui initie le remboursement
    /// </summary>
    public Guid? InitiatedBy { get; set; }
}

/// <summary>
/// Résultat du remboursement
/// </summary>
public class RefundTransactionResult
{
    /// <summary>
    /// Identifiant de la transaction de remboursement
    /// </summary>
    public Guid RefundTransactionId { get; set; }

    /// <summary>
    /// Numéro de la transaction de remboursement
    /// </summary>
    public string RefundTransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant de la transaction originale
    /// </summary>
    public Guid OriginalTransactionId { get; set; }

    /// <summary>
    /// Montant remboursé
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut du remboursement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Indication de succès
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }
}