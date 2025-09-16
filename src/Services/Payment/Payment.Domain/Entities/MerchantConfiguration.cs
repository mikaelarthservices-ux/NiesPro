namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant la configuration d'un marchand
/// Stocke les paramètres de configuration par marchand
/// </summary>
public class MerchantConfiguration
{
    /// <summary>
    /// Identifiant unique de la configuration
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du marchand
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Clé de configuration
    /// </summary>
    public string ConfigurationKey { get; set; } = string.Empty;

    /// <summary>
    /// Valeur de configuration
    /// </summary>
    public string ConfigurationValue { get; set; } = string.Empty;

    /// <summary>
    /// Valeur chiffrée (si applicable)
    /// </summary>
    public string? EncryptedValue { get; set; }

    /// <summary>
    /// Indique si la valeur est chiffrée
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Navigation vers le marchand
    /// </summary>
    public virtual Merchant? Merchant { get; set; }
}