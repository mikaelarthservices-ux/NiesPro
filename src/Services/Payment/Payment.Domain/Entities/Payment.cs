using Payment.Domain.Enums;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant un paiement dans le système
/// Gère les transactions financières sécurisées avec validation métier
/// </summary>
public class Payment
{
    /// <summary>
    /// Identifiant unique du paiement
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Identifiant du marchand
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Identifiant de la commande associée
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Référence unique du paiement
    /// Utilisée pour les réconciliations et le suivi client
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Description du paiement
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Statut actuel du paiement
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Méthode de paiement utilisée
    /// </summary>
    public PaymentMethodType Method { get; set; }

    /// <summary>
    /// Montant du paiement avec devise
    /// </summary>
    public Money Amount { get; set; } = Money.Zero("EUR");

    /// <summary>
    /// Métadonnées additionnelles du paiement
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// URL de retour après paiement réussi
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// URL de retour après annulation
    /// </summary>
    public string? CancelUrl { get; set; }

    /// <summary>
    /// URL de webhook pour les notifications
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Date de création du paiement
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de dernière mise à jour
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Date d'expiration du paiement
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Date de confirmation du paiement
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Identifiant de session
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Adresse IP du client
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Payment()
    {
        Id = Guid.NewGuid();
        Reference = GenerateReference();
    }

    /// <summary>
    /// Constructeur avec paramètres requis
    /// </summary>
    public Payment(Guid customerId, Guid merchantId, Guid orderId, Money amount, PaymentMethodType method)
        : this()
    {
        CustomerId = customerId;
        MerchantId = merchantId;
        OrderId = orderId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Method = method;
    }

    /// <summary>
    /// Génère une référence unique pour le paiement
    /// </summary>
    private static string GenerateReference()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = Random.Shared.Next(1000, 9999);
        return $"PAY-{timestamp}-{random}";
    }

    /// <summary>
    /// Met à jour le statut du paiement
    /// </summary>
    public void UpdateStatus(PaymentStatus newStatus)
    {
        var possibleStatuses = Status.GetPossibleNextStatuses();
        if (!possibleStatuses.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Impossible de passer du statut {Status} au statut {newStatus}");
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == PaymentStatus.Completed || newStatus == PaymentStatus.Captured)
        {
            ConfirmedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Vérifie si le paiement a expiré
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    /// <summary>
    /// Vérifie si le paiement est dans un état final
    /// </summary>
    public bool IsInFinalState()
    {
        return Status.IsFinalized();
    }

    /// <summary>
    /// Ajoute des métadonnées au paiement
    /// </summary>
    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clé ne peut pas être vide", nameof(key));

        Metadata[key] = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définit l'expiration du paiement
    /// </summary>
    public void SetExpiration(DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("La date d'expiration doit être dans le futur", nameof(expiresAt));

        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}