using Payment.Domain.Enums;
using Payment.Domain.Entities;

namespace Payment.Application.DTOs;

/// <summary>
/// DTO pour les détails complets d'un paiement
/// </summary>
public class PaymentDetailDto
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
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut du paiement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Identifiant de la commande
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
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date d'expiration
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Date de finalisation
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Nombre de tentatives
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Nombre maximum de tentatives
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Montant payé
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Montant remboursé
    /// </summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>
    /// Montant net (payé - remboursé)
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Frais de traitement
    /// </summary>
    public decimal? ProcessingFees { get; set; }

    /// <summary>
    /// Mode de calcul des frais
    /// </summary>
    public string? FeeMode { get; set; }

    /// <summary>
    /// Autorise les paiements partiels
    /// </summary>
    public bool AllowPartialPayments { get; set; }

    /// <summary>
    /// Montant minimum pour paiement partiel
    /// </summary>
    public decimal? MinimumPartialAmount { get; set; }

    /// <summary>
    /// URLs de redirection
    /// </summary>
    public string? SuccessUrl { get; set; }
    public string? FailureUrl { get; set; }
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Transactions associées
    /// </summary>
    public List<TransactionSummaryDto> Transactions { get; set; } = new();

    /// <summary>
    /// Dernier moyen de paiement utilisé
    /// </summary>
    public PaymentMethodSummaryDto? LastPaymentMethod { get; set; }

    /// <summary>
    /// Métadonnées de session
    /// </summary>
    public Dictionary<string, string> SessionData { get; set; } = new();
}

/// <summary>
/// DTO pour le résumé d'un paiement
/// </summary>
public class PaymentSummaryDto
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
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut du paiement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Identifiant de la commande
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de finalisation
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Montant payé
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Type du dernier moyen de paiement utilisé
    /// </summary>
    public PaymentMethodType? LastPaymentMethodType { get; set; }

    /// <summary>
    /// Description courte
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// DTO pour les détails complets d'une transaction
/// </summary>
public class TransactionDetailDto
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Numéro de transaction
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut de la transaction
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Type de transaction
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Identifiant du paiement
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Identifiant de la commande
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Référence externe
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// Code d'autorisation
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Date d'expiration de l'autorisation
    /// </summary>
    public DateTime? AuthorizationExpiresAt { get; set; }

    /// <summary>
    /// Raison du refus
    /// </summary>
    public PaymentDeclineReason? DeclineReason { get; set; }

    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Frais de transaction
    /// </summary>
    public decimal? Fees { get; set; }

    /// <summary>
    /// Score de fraude
    /// </summary>
    public int? FraudScore { get; set; }

    /// <summary>
    /// Informations du client
    /// </summary>
    public string? ClientIpAddress { get; set; }
    public string? ClientUserAgent { get; set; }
    public string? GeoLocation { get; set; }

    /// <summary>
    /// Moyen de paiement utilisé
    /// </summary>
    public PaymentMethodSummaryDto? PaymentMethod { get; set; }

    /// <summary>
    /// Transaction parent (pour les remboursements)
    /// </summary>
    public Guid? ParentTransactionId { get; set; }

    /// <summary>
    /// Transactions enfants (remboursements)
    /// </summary>
    public List<TransactionSummaryDto> ChildTransactions { get; set; } = new();

    /// <summary>
    /// Métadonnées
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// DTO pour le résumé d'une transaction
/// </summary>
public class TransactionSummaryDto
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Numéro de transaction
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut de la transaction
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Type de transaction
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Code d'autorisation
    /// </summary>
    public string? AuthorizationCode { get; set; }

    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType? PaymentMethodType { get; set; }
}

