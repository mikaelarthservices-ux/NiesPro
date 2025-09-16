using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

/// <summary>
/// Entité représentant une authentification 3D Secure
/// </summary>
public class ThreeDSecureAuthentication
{
    /// <summary>
    /// Identifiant unique
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Identifiant de la transaction
    /// </summary>
    public string TransactionId { get; private set; } = string.Empty;

    /// <summary>
    /// Identifiant de la carte
    /// </summary>
    public Guid CardId { get; private set; }

    /// <summary>
    /// Statut de l'authentification
    /// </summary>
    public ThreeDSecureStatus Status { get; private set; }

    /// <summary>
    /// URL de redirection pour l'authentification
    /// </summary>
    public string? AuthenticationUrl { get; private set; }

    /// <summary>
    /// Token d'authentification
    /// </summary>
    public string? AuthenticationToken { get; private set; }

    /// <summary>
    /// Résultat de l'authentification
    /// </summary>
    public string? AuthenticationResult { get; private set; }

    /// <summary>
    /// Code de réponse du fournisseur
    /// </summary>
    public string? ProviderResponseCode { get; private set; }

    /// <summary>
    /// Message du fournisseur
    /// </summary>
    public string? ProviderMessage { get; private set; }

    /// <summary>
    /// Version du protocole 3DS
    /// </summary>
    public string? ProtocolVersion { get; private set; }

    /// <summary>
    /// Fournisseur 3DS (Visa, Mastercard, etc.)
    /// </summary>
    public string? Provider { get; private set; }

    /// <summary>
    /// Cardholder Authentication Verification Value
    /// </summary>
    public string? Cavv { get; private set; }

    /// <summary>
    /// Electronic Commerce Indicator
    /// </summary>
    public string? Eci { get; private set; }

    /// <summary>
    /// Transaction Identifier
    /// </summary>
    public string? Xid { get; private set; }

    /// <summary>
    /// Directory Server Transaction ID
    /// </summary>
    public string? DirectoryServerTransactionId { get; private set; }

    /// <summary>
    /// URL ACS (Access Control Server)
    /// </summary>
    public string? AcsUrl { get; private set; }

    /// <summary>
    /// Payer Authentication Request
    /// </summary>
    public string? PaReq { get; private set; }

    /// <summary>
    /// Merchant Data
    /// </summary>
    public string? Md { get; private set; }

    /// <summary>
    /// Valeur d'authentification 3DS
    /// </summary>
    public string? AuthenticationValue { get; private set; }

    /// <summary>
    /// Message d'erreur (si applicable)
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Identifiant du client (pour requêtes)
    /// </summary>
    public Guid? CustomerId { get; private set; }

    /// <summary>
    /// Navigation vers la carte associée
    /// </summary>
    public virtual Card? Card { get; private set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Date de completion
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Date d'expiration
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Constructeur pour Entity Framework
    /// </summary>
    private ThreeDSecureAuthentication() { }

    /// <summary>
    /// Constructeur principal
    /// </summary>
    public ThreeDSecureAuthentication(string transactionId, Guid cardId, string? provider = null, string? authenticationUrl = null, string? authenticationToken = null)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId ?? throw new ArgumentNullException(nameof(transactionId));
        CardId = cardId;
        Provider = provider;
        Status = ThreeDSecureStatus.Pending;
        AuthenticationUrl = authenticationUrl;
        AuthenticationToken = authenticationToken;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15); // Expiration standard 3DS
    }

    /// <summary>
    /// Marquer comme réussi
    /// </summary>
    public void MarkAsSuccessful(string authenticationResult, string? cavv = null, string? eci = null, string? xid = null, string? directoryServerTransactionId = null, string? providerResponseCode = null, string? providerMessage = null)
    {
        Status = ThreeDSecureStatus.Successful;
        AuthenticationResult = authenticationResult;
        Cavv = cavv;
        Eci = eci;
        Xid = xid;
        DirectoryServerTransactionId = directoryServerTransactionId;
        ProviderResponseCode = providerResponseCode;
        ProviderMessage = providerMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Définir la valeur CAVV
    /// </summary>
    public void SetCavv(string cavv)
    {
        Cavv = cavv ?? throw new ArgumentNullException(nameof(cavv));
    }

    /// <summary>
    /// Définir la valeur ECI
    /// </summary>
    public void SetEci(string eci)
    {
        Eci = eci ?? throw new ArgumentNullException(nameof(eci));
    }

    /// <summary>
    /// Définir la valeur XID
    /// </summary>
    public void SetXid(string xid)
    {
        Xid = xid ?? throw new ArgumentNullException(nameof(xid));
    }

    /// <summary>
    /// Marquer comme échoué
    /// </summary>
    public void MarkAsFailed(string? providerResponseCode = null, string? providerMessage = null)
    {
        Status = ThreeDSecureStatus.Failed;
        ProviderResponseCode = providerResponseCode;
        ProviderMessage = providerMessage;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer les données ACS
    /// </summary>
    public void SetAcsData(string acsUrl, string paReq, string md)
    {
        AcsUrl = acsUrl ?? throw new ArgumentNullException(nameof(acsUrl));
        PaReq = paReq ?? throw new ArgumentNullException(nameof(paReq));
        Md = md ?? throw new ArgumentNullException(nameof(md));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer la valeur d'authentification
    /// </summary>
    public void SetAuthenticationValue(string authenticationValue)
    {
        AuthenticationValue = authenticationValue ?? throw new ArgumentNullException(nameof(authenticationValue));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configurer un message d'erreur
    /// </summary>
    public void SetErrorMessage(string errorMessage)
    {
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associer un client
    /// </summary>
    public void SetCustomerId(Guid customerId)
    {
        CustomerId = customerId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marquer comme abandonné
    /// </summary>
    public void MarkAsAbandoned()
    {
        Status = ThreeDSecureStatus.Abandoned;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marquer comme non requis
    /// </summary>
    public void MarkAsNotRequired()
    {
        Status = ThreeDSecureStatus.NotRequired;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Vérifier si l'authentification a expiré
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Vérifier si l'authentification est en cours
    /// </summary>
    public bool IsPending() => Status == ThreeDSecureStatus.Pending && !IsExpired();

    /// <summary>
    /// Vérifier si l'authentification est complétée
    /// </summary>
    public bool IsCompleted() => Status != ThreeDSecureStatus.Pending;
}