namespace Payment.Domain.Enums;

/// <summary>
/// Statuts d'authentification 3D Secure
/// </summary>
public enum ThreeDSecureStatus
{
    /// <summary>
    /// Non trouvé
    /// </summary>
    NotFound = 0,

    /// <summary>
    /// Non requis
    /// </summary>
    NotRequired = 1,

    /// <summary>
    /// En attente
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Réussi
    /// </summary>
    Successful = 3,

    /// <summary>
    /// Échoué
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Abandonné
    /// </summary>
    Abandoned = 5,

    /// <summary>
    /// Erreur
    /// </summary>
    Error = 6
}