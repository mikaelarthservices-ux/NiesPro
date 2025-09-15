using MediatR;
using AutoMapper;
using Payment.Application.Commands;
using Payment.Application.DTOs;
using Payment.Domain.Entities;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Handlers;

/// <summary>
/// Handler pour la création de paiement
/// </summary>
public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderService _orderService;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreatePaymentHandler> _logger;

    public CreatePaymentHandler(
        IPaymentRepository paymentRepository,
        IOrderService orderService,
        IFraudDetectionService fraudDetectionService,
        IPaymentProcessorFactory paymentProcessorFactory,
        IMapper mapper,
        IMediator mediator,
        ILogger<CreatePaymentHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _orderService = orderService;
        _fraudDetectionService = fraudDetectionService;
        _paymentProcessorFactory = paymentProcessorFactory;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating payment for order {OrderId}, amount {Amount} {Currency}", 
                request.OrderId, request.Amount, request.Currency);

            // 1. Valider que la commande existe et est valide
            var order = await _orderService.GetOrderByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                return new CreatePaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Commande introuvable"
                };
            }

            if (order.Status != OrderStatus.Confirmed)
            {
                _logger.LogWarning("Order {OrderId} is not in confirmed status: {Status}", request.OrderId, order.Status);
                return new CreatePaymentResult
                {
                    IsSuccess = false,
                    ErrorMessage = "La commande doit être confirmée pour procéder au paiement"
                };
            }

            // 2. Vérifier s'il n'y a pas déjà un paiement en cours pour cette commande
            var existingPayments = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            var activePending = existingPayments.FirstOrDefault(p => 
                p.Status == PaymentStatus.Pending && 
                !p.IsExpired());

            if (activePending != null)
            {
                _logger.LogInformation("Found existing pending payment {PaymentId} for order {OrderId}", 
                    activePending.Id, request.OrderId);
                
                return new CreatePaymentResult
                {
                    PaymentId = activePending.Id,
                    PaymentNumber = activePending.Reference,
                    Status = activePending.Status,
                    ExpiresAt = activePending.ExpiresAt ?? DateTime.UtcNow,
                    Amount = activePending.Amount.Amount,
                    Currency = activePending.Amount.Currency.Code,
                    IsSuccess = true
                };
            }

            // 3. Créer le montant Money
            var amount = new Money(request.Amount, request.Currency);
            Money? minimumPartialAmount = null;

            if (request.AllowPartialPayments && request.MinimumPartialAmount.HasValue)
            {
                minimumPartialAmount = new Money(
                    request.MinimumPartialAmount.Value, 
                    request.MinimumPartialCurrency ?? request.Currency);
            }

            // 4. Créer le paiement
            var payment = new Domain.Entities.Payment(
                amount,
                request.OrderId,
                request.CustomerId,
                request.MerchantId,
                request.Description,
                request.TimeoutMinutes,
                request.MaxAttempts,
                request.AllowPartialPayments,
                minimumPartialAmount,
                request.SuccessUrl,
                request.FailureUrl,
                request.WebhookUrl);

            // 5. Ajouter les métadonnées si présentes
            if (request.Metadata != null)
            {
                foreach (var metadata in request.Metadata)
                {
                    payment.SetSessionData(metadata.Key, metadata.Value);
                }
            }

            // 6. Sauvegarder le paiement
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // 7. Publier l'événement de création
            await _mediator.Publish(new PaymentCreatedEvent(
                payment.Id,
                payment.PaymentNumber,
                payment.Amount,
                payment.OrderId,
                payment.CustomerId), cancellationToken);

            _logger.LogInformation("Payment {PaymentId} created successfully for order {OrderId}", 
                payment.Id, request.OrderId);

            // 8. Générer l'URL de paiement si nécessaire
            string? paymentUrl = null;
            var processor = _paymentProcessorFactory.GetDefaultProcessor();
            if (processor.SupportsHostedPayment())
            {
                paymentUrl = await processor.GeneratePaymentUrlAsync(payment, cancellationToken);
            }

            return new CreatePaymentResult
            {
                PaymentId = payment.Id,
                PaymentNumber = payment.Reference,
                Status = payment.Status,
                ExpiresAt = payment.ExpiresAt ?? DateTime.UtcNow,
                PaymentUrl = paymentUrl,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency.Code,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
            return new CreatePaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Erreur lors de la création du paiement"
            };
        }
    }
}

