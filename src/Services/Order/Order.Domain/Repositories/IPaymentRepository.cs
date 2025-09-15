using NiesPro.Contracts.Common;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(DateTime olderThan, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaidAmountAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundedAmountAsync(Guid orderId, CancellationToken cancellationToken = default);
}