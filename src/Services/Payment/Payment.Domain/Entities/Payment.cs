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
    /// Navigation vers le marchand
    /// </summary>
    public Merchant? Merchant { get; set; }

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
    /// Numéro de paiement (alias pour Reference pour compatibilité)
    /// </summary>
    public string PaymentNumber => Reference;

    /// <summary>
    /// Description du paiement
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Statut actuel du paiement
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

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
    /// User agent du navigateur client
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Date de suppression (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

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

    /// <summary>
    /// Définit les données de session du paiement
    /// </summary>
    public void SetSessionData(string sessionId, string? ipAddress = null)
    {
        SessionId = sessionId;
        if (!string.IsNullOrEmpty(ipAddress))
            IpAddress = ipAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marque le paiement comme réussi
    /// </summary>
    public void MarkAsSuccessful()
    {
        UpdateStatus(PaymentStatus.Completed);
    }

    /// <summary>
    /// Marque le paiement comme échoué
    /// </summary>
    public void MarkAsFailed()
    {
        UpdateStatus(PaymentStatus.Failed);
    }

    /// <summary>
    /// Vérifie si le paiement est entièrement payé
    /// </summary>
    public bool IsFullyPaid()
    {
        return Status == PaymentStatus.Completed || Status == PaymentStatus.Captured;
    }

    /// <summary>
    /// Obtient le montant payé (pour l'instant, retourne le montant total si payé)
    /// </summary>
    public Money GetPaidAmount()
    {
        return IsFullyPaid() ? Amount : Money.Zero(Amount.Currency);
    }

    /// <summary>
    /// Obtient le montant remboursé (pour l'instant, retourne zéro - à implémenter avec les remboursements)
    /// </summary>
    public Money GetRefundedAmount()
    {
        // TODO: Implémenter avec la logique des remboursements
        return Money.Zero(Amount.Currency);
    }

    /// <summary>
    /// Obtient le montant net (montant - remboursements)
    /// </summary>
    public Money GetNetAmount()
    {
        return GetPaidAmount().Subtract(GetRefundedAmount());
    }

    /// <summary>
    /// Frais de traitement (pour l'instant, retourne null - à implémenter)
    /// </summary>
    public Money? ProcessingFees => null;

    /// <summary>
    /// Mode de frais (pour l'instant, par défaut)
    /// </summary>
    public string FeeMode => "Standard";

    /// <summary>
    /// Montant minimum pour paiement partiel (pour l'instant, null)
    /// </summary>
    public Money? MinimumPartialAmount => null;

    /// <summary>
    /// Dernière méthode de paiement utilisée (basée sur la dernière transaction)
    /// </summary>
    public PaymentMethod? LastPaymentMethod => _transactions.LastOrDefault()?.PaymentMethod;

    /// <summary>
    /// Collection privée de transactions
    /// </summary>
    private readonly List<Transaction> _transactions = new();

    /// <summary>
    /// Collection publique en lecture seule des transactions
    /// </summary>
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    /// <summary>
    /// Collections de remboursements associés à ce paiement  
    /// </summary>
    public virtual ICollection<PaymentRefund> Refunds { get; private set; } = new List<PaymentRefund>();

    /// <summary>
    /// Données de session (pour l'instant, dictionnaire simple)
    /// </summary>
    public Dictionary<string, string> SessionData => Metadata;

    /// <summary>
    /// Autorise les paiements partiels (pour l'instant, false par défaut)
    /// </summary>
    public bool AllowPartialPayments => false;

    /// <summary>
    /// Vérifie si un paiement partiel est valide
    /// </summary>
    public bool IsPartialPaymentValid(Money partialAmount)
    {
        if (!AllowPartialPayments) return false;
        if (MinimumPartialAmount != null)
            return partialAmount.IsGreaterThan(MinimumPartialAmount);
        return partialAmount.IsGreaterThan(Money.Zero(Amount.Currency));
    }

    /// <summary>
    /// Ajoute une transaction existante au paiement
    /// </summary>
    public Transaction AddTransaction(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        _transactions.Add(transaction);
        UpdatedAt = DateTime.UtcNow;
        return transaction;
    }

    /// <summary>
    /// Ajoute une nouvelle transaction avec paramètres (surcharge pour compatibilité Application layer)
    /// </summary>
    public Transaction AddTransaction(Money amount, TransactionType transactionType, PaymentMethod paymentMethod, string? ipAddress = null, string? userAgent = null)
    {
        // Créer une nouvelle transaction en utilisant le constructeur public
        var transaction = new Transaction(
            amount: amount,
            type: transactionType,
            paymentMethod: paymentMethod,
            customerId: CustomerId,
            merchantId: MerchantId,
            orderId: OrderId,
            clientIpAddress: ipAddress,
            clientUserAgent: userAgent
        );
        
        _transactions.Add(transaction);
        UpdatedAt = DateTime.UtcNow;
        return transaction;
    }

    /// <summary>
    /// Vérifie si le paiement peut être retenté (pour l'instant, basé sur le statut)
    /// </summary>
    public bool CanRetry()
    {
        return Status == PaymentStatus.Failed || Status == PaymentStatus.Cancelled;
    }

    /// <summary>
    /// Configure la description du paiement (utilise les métadonnées)
    /// </summary>
    public void SetDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description))
        {
            Metadata["description"] = description;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configure le timeout du paiement 
    /// </summary>
    public void SetTimeout(int timeoutMinutes)
    {
        // Configure l'expiration basée sur le timeout
        ExpiresAt = DateTime.UtcNow.AddMinutes(timeoutMinutes);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configure le nombre maximum de tentatives (via métadonnées)
    /// </summary>
    public void SetMaxAttempts(int maxAttempts)
    {
        Metadata["maxAttempts"] = maxAttempts.ToString();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configure les paiements partiels (via métadonnées pour l'instant)
    /// </summary>
    public void SetPartialPayment(bool allowPartialPayments, Money? minimumPartialAmount = null)
    {
        Metadata["allowPartialPayments"] = allowPartialPayments.ToString();
        if (minimumPartialAmount != null)
        {
            Metadata["minimumPartialAmount"] = minimumPartialAmount.Amount.ToString();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configure les URLs de callback
    /// </summary>
    public void SetUrls(string? successUrl, string? failureUrl, string? webhookUrl)
    {
        ReturnUrl = successUrl;
        CancelUrl = failureUrl;
        WebhookUrl = webhookUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}