/// <summary>
/// Handler pour le traitement de paiement
/// </summary>
public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IThreeDSecureService _threeDSecureService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository,
        ITransactionRepository transactionRepository,
        IFraudDetectionService fraudDetectionService,
        IPaymentProcessorFactory paymentProcessorFactory,
        IThreeDSecureService threeDSecureService,
        IMapper mapper,
        IMediator mediator,
        ILogger<ProcessPaymentHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _transactionRepository = transactionRepository;
        _fraudDetectionService = fraudDetectionService;
        _paymentProcessorFactory = paymentProcessorFactory;
        _threeDSecureService = threeDSecureService;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ProcessPaymentResult> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing payment {PaymentId} with payment method {PaymentMethodId}", 
                request.PaymentId, request.PaymentMethodId);

            // 1. Récupérer le paiement
            var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
            if (payment == null)
            {
                return CreateErrorResult("Paiement introuvable");
            }

            // 2. Vérifier l'état du paiement
            if (!payment.CanRetry())
            {
                return CreateErrorResult("Le paiement ne peut plus être tenté");
            }

            // 3. Récupérer le moyen de paiement
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(request.PaymentMethodId, cancellationToken);
            if (paymentMethod == null || !paymentMethod.CanBeUsed())
            {
                return CreateErrorResult("Moyen de paiement invalide ou inactif");
            }

            // 4. Déterminer le montant à traiter
            var amountToProcess = request.Amount.HasValue && !string.IsNullOrEmpty(request.Currency) 
                ? new Money(request.Amount.Value, request.Currency)
                : payment.Amount;

            // 5. Valider le montant pour paiements partiels
            if (payment.AllowPartialPayments && !payment.IsPartialPaymentValid(amountToProcess))
            {
                return CreateErrorResult("Montant de paiement partiel invalide");
            }

            // 6. Créer la transaction
            var transaction = payment.AddTransaction(
                amountToProcess,
                TransactionType.Payment,
                paymentMethod,
                request.ClientIpAddress,
                request.ClientUserAgent);

            // 7. Analyse de fraude
            var fraudScore = await _fraudDetectionService.AnalyzeTransactionAsync(
                transaction, request.ClientIpAddress, request.GeoLocation, cancellationToken);

            transaction.SetFraudScore(fraudScore);

            if (fraudScore >= 80 && !request.ForceProcess)
            {
                transaction.Decline(PaymentDeclineReason.FraudSuspected, "Score de fraude élevé détecté");
                await _transactionRepository.SaveChangesAsync(cancellationToken);

                return new ProcessPaymentResult
                {
                    TransactionId = transaction.Id,
                    TransactionNumber = transaction.TransactionNumber,
                    Status = transaction.Status,
                    Amount = transaction.Amount.Amount,
                    Currency = transaction.Amount.Currency.Code,
                    DeclineReason = PaymentDeclineReason.FraudSuspected,
                    ErrorMessage = "Transaction bloquée pour suspicion de fraude",
                    FraudScore = fraudScore,
                    IsSuccess = false
                };
            }

            // 8. Vérification 3D Secure si nécessaire
            if (paymentMethod.Type.RequiresOnlineValidation() && 
                request.ThreeDSecure?.Enabled == true)
            {
                var threeDSecureResult = await _threeDSecureService.ValidateAsync(
                    paymentMethod, request.ThreeDSecure, cancellationToken);

                if (!threeDSecureResult.IsValid)
                {
                    transaction.Decline(PaymentDeclineReason.ThreeDSecureFailed, 
                        "Échec de l'authentification 3D Secure");
                    await _transactionRepository.SaveChangesAsync(cancellationToken);

                    return new ProcessPaymentResult
                    {
                        TransactionId = transaction.Id,
                        TransactionNumber = transaction.TransactionNumber,
                        Status = transaction.Status,
                        Amount = transaction.Amount.Amount,
                        Currency = transaction.Amount.Currency.Code,
                        DeclineReason = PaymentDeclineReason.ThreeDSecureFailed,
                        ErrorMessage = "Authentification 3D Secure échouée",
                        IsSuccess = false
                    };
                }
            }

            // 9. Traitement par le processeur de paiement
            var processor = _paymentProcessorFactory.GetProcessor(paymentMethod.Type);
            var processingResult = await processor.ProcessPaymentAsync(
                transaction, paymentMethod, request.VerificationCode, cancellationToken);

            // 10. Mettre à jour la transaction selon le résultat
            if (processingResult.IsSuccess)
            {
                if (processingResult.RequiresCapture)
                {
                    transaction.Authorize(processingResult.AuthorizationCode!, processingResult.AuthorizationExpiresAt);
                }
                else
                {
                    transaction.Authorize(processingResult.AuthorizationCode!, processingResult.AuthorizationExpiresAt);
                    transaction.Capture(amountToProcess, processingResult.Fees);
                    payment.MarkAsSuccessful();
                }

                transaction.SetExternalReference(processingResult.ExternalReference!);
                paymentMethod.UpdateLastUsed();
            }
            else
            {
                transaction.Decline(processingResult.DeclineReason ?? PaymentDeclineReason.SystemError, 
                    processingResult.ErrorMessage);
                
                // Marquer le paiement comme échoué si plus de tentatives
                if (!payment.CanRetry())
                {
                    payment.MarkAsFailed();
                }
            }

            // 11. Sauvegarder les changements
            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment processing completed for transaction {TransactionId}, success: {IsSuccess}", 
                transaction.Id, processingResult.IsSuccess);

            // 12. Publier les événements
            foreach (var domainEvent in transaction.DomainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            transaction.ClearDomainEvents();

            return new ProcessPaymentResult
            {
                TransactionId = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                Status = transaction.Status,
                AuthorizationCode = transaction.AuthorizationCode,
                ExternalReference = transaction.ExternalReference,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency.Code,
                Fees = transaction.Fees?.Amount,
                DeclineReason = transaction.DeclineReason,
                ErrorMessage = transaction.ErrorMessage,
                FraudScore = transaction.FraudScore,
                IsSuccess = processingResult.IsSuccess,
                Requires3DSecure = processingResult.Requires3DSecure,
                ThreeDSecureUrl = processingResult.ThreeDSecureUrl,
                ThreeDSecureData = processingResult.ThreeDSecureData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", request.PaymentId);
            return CreateErrorResult("Erreur lors du traitement du paiement");
        }
    }

    private static ProcessPaymentResult CreateErrorResult(string errorMessage)
    {
        return new ProcessPaymentResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

// Interfaces des services (à implémenter dans Infrastructure)
public interface IPaymentRepository
{
    Task<Domain.Entities.Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Payment?> GetByNumberAsync(string paymentNumber, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Payment>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<(List<Domain.Entities.Payment> payments, int totalCount)> GetByCustomerIdPagedAsync(
        Guid customerId, int page, int pageSize, PaymentStatus? status = null,
        DateTime? fromDate = null, DateTime? toDate = null, decimal? minAmount = null, decimal? maxAmount = null,
        string? currency = null, CancellationToken cancellationToken = default);
    Task<(List<Domain.Entities.Payment> payments, int totalCount)> SearchAsync(
        string? searchTerm, Guid? merchantId = null, Guid? customerId = null, List<PaymentStatus>? statuses = null,
        List<PaymentMethodType>? paymentMethodTypes = null, DateTime? fromDate = null, DateTime? toDate = null,
        decimal? minAmount = null, decimal? maxAmount = null, string? currency = null,
        int page = 1, int pageSize = 10, string? sortBy = null, string? sortOrder = null,
        CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Payment>> GetByMerchantIdAndDateRangeAsync(
        Guid merchantId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Payment> AddAsync(Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PaymentMethod>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByPaymentMethodIdAsync(Guid paymentMethodId, CancellationToken cancellationToken = default);
    Task<List<Transaction>> GetByCustomerIdAsync(Guid customerId, DateTime? fromDate = null, DateTime? toDate = null, TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public interface IFraudDetectionService
{
    Task<int> AnalyzeTransactionAsync(Transaction transaction, string? ipAddress, string? geoLocation, CancellationToken cancellationToken = default);
}

public interface IPaymentProcessorFactory
{
    IPaymentProcessor GetProcessor(PaymentMethodType type);
    IPaymentProcessor GetDefaultProcessor();
}

public interface IPaymentProcessor
{
    Task<PaymentProcessingResult> ProcessPaymentAsync(Transaction transaction, PaymentMethod paymentMethod, string? verificationCode, CancellationToken cancellationToken = default);
    Task<string?> GeneratePaymentUrlAsync(Domain.Entities.Payment payment, CancellationToken cancellationToken = default);
    bool SupportsHostedPayment();
}

public interface IThreeDSecureService
{
    Task<ThreeDSecureValidationResult> ValidateAsync(PaymentMethod paymentMethod, ThreeDSecureData data, CancellationToken cancellationToken = default);
}

// DTOs pour les services
public class OrderDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class PaymentProcessingResult
{
    public bool IsSuccess { get; set; }
    public string? AuthorizationCode { get; set; }
    public DateTime? AuthorizationExpiresAt { get; set; }
    public string? ExternalReference { get; set; }
    public Money? Fees { get; set; }
    public PaymentDeclineReason? DeclineReason { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresCapture { get; set; }
    public bool Requires3DSecure { get; set; }
    public string? ThreeDSecureUrl { get; set; }
    public Dictionary<string, string>? ThreeDSecureData { get; set; }
}

public class ThreeDSecureValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}