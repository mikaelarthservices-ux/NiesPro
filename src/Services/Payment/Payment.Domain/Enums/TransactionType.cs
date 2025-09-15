namespace Payment.Domain.Enums;

/// <summary>
/// Types de transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Paiement normal
    /// </summary>
    Payment = 0,

    /// <summary>
    /// Remboursement
    /// </summary>
    Refund = 1,

    /// <summary>
    /// Autorisation uniquement
    /// </summary>
    Authorization = 2,

    /// <summary>
    /// Capture différée
    /// </summary>
    Capture = 3,

    /// <summary>
    /// Annulation
    /// </summary>
    Void = 4,

    /// <summary>
    /// Pré-autorisation
    /// </summary>
    PreAuthorization = 5
}