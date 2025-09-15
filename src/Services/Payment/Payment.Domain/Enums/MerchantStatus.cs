namespace Payment.Domain.Enums;

/// <summary>
/// Statuts possibles d'un marchand
/// </summary>
public enum MerchantStatus
{
    /// <summary>
    /// Marchand en attente d'activation
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Marchand actif et autorisé à traiter des paiements
    /// </summary>
    Active = 1,

    /// <summary>
    /// Marchand temporairement suspendu
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Marchand bloqué définitivement
    /// </summary>
    Blocked = 3
}