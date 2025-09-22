using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using System.ComponentModel.DataAnnotations.Schema;
using NiesPro.Contracts.Primitives;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant une transaction de paiement
/// </summary>
public class Transaction : Entity
{
    /// <summary>
    /// Numéro de transaction unique
    /// </summary>
    public string TransactionNumber { get; private set; }

    /// <summary>
    /// Montant de la transaction
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// Statut de la transaction
    /// </summary>
    public TransactionStatus Status { get; private set; }

    /// <summary>
    /// Type de transaction
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// Moyen de paiement utilisé
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Identifiant du moyen de paiement
    /// </summary>
    public Guid PaymentMethodId { get; private set; }

    /// <summary>
    /// Identifiant de la commande associée
    /// </summary>
    public Guid? OrderId { get; private set; }

    /// <summary>
    /// Identifiant du client
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Identifiant du commerçant
    /// </summary>
    public Guid MerchantId { get; private set; }

    /// <summary>
    /// Référence externe de la transaction (gateway, banque)
    /// </summary>
    public string? ExternalReference { get; private set; }

    /// <summary>
    /// Code d'autorisation (pour les cartes)
    /// </summary>
    public string? AuthorizationCode { get; private set; }

    /// <summary>
    /// Raison du refus en cas d'échec
    /// </summary>
    public PaymentDeclineReason? DeclineReason { get; private set; }

    /// <summary>
    /// Message d'erreur détaillé
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Adresse IP du client
    /// </summary>
    public string? ClientIpAddress { get; private set; }

    /// <summary>
    /// User Agent du client
    /// </summary>
    public string? ClientUserAgent { get; private set; }

    /// <summary>
    /// Date de traitement de la transaction
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Date d'expiration de l'autorisation
    /// </summary>
    public DateTime? AuthorizationExpiresAt { get; private set; }

    /// <summary>
    /// Montant des frais de transaction
    /// </summary>
    public Money? Fees { get; private set; }

    /// <summary>
    /// Taux de change appliqué (si conversion de devise)
    /// </summary>
    public decimal? ExchangeRate { get; private set; }

    /// <summary>
    /// Devise originale (avant conversion)
    /// </summary>
    public string? OriginalCurrency { get; private set; }

    /// <summary>
    /// Données de géolocalisation
    /// </summary>
    public string? GeoLocation { get; private set; }

    /// <summary>
    /// Score de risque de fraude (0-100)
    /// </summary>
    public int? FraudScore { get; private set; }

    /// <summary>
    /// Identifiant du paiement associé (Foreign Key)
    /// </summary>
    public Guid? PaymentId { get; private set; }

    /// <summary>
    /// Navigation vers le paiement associé
    /// </summary>
    public virtual Payment? Payment { get; private set; }

    /// <summary>
    /// Identifiant de transaction du processeur de paiement
    /// </summary>
    public string? ProcessorTransactionId { get; private set; }

    /// <summary>
    /// Identifiant de transaction de la passerelle
    /// </summary>
    public string? GatewayTransactionId { get; private set; }

    /// <summary>
    /// Adresse IP pour la transaction (alias vers ClientIpAddress)
    /// </summary>
    [NotMapped]
    public string? IpAddress => ClientIpAddress;

    /// <summary>
    /// Description de la transaction
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Réponse du processeur de paiement
    /// </summary>
    public string? ProcessorResponse { get; private set; }

    /// <summary>
    /// Raison de l'échec de la transaction
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Empreinte digitale de l'appareil
    /// </summary>
    public string? DeviceFingerprint { get; private set; }

    /// <summary>
    /// Frais de transaction (alias vers Fees pour compatibilité EF)
    /// </summary>
    [NotMapped]
    public Money? Fee => Fees;

    /// <summary>
    /// Métadonnées additionnelles
    /// </summary>
    public Dictionary<string, string> Metadata { get; private set; }

    /// <summary>
    /// Transaction parent (pour les remboursements)
    /// </summary>
    public Transaction? ParentTransaction { get; private set; }

    /// <summary>
    /// Identifiant de la transaction parent
    /// </summary>
    public Guid? ParentTransactionId { get; private set; }

    /// <summary>
    /// Transactions enfants (remboursements partiels)
    /// </summary>
    public virtual ICollection<Transaction> ChildTransactions { get; private set; }

