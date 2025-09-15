namespace Payment.Domain.Enums;

/// <summary>
/// Code de raison pour les refus de paiement
/// </summary>
public enum PaymentDeclineReason
{
    /// <summary>
    /// Aucune raison spécifique - paiement réussi
    /// </summary>
    None = 0,

    /// <summary>
    /// Fonds insuffisants
    /// </summary>
    InsufficientFunds = 1,

    /// <summary>
    /// Carte expirée
    /// </summary>
    CardExpired = 2,

    /// <summary>
    /// Carte bloquée ou suspendue
    /// </summary>
    CardBlocked = 3,

    /// <summary>
    /// Code PIN incorrect
    /// </summary>
    IncorrectPin = 4,

    /// <summary>
    /// CVV invalide
    /// </summary>
    InvalidCvv = 5,

    /// <summary>
    /// Numéro de carte invalide
    /// </summary>
    InvalidCardNumber = 6,

    /// <summary>
    /// Transaction suspecte - détection de fraude
    /// </summary>
    FraudSuspected = 7,

    /// <summary>
    /// Limite de transaction dépassée
    /// </summary>
    TransactionLimitExceeded = 8,

    /// <summary>
    /// Limite quotidienne dépassée
    /// </summary>
    DailyLimitExceeded = 9,

    /// <summary>
    /// Pays non autorisé
    /// </summary>
    CountryNotAllowed = 10,

    /// <summary>
    /// Monnaie non supportée
    /// </summary>
    CurrencyNotSupported = 11,

    /// <summary>
    /// Commerçant non autorisé
    /// </summary>
    MerchantNotAllowed = 12,

    /// <summary>
    /// Type de transaction non autorisé
    /// </summary>
    TransactionTypeNotAllowed = 13,

    /// <summary>
    /// Erreur de communication avec la banque
    /// </summary>
    BankCommunicationError = 14,

    /// <summary>
    /// Processeur de paiement indisponible
    /// </summary>
    ProcessorUnavailable = 15,

    /// <summary>
    /// Délai d'attente dépassé
    /// </summary>
    Timeout = 16,

    /// <summary>
    /// Données de transaction invalides
    /// </summary>
    InvalidTransactionData = 17,

    /// <summary>
    /// 3D Secure échoué
    /// </summary>
    ThreeDSecureFailed = 18,

    /// <summary>
    /// Authentification échouée
    /// </summary>
    AuthenticationFailed = 19,

    /// <summary>
    /// Erreur système générique
    /// </summary>
    SystemError = 20,

    /// <summary>
    /// Transaction dupliquée
    /// </summary>
    DuplicateTransaction = 21,

    /// <summary>
    /// Montant invalide
    /// </summary>
    InvalidAmount = 22,

    /// <summary>
    /// Carte non encore activée
    /// </summary>
    CardNotActivated = 23,

    /// <summary>
    /// Terminal non autorisé
    /// </summary>
    TerminalNotAuthorized = 24,

    /// <summary>
    /// Transaction annulée par le client
    /// </summary>
    CancelledByCustomer = 25
}

/// <summary>
/// Extensions pour PaymentDeclineReason
/// </summary>
public static class PaymentDeclineReasonExtensions
{
    /// <summary>
    /// Vérifier si la raison indique une possibilité de réessayer
    /// </summary>
    public static bool CanRetry(this PaymentDeclineReason reason)
    {
        return reason switch
        {
            PaymentDeclineReason.BankCommunicationError or
            PaymentDeclineReason.ProcessorUnavailable or
            PaymentDeclineReason.Timeout or
            PaymentDeclineReason.SystemError => true,
            _ => false
        };
    }

    /// <summary>
    /// Vérifier si la raison nécessite une intervention du client
    /// </summary>
    public static bool RequiresCustomerAction(this PaymentDeclineReason reason)
    {
        return reason switch
        {
            PaymentDeclineReason.InsufficientFunds or
            PaymentDeclineReason.CardExpired or
            PaymentDeclineReason.CardBlocked or
            PaymentDeclineReason.IncorrectPin or
            PaymentDeclineReason.InvalidCvv or
            PaymentDeclineReason.InvalidCardNumber or
            PaymentDeclineReason.CardNotActivated => true,
            _ => false
        };
    }

    /// <summary>
    /// Obtenir le niveau de gravité de la raison
    /// </summary>
    public static SeverityLevel GetSeverityLevel(this PaymentDeclineReason reason)
    {
        return reason switch
        {
            PaymentDeclineReason.FraudSuspected => SeverityLevel.Critical,
            PaymentDeclineReason.CardBlocked or
            PaymentDeclineReason.CountryNotAllowed or
            PaymentDeclineReason.MerchantNotAllowed => SeverityLevel.High,
            PaymentDeclineReason.InsufficientFunds or
            PaymentDeclineReason.TransactionLimitExceeded or
            PaymentDeclineReason.DailyLimitExceeded => SeverityLevel.Medium,
            PaymentDeclineReason.BankCommunicationError or
            PaymentDeclineReason.ProcessorUnavailable or
            PaymentDeclineReason.Timeout => SeverityLevel.Low,
            _ => SeverityLevel.Medium
        };
    }

    /// <summary>
    /// Obtenir le message d'erreur localisé pour l'utilisateur
    /// </summary>
    public static string GetUserMessage(this PaymentDeclineReason reason)
    {
        return reason switch
        {
            PaymentDeclineReason.None => "Paiement réussi",
            PaymentDeclineReason.InsufficientFunds => "Fonds insuffisants. Veuillez vérifier votre solde.",
            PaymentDeclineReason.CardExpired => "Votre carte a expiré. Veuillez utiliser une autre carte.",
            PaymentDeclineReason.CardBlocked => "Votre carte est bloquée. Contactez votre banque.",
            PaymentDeclineReason.IncorrectPin => "Code PIN incorrect. Veuillez réessayer.",
            PaymentDeclineReason.InvalidCvv => "Code de sécurité invalide. Veuillez vérifier le CVV.",
            PaymentDeclineReason.InvalidCardNumber => "Numéro de carte invalide. Veuillez vérifier votre saisie.",
            PaymentDeclineReason.FraudSuspected => "Transaction suspecte détectée. Contactez votre banque.",
            PaymentDeclineReason.TransactionLimitExceeded => "Limite de transaction dépassée.",
            PaymentDeclineReason.DailyLimitExceeded => "Limite quotidienne dépassée.",
            PaymentDeclineReason.CountryNotAllowed => "Paiement non autorisé depuis votre pays.",
            PaymentDeclineReason.CurrencyNotSupported => "Devise non supportée.",
            PaymentDeclineReason.BankCommunicationError => "Erreur de communication. Veuillez réessayer.",
            PaymentDeclineReason.ProcessorUnavailable => "Service temporairement indisponible.",
            PaymentDeclineReason.Timeout => "Délai d'attente dépassé. Veuillez réessayer.",
            PaymentDeclineReason.ThreeDSecureFailed => "Authentification 3D Secure échouée.",
            PaymentDeclineReason.CancelledByCustomer => "Transaction annulée.",
            _ => "Paiement refusé. Veuillez réessayer ou utiliser un autre moyen de paiement."
        };
    }

    /// <summary>
    /// Obtenir le code d'erreur technique pour les logs
    /// </summary>
    public static string GetErrorCode(this PaymentDeclineReason reason)
    {
        return $"PAY_{(int)reason:D3}";
    }
}

/// <summary>
/// Niveau de gravité pour les raisons de refus
/// </summary>
public enum SeverityLevel
{
    Low,
    Medium,
    High,
    Critical
}