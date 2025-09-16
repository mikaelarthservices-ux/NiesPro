using Payment.Domain.Enums;

namespace Payment.Application.Utilities;

/// <summary>
/// Extensions pour mapper les enums entre Domain et Application layers
/// </summary>
public static class EnumMappingExtensions
{
    /// <summary>
    /// Convertit TransactionStatus vers PaymentStatus
    /// </summary>
    public static PaymentStatus ToPaymentStatus(this TransactionStatus transactionStatus)
    {
        return transactionStatus switch
        {
            TransactionStatus.Pending => PaymentStatus.Pending,
            TransactionStatus.Successful => PaymentStatus.Captured,  // Success = Captured
            TransactionStatus.Failed => PaymentStatus.Failed,
            TransactionStatus.Cancelled => PaymentStatus.Cancelled,
            TransactionStatus.Refunded => PaymentStatus.Refunded,
            _ => PaymentStatus.Failed  // Default pour sécurité
        };
    }

    /// <summary>
    /// Convertit PaymentStatus vers TransactionStatus
    /// </summary>
    public static TransactionStatus ToTransactionStatus(this PaymentStatus paymentStatus)
    {
        return paymentStatus switch
        {
            PaymentStatus.Pending => TransactionStatus.Pending,
            PaymentStatus.Authorized => TransactionStatus.Successful,  // Authorized = Success
            PaymentStatus.Captured => TransactionStatus.Successful,
            PaymentStatus.Failed => TransactionStatus.Failed,
            PaymentStatus.Cancelled => TransactionStatus.Cancelled,
            PaymentStatus.Refunded => TransactionStatus.Refunded,
            PaymentStatus.Settled => TransactionStatus.Successful,  // Settled = Success
            _ => TransactionStatus.Failed  // Default pour sécurité
        };
    }

    /// <summary>
    /// Vérifie si TransactionStatus correspond aux critères de succès (Captured/Settled)
    /// </summary>
    public static bool IsSuccessful(this TransactionStatus status)
    {
        return status == TransactionStatus.Successful;
    }

    /// <summary>
    /// Vérifie si TransactionStatus est dans un état équivalent à PaymentStatus donné
    /// </summary>
    public static bool IsEquivalentTo(this TransactionStatus transactionStatus, PaymentStatus paymentStatus)
    {
        return transactionStatus.ToPaymentStatus() == paymentStatus;
    }
}