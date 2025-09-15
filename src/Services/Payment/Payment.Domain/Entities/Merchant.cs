using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant un marchand
/// </summary>
public class Merchant : BaseEntity
{
    /// <summary>
    /// Nom du marchand
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Email du marchand
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Numéro de téléphone du marchand
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Adresse du marchand
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Code pays ISO du marchand
    /// </summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>
    /// Code devise du marchand
    /// </summary>
    public string CurrencyCode { get; private set; } = string.Empty;

    /// <summary>
    /// Statut du marchand
    /// </summary>
    public MerchantStatus Status { get; private set; }

    /// <summary>
    /// Clé API du marchand
    /// </summary>
    public string ApiKey { get; private set; } = string.Empty;

    /// <summary>
    /// URL de webhook pour les notifications
    /// </summary>
    public string? WebhookUrl { get; private set; }

    /// <summary>
    /// Token secret pour les webhooks
    /// </summary>
    public string? WebhookSecret { get; private set; }

    /// <summary>
    /// Date de création du compte marchand
    /// </summary>
    public new DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de dernière mise à jour
    /// </summary>
    public new DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Constructeur privé pour Entity Framework
    /// </summary>
    private Merchant() { }

    /// <summary>
    /// Constructeur principal
    /// </summary>
    public Merchant(
        string name,
        string email,
        string countryCode,
        string currencyCode,
        string? phone = null,
        string? address = null,
        string? webhookUrl = null)
    {
        ValidateConstructorParameters(name, email, countryCode, currencyCode);

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        CountryCode = countryCode;
        CurrencyCode = currencyCode;
        Status = MerchantStatus.Pending;
        ApiKey = GenerateApiKey();
        WebhookUrl = webhookUrl;
        WebhookSecret = GenerateWebhookSecret();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Met à jour les informations du marchand
    /// </summary>
    public void UpdateInfo(string name, string email, string? phone = null, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Active le marchand
    /// </summary>
    public void Activate()
    {
        if (Status == MerchantStatus.Blocked)
            throw new InvalidOperationException("Cannot activate a blocked merchant");

        Status = MerchantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspend le marchand
    /// </summary>
    public void Suspend()
    {
        Status = MerchantStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bloque le marchand
    /// </summary>
    public void Block()
    {
        Status = MerchantStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Met à jour l'URL de webhook
    /// </summary>
    public void UpdateWebhook(string? webhookUrl)
    {
        WebhookUrl = webhookUrl;
        WebhookSecret = GenerateWebhookSecret();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Régénère la clé API
    /// </summary>
    public void RegenerateApiKey()
    {
        ApiKey = GenerateApiKey();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifie si le marchand est actif
    /// </summary>
    public bool IsActive => Status == MerchantStatus.Active;

    /// <summary>
    /// Valide les paramètres du constructeur
    /// </summary>
    private static void ValidateConstructorParameters(string name, string email, string countryCode, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be empty", nameof(countryCode));
        
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty", nameof(currencyCode));
    }

    /// <summary>
    /// Génère une clé API unique
    /// </summary>
    private static string GenerateApiKey()
    {
        return $"pk_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Génère un secret pour les webhooks
    /// </summary>
    private static string GenerateWebhookSecret()
    {
        return $"whsec_{Guid.NewGuid():N}";
    }
}