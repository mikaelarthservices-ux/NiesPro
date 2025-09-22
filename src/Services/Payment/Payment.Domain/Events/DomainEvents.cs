using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using Payment.Domain.Entities;
using NiesPro.Contracts.Primitives;

namespace Payment.Domain.Events;

/// <summary>
/// Classe de base abstraite pour les événements de domaine Payment
/// </summary>
public abstract class PaymentDomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    /// <inheritdoc />
    public virtual int Version => 1;
}

/// <summary>
/// Événement déclenché lors de la création d'une transaction
/// </summary>
public class TransactionCreatedEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public Money Amount { get; }
    public TransactionType Type { get; }
    public Guid CustomerId { get; }

    public TransactionCreatedEvent(
        Guid transactionId,
        string transactionNumber,
        Money amount,
        TransactionType type,
        Guid customerId)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Amount = amount;
        Type = type;
        CustomerId = customerId;
    }
}

/// <summary>
/// Événement déclenché lors de l'autorisation d'une transaction
/// </summary>
public class TransactionAuthorizedEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public Money Amount { get; }
    public string AuthorizationCode { get; }

    public TransactionAuthorizedEvent(
        Guid transactionId,
        string transactionNumber,
        Money amount,
        string authorizationCode)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Amount = amount;
        AuthorizationCode = authorizationCode;
    }
}

/// <summary>
/// Événement déclenché lors de la capture d'une transaction
/// </summary>
public class TransactionCapturedEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public Money Amount { get; }
    public Money? Fees { get; }

    public TransactionCapturedEvent(
        Guid transactionId,
        string transactionNumber,
        Money amount,
        Money? fees = null)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Amount = amount;
        Fees = fees;
    }
}

/// <summary>
/// Événement déclenché lors du refus d'une transaction
/// </summary>
public class TransactionDeclinedEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public PaymentDeclineReason Reason { get; }
    public string? ErrorMessage { get; }

    public TransactionDeclinedEvent(
        Guid transactionId,
        string transactionNumber,
        PaymentDeclineReason reason,
        string? errorMessage = null)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Reason = reason;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Événement déclenché lors de l'annulation d'une transaction
/// </summary>
public class TransactionCancelledEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public string? Reason { get; }

    public TransactionCancelledEvent(
        Guid transactionId,
        string transactionNumber,
        string? reason = null)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Reason = reason;
    }
}

/// <summary>
/// Événement déclenché lors d'un remboursement
/// </summary>
public class TransactionRefundedEvent : PaymentDomainEvent
{
    public Guid OriginalTransactionId { get; }
    public Guid RefundTransactionId { get; }
    public Money RefundAmount { get; }
    public string? Reason { get; }

    public TransactionRefundedEvent(
        Guid originalTransactionId,
        Guid refundTransactionId,
        Money refundAmount,
        string? reason = null)
    {
        OriginalTransactionId = originalTransactionId;
        RefundTransactionId = refundTransactionId;
        RefundAmount = refundAmount;
        Reason = reason;
    }
}

/// <summary>
/// Événement déclenché lors du règlement d'une transaction
/// </summary>
public class TransactionSettledEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public Money Amount { get; }

    public TransactionSettledEvent(
        Guid transactionId,
        string transactionNumber,
        Money amount)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        Amount = amount;
    }
}

/// <summary>
/// Événement déclenché lors de la détection d'un risque de fraude élevé
/// </summary>
public class HighFraudRiskDetectedEvent : PaymentDomainEvent
{
    public Guid TransactionId { get; }
    public string TransactionNumber { get; }
    public int FraudScore { get; }

    public HighFraudRiskDetectedEvent(
        Guid transactionId,
        string transactionNumber,
        int fraudScore)
    {
        TransactionId = transactionId;
        TransactionNumber = transactionNumber;
        FraudScore = fraudScore;
    }
}

