namespace Payment.Domain.Enums;

/// <summary>
/// Statuts possibles d'un remboursement
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// Remboursement en attente de traitement
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Remboursement en cours de traitement
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Remboursement réussi
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Remboursement échoué
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Remboursement annulé
    /// </summary>
    Cancelled = 4
}