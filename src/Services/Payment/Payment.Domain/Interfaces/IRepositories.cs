using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Domain.Interfaces;

/// <summary>
/// Interface de base pour les repositories
/// </summary>
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Payment
/// </summary>
public interface IPaymentRepository : IBaseRepository<Payment.Domain.Entities.Payment>
{
    Task<Payment.Domain.Entities.Payment?> GetByPaymentNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<Payment.Domain.Entities.Payment?> GetByNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<Payment.Domain.Entities.Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<(List<Payment.Domain.Entities.Payment> payments, int totalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByMerchantIdAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetRecentByCustomerIdAsync(Guid customerId, TimeSpan timeSpan, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAmountByMerchantAsync(Guid merchantId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<(List<Payment.Domain.Entities.Payment> payments, int totalCount)> SearchAsync(
        string? searchTerm, Guid? merchantId = null, Guid? customerId = null, List<PaymentStatus>? statuses = null,
        DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<List<Payment.Domain.Entities.Payment>> GetByMerchantIdAndDateRangeAsync(
        Guid merchantId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Transaction
/// </summary>
public interface ITransactionRepository : IBaseRepository<Transaction>
{
    Task<Transaction?> GetByTransactionNumberAsync(string transactionNumber, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByMerchantIdAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByStatusAsync(TransactionStatus status, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByPaymentMethodIdAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetRecentByCustomerAsync(Guid customerId, DateTime since, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository PaymentMethod
/// </summary>
public interface IPaymentMethodRepository : IBaseRepository<PaymentMethod>
{
    Task<List<PaymentMethod>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<PaymentMethod>> GetActiveMethodsAsync(CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Card
/// </summary>
public interface ICardRepository : IBaseRepository<Card>
{
    Task<Card?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<Card>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Card?> GetByMaskedNumberAsync(string maskedNumber, Guid customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository ThreeDSecureAuthentication
/// </summary>
public interface IThreeDSecureRepository : IBaseRepository<ThreeDSecureAuthentication>
{
    Task<ThreeDSecureAuthentication?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<List<ThreeDSecureAuthentication>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Merchant
/// </summary>
public interface IMerchantRepository : IBaseRepository<Merchant>
{
    Task<Merchant?> GetByApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<Merchant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Merchant>> GetByStatusAsync(MerchantStatus status, CancellationToken cancellationToken = default);
    Task<bool> ApiKeyExistsAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Audit Log
/// </summary>
public interface IAuditLogRepository
{
    Task LogActionAsync(string action, string entityType, Guid entityId, string details, Guid userId, CancellationToken cancellationToken = default);
    Task<List<object>> GetLogsByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Security Log
/// </summary>
public interface ISecurityLogRepository
{
    Task LogSecurityEventAsync(string eventType, string description, Guid? userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<List<object>> GetSecurityLogsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Event Store
/// </summary>
public interface IEventStoreRepository
{
    Task SaveEventAsync(string eventType, object eventData, Guid aggregateId, CancellationToken cancellationToken = default);
    Task<List<object>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface pour le repository Fraud Alert
/// </summary>
public interface IFraudAlertRepository
{
    Task CreateAlertAsync(string alertType, string description, Guid? paymentId, Guid? merchantId, CancellationToken cancellationToken = default);
    Task<List<object>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);
}