/// <summary>
/// Événement déclenché lors de la création d'un paiement
/// </summary>
public class PaymentCreatedEvent : PaymentDomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Money Amount { get; }
    public Guid OrderId { get; }
    public Guid CustomerId { get; }

    public PaymentCreatedEvent(
        Guid paymentId,
        string paymentNumber,
        Money amount,
        Guid orderId,
        Guid customerId)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        OrderId = orderId;
        CustomerId = customerId;
    }
}

/// <summary>
/// Événement déclenché lors de la finalisation d'un paiement
/// </summary>
public class PaymentCompletedEvent : PaymentDomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Money Amount { get; }
    public Money PaidAmount { get; }
    public Guid OrderId { get; }

    public PaymentCompletedEvent(
        Guid paymentId,
        string paymentNumber,
        Money amount,
        Money paidAmount,
        Guid orderId)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        PaidAmount = paidAmount;
        OrderId = orderId;
    }
}

/// <summary>
/// Événement déclenché lors de l'échec d'un paiement
/// </summary>
public class PaymentFailedEvent : PaymentDomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Money Amount { get; }
    public string? Reason { get; }

    public PaymentFailedEvent(
        Guid paymentId,
        string paymentNumber,
        Money amount,
        string? reason = null)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Reason = reason;
    }
}

/// <summary>
/// Événement déclenché lors de l'annulation d'un paiement
/// </summary>
public class PaymentCancelledEvent : PaymentDomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Money Amount { get; }
    public string? Reason { get; }

    public PaymentCancelledEvent(
        Guid paymentId,
        string paymentNumber,
        Money amount,
        string? reason = null)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Reason = reason;
    }
}

/// <summary>
/// Événement déclenché lors de l'expiration d'un paiement
/// </summary>
public class PaymentExpiredEvent : PaymentDomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public Money Amount { get; }
    public DateTime ExpiredAt { get; }

    public PaymentExpiredEvent(
        Guid paymentId,
        string paymentNumber,
        Money amount,
        DateTime expiredAt)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        ExpiredAt = expiredAt;
    }
}

/// <summary>
/// Événement déclenché lors de la création d'un moyen de paiement
/// </summary>
public class PaymentMethodCreatedEvent : PaymentDomainEvent
{
    public Guid PaymentMethodId { get; }
    public PaymentMethodType Type { get; }
    public Guid CustomerId { get; }
    public string DisplayName { get; }

    public PaymentMethodCreatedEvent(
        Guid paymentMethodId,
        PaymentMethodType type,
        Guid customerId,
        string displayName)
    {
        PaymentMethodId = paymentMethodId;
        Type = type;
        CustomerId = customerId;
        DisplayName = displayName;
    }
}

/// <summary>
/// Événement déclenché lors de la désactivation d'un moyen de paiement
/// </summary>
public class PaymentMethodDeactivatedEvent : PaymentDomainEvent
{
    public Guid PaymentMethodId { get; }
    public PaymentMethodType Type { get; }
    public Guid CustomerId { get; }
    public string? Reason { get; }

    public PaymentMethodDeactivatedEvent(
        Guid paymentMethodId,
        PaymentMethodType type,
        Guid customerId,
        string? reason = null)
    {
        PaymentMethodId = paymentMethodId;
        Type = type;
        CustomerId = customerId;
        Reason = reason;
    }
}

/// <summary>
/// Événement déclenché lors de la mise à jour d'un moyen de paiement par défaut
/// </summary>
public class DefaultPaymentMethodChangedEvent : PaymentDomainEvent
{
    public Guid CustomerId { get; }
    public Guid? PreviousPaymentMethodId { get; }
    public Guid NewPaymentMethodId { get; }

    public DefaultPaymentMethodChangedEvent(
        Guid customerId,
        Guid newPaymentMethodId,
        Guid? previousPaymentMethodId = null)
    {
        CustomerId = customerId;
        NewPaymentMethodId = newPaymentMethodId;
        PreviousPaymentMethodId = previousPaymentMethodId;
    }
}