/// <summary>
/// DTO pour les détails d'un moyen de paiement
/// </summary>
public class PaymentMethodDetailDto
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nom d'affichage
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Nom d'affichage sécurisé
    /// </summary>
    public string SecureDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Est actif
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Est le moyen par défaut
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de dernière utilisation
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Date d'expiration
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Limites
    /// </summary>
    public decimal? DailyLimit { get; set; }
    public decimal? TransactionLimit { get; set; }

    /// <summary>
    /// Informations de carte (si applicable)
    /// </summary>
    public CreditCardInfoDto? CreditCard { get; set; }

    /// <summary>
    /// Statistiques d'utilisation
    /// </summary>
    public PaymentMethodUsageStatsDto? UsageStats { get; set; }

    /// <summary>
    /// Métadonnées
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// DTO pour le résumé d'un moyen de paiement
/// </summary>
public class PaymentMethodDto
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nom d'affichage sécurisé
    /// </summary>
    public string SecureDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Est actif
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Est le moyen par défaut
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Date d'expiration
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Derniers chiffres (pour cartes)
    /// </summary>
    public string? LastFourDigits { get; set; }

    /// <summary>
    /// Marque de carte (pour cartes)
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Identifiant du client propriétaire
    /// </summary>
    public Guid CustomerId { get; set; }
}

/// <summary>
/// DTO pour résumé d'un moyen de paiement
/// </summary>
public class PaymentMethodSummaryDto
{
    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nom d'affichage sécurisé
    /// </summary>
    public string SecureDisplayName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour les informations de carte de crédit
/// </summary>
public class CreditCardInfoDto
{
    /// <summary>
    /// Marque de la carte
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Derniers 4 chiffres
    /// </summary>
    public string LastFourDigits { get; set; } = string.Empty;

    /// <summary>
    /// Mois d'expiration
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Année d'expiration
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Nom du porteur
    /// </summary>
    public string HolderName { get; set; } = string.Empty;

    /// <summary>
    /// Token de la carte
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// DTO pour les statistiques d'utilisation d'un moyen de paiement
/// </summary>
public class PaymentMethodUsageStatsDto
{
    /// <summary>
    /// Nombre total de transactions
    /// </summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Nombre de transactions réussies
    /// </summary>
    public int SuccessfulTransactions { get; set; }

    /// <summary>
    /// Taux de réussite
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Montant total traité
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Montant moyen par transaction
    /// </summary>
    public decimal AverageTransactionAmount { get; set; }

    /// <summary>
    /// Date de dernière utilisation
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// DTO pour les statistiques de paiement d'un commerçant
/// </summary>
public class MerchantPaymentStatsDto
{
    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Période des statistiques
    /// </summary>
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Devise des montants
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statistiques globales
    /// </summary>
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int CancelledPayments { get; set; }
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Montants
    /// </summary>
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AveragePaymentAmount { get; set; }

    /// <summary>
    /// Frais
    /// </summary>
    public decimal TotalFees { get; set; }

    /// <summary>
    /// Répartition par type de moyen de paiement
    /// </summary>
    public List<PaymentMethodStatsDto> PaymentMethodStats { get; set; } = new();

    /// <summary>
    /// Statistiques par période
    /// </summary>
    public List<PeriodStatsDto> PeriodStats { get; set; } = new();
}

/// <summary>
/// DTO pour les statistiques par type de moyen de paiement
/// </summary>
public class PaymentMethodStatsDto
{
    /// <summary>
    /// Type de moyen de paiement
    /// </summary>
    public PaymentMethodType Type { get; set; }

    /// <summary>
    /// Nombre de paiements
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Montant total
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Pourcentage du total
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Taux de réussite
    /// </summary>
    public decimal SuccessRate { get; set; }
}

/// <summary>
/// DTO pour les statistiques par période
/// </summary>
public class PeriodStatsDto
{
    /// <summary>
    /// Date de la période
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Nombre de paiements
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// Montant total
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Taux de réussite
    /// </summary>
    public decimal SuccessRate { get; set; }
}

/// <summary>
/// DTO simplifié pour les paiements (pour API publique)
/// </summary>
public class PaymentDto
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
    /// Montant du paiement
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Devise
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Statut du paiement
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Identifiant de la commande
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
}

/// <summary>
/// DTO simplifié pour les transactions (pour API publique)
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Numéro de transaction
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

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
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de traitement
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Identifiant du paiement parent
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }
}

/// <summary>
/// DTO pour les statistiques de transactions
/// </summary>
public class TransactionStatsDto
{
    public int TotalTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public int SuccessfulTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public decimal SuccessRate { get; set; }
    public Dictionary<string, int> TransactionsByStatus { get; set; } = new();
    public Dictionary<string, decimal> AmountByStatus { get; set; } = new();
}