    /// <summary>
    /// Constructeur protégé pour Entity Framework
    /// </summary>
    protected Transaction()
    {
        TransactionNumber = string.Empty;
        Amount = Money.Zero("EUR"); // Devise par défaut
        PaymentMethod = null!;
        Metadata = new Dictionary<string, string>();
        ChildTransactions = new List<Transaction>();
    }

    /// <summary>
    /// Constructeur pour créer une nouvelle transaction
    /// </summary>
    public Transaction(
        Money amount,
        TransactionType type,
        PaymentMethod paymentMethod,
        Guid customerId,
        Guid merchantId,
        Guid? orderId = null,
        string? clientIpAddress = null,
        string? clientUserAgent = null)
    {
        ValidateConstructorParameters(amount, paymentMethod, customerId, merchantId);

        Id = Guid.NewGuid();
        TransactionNumber = GenerateTransactionNumber();
        Amount = amount;
        Type = type;
        PaymentMethod = paymentMethod;
        PaymentMethodId = paymentMethod.Id;
        CustomerId = customerId;
        MerchantId = merchantId;
        OrderId = orderId;
        ClientIpAddress = clientIpAddress;
        ClientUserAgent = clientUserAgent;
        Status = TransactionStatus.Pending;
        Metadata = new Dictionary<string, string>();
        ChildTransactions = new List<Transaction>();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Événement de création
        AddDomainEvent(new TransactionCreatedEvent(Id, TransactionNumber, Amount, Type, CustomerId));
    }

