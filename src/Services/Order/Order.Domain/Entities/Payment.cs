using NiesPro.Contracts.Primitives;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public sealed class Payment : Entity
{
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public Money RefundedAmount { get; private set; } = Money.Zero();
    public string? TransactionId { get; private set; }
    public string? ProviderReference { get; private set; }
    public string? FailureReason { get; private set; }
    
    // Propriétés de suivi - utilise Entity.CreatedAt de base
    public new DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    // Navigation property
    public Guid OrderId { get; private set; }

    private Payment() { } // EF Constructor

    private Payment(
        Guid id,
        PaymentMethod method,
        Money amount,
        Guid orderId) : base(id)
    {
        Method = method;
        Status = PaymentStatus.Pending;
        Amount = amount;
        RefundedAmount = Money.Zero(amount.Currency);
        OrderId = orderId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Create(PaymentMethod method, Money amount, Guid orderId)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));

        return new Payment(Guid.NewGuid(), method, amount, orderId);
    }

    public void MarkAsProcessing(string? transactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot mark payment as processing. Current status: {Status}");

        Status = PaymentStatus.Processing;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string? providerReference = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete payment. Current status: {Status}");

        Status = PaymentStatus.Completed;
        ProviderReference = providerReference;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string failureReason)
    {
        if (Status is PaymentStatus.Completed or PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot mark completed/refunded payment as failed. Current status: {Status}");

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required", nameof(failureReason));

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        FailedAt = DateTime.UtcNow;
    }

    public void ProcessRefund(Money refundAmount, string reason)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Cannot refund non-completed payment. Current status: {Status}");

        if (refundAmount.Amount <= 0)
            throw new ArgumentException("Refund amount must be greater than zero", nameof(refundAmount));

        if (refundAmount.Currency != Amount.Currency)
            throw new InvalidOperationException($"Currency mismatch: {Amount.Currency} vs {refundAmount.Currency}");

        var totalRefunded = RefundedAmount.Add(refundAmount);
        if (totalRefunded.Amount > Amount.Amount)
            throw new InvalidOperationException("Total refund amount cannot exceed payment amount");

        RefundedAmount = totalRefunded;
        
        Status = totalRefunded.Amount == Amount.Amount 
            ? PaymentStatus.Refunded 
            : PaymentStatus.PartiallyRefunded;
    }

    public Money GetRemainingAmount() => Amount.Subtract(RefundedAmount);

    public bool CanBeRefunded() => Status == PaymentStatus.Completed && RefundedAmount.Amount < Amount.Amount;

    public bool IsFullyRefunded() => Status == PaymentStatus.Refunded;

    public override string ToString() => $"Payment {Method} - {Amount} ({Status})";
}