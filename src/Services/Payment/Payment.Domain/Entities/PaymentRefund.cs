using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant un remboursement de paiement
/// </summary>
public class PaymentRefund : BaseEntity
{
    /// <summary>
    /// ID du paiement original
    /// </summary>
    public Guid PaymentId { get; private set; }

    /// <summary>
    /// Référence au paiement original
    /// </summary>
    public Payment Payment { get; private set; } = null!;

    /// <summary>
    /// Numéro unique du remboursement
    /// </summary>
    public string RefundNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Référence unique du remboursement (alias pour RefundNumber)
    /// </summary>
    [NotMapped]
    public string Reference => RefundNumber;

    /// <summary>
    /// Identifiant du remboursement côté processeur de paiement
    /// </summary>
    public string? ProcessorRefundId { get; private set; }

    /// <summary>
    /// Réponse du processeur de paiement
    /// </summary>
    public string? ProcessorResponse { get; private set; }

    /// <summary>
    /// Montant du remboursement
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Raison du remboursement
    /// </summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// Statut du remboursement
    /// </summary>
    public RefundStatus Status { get; private set; }

    /// <summary>
    /// ID de la transaction de remboursement externe
    /// </summary>
    public string? ExternalRefundId { get; private set; }

    /// <summary>
    /// ID de l'utilisateur qui a initié le remboursement
    /// </summary>
    public Guid InitiatedBy { get; private set; }

    /// <summary>
    /// Date de création du remboursement
    /// </summary>
    public new DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de traitement du remboursement
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Date de finalisation du remboursement
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Commentaires additionnels
    /// </summary>
    public string? Comments { get; private set; }

    /// <summary>
    /// Constructeur privé pour Entity Framework
    /// </summary>
    private PaymentRefund() { }

    /// <summary>
    /// Constructeur principal
    /// </summary>
    public PaymentRefund(
        Guid paymentId,
        Money amount,
        string reason,
        Guid initiatedBy,
        string? comments = null)
    {
        ValidateConstructorParameters(paymentId, amount, reason, initiatedBy);

        Id = Guid.NewGuid();
        PaymentId = paymentId;
        RefundNumber = GenerateRefundNumber();
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Pending;
        InitiatedBy = initiatedBy;
        Comments = comments;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marque le remboursement comme en cours de traitement
    /// </summary>
    public void MarkAsProcessing(string? externalRefundId = null)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot process refund in status {Status}");

        Status = RefundStatus.Processing;
        ExternalRefundId = externalRefundId;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer l'identifiant du processeur
    /// </summary>
    public void SetProcessorRefundId(string processorRefundId)
    {
        ProcessorRefundId = processorRefundId ?? throw new ArgumentNullException(nameof(processorRefundId));
    }

    /// <summary>
    /// Configurer la réponse du processeur
    /// </summary>
    public void SetProcessorResponse(string processorResponse)
    {
        ProcessorResponse = processorResponse;
    }

    /// <summary>
    /// Marque le remboursement comme réussi
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status != RefundStatus.Processing)
            throw new InvalidOperationException($"Cannot complete refund in status {Status}");

        Status = RefundStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marque le remboursement comme échoué
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        if (Status == RefundStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed refund");

        Status = RefundStatus.Failed;
        Comments = string.IsNullOrEmpty(Comments) ? reason : $"{Comments}. Failure: {reason}";
    }

    /// <summary>
    /// Annule le remboursement
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel refund in status {Status}");

        Status = RefundStatus.Cancelled;
        Comments = string.IsNullOrEmpty(Comments) ? reason : $"{Comments}. Cancelled: {reason}";
    }

    /// <summary>
    /// Vérifie si le remboursement est finalisé
    /// </summary>
    public bool IsFinalized => Status == RefundStatus.Completed || Status == RefundStatus.Failed || Status == RefundStatus.Cancelled;

    /// <summary>
    /// Valide les paramètres du constructeur
    /// </summary>
    private static void ValidateConstructorParameters(Guid paymentId, Money amount, string reason, Guid initiatedBy)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("Payment ID cannot be empty");

        if (amount == null)
            throw new ArgumentNullException(nameof(amount));

        if (amount.Amount <= 0)
            throw new ArgumentException("Refund amount must be positive");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty");

        if (initiatedBy == Guid.Empty)
            throw new ArgumentException("InitiatedBy cannot be empty");
    }

    /// <summary>
    /// Génère un numéro de remboursement unique
    /// </summary>
    private static string GenerateRefundNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return $"REF{timestamp}{random}";
    }
}