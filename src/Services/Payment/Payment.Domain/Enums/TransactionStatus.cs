namespace Payment.Domain.Enums;

/// <summary>
/// Statuts possibles d'une transaction
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction en attente
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction en cours de traitement
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Transaction réussie
    /// </summary>
    Successful = 2,

    /// <summary>
    /// Transaction complétée (alias pour Successful)
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Transaction échouée
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Transaction annulée
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Transaction remboursée
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// Transaction partiellement remboursée
    /// </summary>
    PartiallyRefunded = 6,

    /// <summary>
    /// Transaction expirée
    /// </summary>
    Expired = 7
}