    /// <summary>
    /// Marquer la transaction comme autorisée
    /// </summary>
    public void Authorize(string authorizationCode, DateTime? expiresAt = null)
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"Cannot authorize transaction in status {Status}");

        Status = TransactionStatus.Successful;
        AuthorizationCode = authorizationCode;
        AuthorizationExpiresAt = expiresAt ?? DateTime.UtcNow.AddHours(7); // Défaut 7 jours
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionAuthorizedEvent(Id, TransactionNumber, Amount, authorizationCode));
    }

    /// <summary>
    /// Capturer la transaction (finaliser le paiement)
    /// </summary>
    public void Capture(Money? captureAmount = null, Money? fees = null)
    {
        if (Status != TransactionStatus.Processing)
            throw new InvalidOperationException($"Cannot capture transaction in status {Status}");

        var amountToCapture = captureAmount ?? Amount;
        
        if (amountToCapture > Amount)
            throw new ArgumentException("Capture amount cannot exceed authorized amount");

        if (AuthorizationExpiresAt.HasValue && DateTime.UtcNow > AuthorizationExpiresAt.Value)
            throw new InvalidOperationException("Authorization has expired");

        // Si capture partielle, créer une nouvelle transaction pour le reste
        if (amountToCapture < Amount)
        {
            CreatePartialCaptureTransaction(amountToCapture);
        }

        Status = TransactionStatus.Successful;
        Amount = amountToCapture;
        Fees = fees;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionCapturedEvent(Id, TransactionNumber, amountToCapture, fees));
    }

    /// <summary>
    /// Refuser la transaction
    /// </summary>
    public void Decline(PaymentDeclineReason reason, string? errorMessage = null)
    {
        if (Status is TransactionStatus.Successful or TransactionStatus.Refunded)
            throw new InvalidOperationException($"Cannot decline transaction in status {Status}");

        Status = TransactionStatus.Failed;
        DeclineReason = reason;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionDeclinedEvent(Id, TransactionNumber, reason, errorMessage));
    }

    /// <summary>
    /// Annuler la transaction
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status is TransactionStatus.Successful or TransactionStatus.Refunded)
            throw new InvalidOperationException($"Cannot cancel transaction in status {Status}. Use refund instead.");

        Status = TransactionStatus.Cancelled;
        ErrorMessage = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionCancelledEvent(Id, TransactionNumber, reason));
    }

    /// <summary>
    /// Rembourser la transaction
    /// </summary>
    public Transaction Refund(Money refundAmount, string? reason = null)
    {
        if (Status != TransactionStatus.Successful)
            throw new InvalidOperationException($"Cannot refund transaction in status {Status}");

        if (refundAmount > Amount)
            throw new ArgumentException("Refund amount cannot exceed captured amount");

        if (refundAmount <= Money.Zero(Amount.Currency))
            throw new ArgumentException("Refund amount must be positive");

        // Créer une transaction de remboursement
        var refundTransaction = new Transaction(
            refundAmount,
            TransactionType.Refund,
            PaymentMethod,
            CustomerId,
            MerchantId)
        {
            ParentTransactionId = Id,
            ParentTransaction = this
        };

        if (!string.IsNullOrEmpty(reason))
            refundTransaction.SetMetadata("refund_reason", reason);

        // Marquer immédiatement comme réussie car c'est un remboursement
        refundTransaction.Status = TransactionStatus.Successful;
        refundTransaction.ProcessedAt = DateTime.UtcNow;

        ChildTransactions.Add(refundTransaction);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionRefundedEvent(Id, refundTransaction.Id, refundAmount, reason));

        return refundTransaction;
    }

    /// <summary>
    /// Marquer comme réglée (settlement)
    /// </summary>
    public void Settle()
    {
        if (Status != TransactionStatus.Successful)
            throw new InvalidOperationException($"Cannot settle transaction in status {Status}");

        Status = TransactionStatus.Successful; // Ou garder le même statut
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TransactionSettledEvent(Id, TransactionNumber, Amount));
    }

    /// <summary>
    /// Définir la référence externe
    /// </summary>
    public void SetExternalReference(string externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
            throw new ArgumentException("External reference cannot be null or empty");

        ExternalReference = externalReference;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir le score de fraude
    /// </summary>
    public void SetFraudScore(int score)
    {
        if (score < 0 || score > 100)
            throw new ArgumentException("Fraud score must be between 0 and 100");

        FraudScore = score;
        UpdatedAt = DateTime.UtcNow;

        // Si score élevé, déclencher un événement
        if (score >= 80)
        {
            AddDomainEvent(new HighFraudRiskDetectedEvent(Id, TransactionNumber, score));
        }
    }

    /// <summary>
    /// Ajouter des métadonnées
    /// </summary>
    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty");

        Metadata[key] = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculer le montant total remboursé
    /// </summary>
    public Money GetTotalRefundedAmount()
    {
        return ChildTransactions
            .Where(t => t.Type == TransactionType.Refund && t.Status == TransactionStatus.Successful)
            .Aggregate(Money.Zero(Amount.Currency), (sum, refund) => sum + refund.Amount);
    }

    /// <summary>
    /// Vérifier si la transaction peut être remboursée
    /// </summary>
    public bool CanBeRefunded()
    {
        return Status is TransactionStatus.Successful &&
               GetTotalRefundedAmount() < Amount;
    }

    /// <summary>
    /// Obtenir le montant disponible pour remboursement
    /// </summary>
    public Money GetRefundableAmount()
    {
        return Amount - GetTotalRefundedAmount();
    }

    /// <summary>
    /// Ajouter un événement de domaine
    /// </summary>
    private new void AddDomainEvent(NiesPro.Contracts.Primitives.IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
    }

    /// <summary>
    /// Créer une transaction pour capture partielle
    /// </summary>
    private void CreatePartialCaptureTransaction(Money capturedAmount)
    {
        var remainingAmount = Amount - capturedAmount;
        // Logique pour gérer le montant restant (void automatique ou manuel)
        SetMetadata("partial_capture", "true");
        SetMetadata("remaining_amount", remainingAmount.ToString());
    }

    /// <summary>
    /// Générer un numéro de transaction unique
    /// </summary>
    private static string GenerateTransactionNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return $"TXN{timestamp}{random}";
    }

    /// <summary>
    /// Validation des paramètres du constructeur
    /// </summary>
    private static void ValidateConstructorParameters(
        Money amount, 
        PaymentMethod paymentMethod, 
        Guid customerId, 
        Guid merchantId)
    {
        if (amount <= Money.Zero(amount.Currency))
            throw new ArgumentException("Transaction amount must be positive");

        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        if (!paymentMethod.CanBeUsed())
            throw new ArgumentException("Payment method cannot be used");

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty");

        if (merchantId == Guid.Empty)
            throw new ArgumentException("Merchant ID cannot be empty");
    }
}