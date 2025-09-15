namespace Payment.Domain.Enums;

/// <summary>
/// Statuts possibles d'un paiement
/// Avec workflow sécurisé pour transactions financières
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Paiement en cours de création
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Paiement autorisé mais pas encore capturé
    /// </summary>
    Authorized = 1,

    /// <summary>
    /// Paiement capturé et confirmé
    /// </summary>
    Captured = 2,

    /// <summary>
    /// Paiement réussi et finalisé
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Paiement échoué
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Paiement annulé
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Paiement remboursé (partiellement ou totalement)
    /// </summary>
    Refunded = 6,

    /// <summary>
    /// Paiement en cours de traitement
    /// </summary>
    Processing = 7,

    /// <summary>
    /// Paiement en attente de validation
    /// </summary>
    PendingValidation = 8,

    /// <summary>
    /// Paiement expiré
    /// </summary>
    Expired = 9,

    /// <summary>
    /// Paiement réglé et finalisé
    /// </summary>
    Settled = 10
}

/// <summary>
/// Extensions pour PaymentStatus
/// </summary>
public static class PaymentStatusExtensions
{
    /// <summary>
    /// Vérifier si le statut indique un paiement finalisé
    /// </summary>
    public static bool IsFinalized(this PaymentStatus status)
    {
        return status is PaymentStatus.Completed or PaymentStatus.Failed or 
               PaymentStatus.Cancelled or PaymentStatus.Refunded or PaymentStatus.Expired;
    }

    /// <summary>
    /// Vérifier si le statut indique un paiement réussi
    /// </summary>
    public static bool IsSuccessful(this PaymentStatus status)
    {
        return status is PaymentStatus.Completed or PaymentStatus.Captured or PaymentStatus.Settled;
    }

    /// <summary>
    /// Vérifier si le paiement peut être annulé
    /// </summary>
    public static bool CanBeCancelled(this PaymentStatus status)
    {
        return status is PaymentStatus.Pending or PaymentStatus.Authorized or PaymentStatus.PendingValidation;
    }

    /// <summary>
    /// Vérifier si le paiement peut être remboursé
    /// </summary>
    public static bool CanBeRefunded(this PaymentStatus status)
    {
        return status is PaymentStatus.Completed or PaymentStatus.Captured or PaymentStatus.Settled;
    }

    /// <summary>
    /// Obtenir les statuts suivants possibles
    /// </summary>
    public static IEnumerable<PaymentStatus> GetPossibleNextStatuses(this PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => new[] { PaymentStatus.Authorized, PaymentStatus.Processing, PaymentStatus.Failed, PaymentStatus.Cancelled, PaymentStatus.Expired },
            PaymentStatus.Authorized => new[] { PaymentStatus.Captured, PaymentStatus.Cancelled, PaymentStatus.Expired },
            PaymentStatus.Captured => new[] { PaymentStatus.Completed, PaymentStatus.Refunded },
            PaymentStatus.Processing => new[] { PaymentStatus.Completed, PaymentStatus.Failed, PaymentStatus.PendingValidation },
            PaymentStatus.PendingValidation => new[] { PaymentStatus.Completed, PaymentStatus.Failed, PaymentStatus.Cancelled },
            _ => Array.Empty<PaymentStatus>()
        };
    }
}