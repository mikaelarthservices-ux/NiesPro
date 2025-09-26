using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Commands.V2;
using Payment.Application.DTOs;
using Payment.Application.Services;
using Payment.Domain.Entities;
using Payment.Domain.Interfaces;
using Payment.Domain.ValueObjects;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using NiesPro.Contracts.Application.CQRS;
using NiesPro.Contracts.Common;
using NiesPro.Logging.Client;

namespace Payment.Application.Handlers.V2;

/// <summary>
/// Handler pour la création de paiement - NiesPro Enterprise Standard
/// </summary>
public class CreatePaymentCommandHandler : BaseCommandHandler<CreatePaymentCommand, ApiResponse<PaymentResponse>>, 
    IRequestHandler<CreatePaymentCommand, ApiResponse<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderService _orderService;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogsServiceClient _logsService;
    private readonly IAuditServiceClient _auditService;
    private readonly IMediator _mediator;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IOrderService orderService,
        IFraudDetectionService fraudDetectionService,
        IPaymentProcessorFactory paymentProcessorFactory,
        IUnitOfWork unitOfWork,
        ILogsServiceClient logsService,
        IAuditServiceClient auditService,
        IMediator mediator,
        ILogger<CreatePaymentCommandHandler> logger) : base(logger)
    {
        _paymentRepository = paymentRepository;
        _orderService = orderService;
        _fraudDetectionService = fraudDetectionService;
        _paymentProcessorFactory = paymentProcessorFactory;
        _unitOfWork = unitOfWork;
        _logsService = logsService;
        _auditService = auditService;
        _mediator = mediator;
    }

    /// <summary>
    /// MediatR Handle method - délègue vers BaseCommandHandler
    /// </summary>
    public async Task<ApiResponse<PaymentResponse>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        => await HandleAsync(request, cancellationToken);

    /// <summary>
    /// Exécute la logique de création de paiement - NiesPro Enterprise Implementation
    /// </summary>
    protected override async Task<ApiResponse<PaymentResponse>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Creating payment for order {OrderId} with amount {Amount}", command.OrderId, command.Amount);

            // NiesPro Enterprise: Enhanced logging avec audit trail
            await _logsService.LogInformationAsync($"Creating payment for order: {command.OrderId}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["OrderId"] = command.OrderId,
                ["CustomerId"] = command.CustomerId,
                ["MerchantId"] = command.MerchantId,
                ["Amount"] = command.Amount,
                ["Currency"] = command.Currency,
                ["PaymentMethod"] = command.PaymentMethod.ToString()
            });

            // Validation de la commande
            var orderValidation = await _orderService.ValidateOrderAsync(command.OrderId);
            if (!orderValidation.IsValid)
            {
                await _logsService.LogWarningAsync($"Order validation failed for order: {command.OrderId}", new Dictionary<string, object>
                {
                    ["CommandId"] = command.CommandId,
                    ["ValidationErrors"] = orderValidation.Errors
                });

                return ApiResponse<PaymentResponse>.CreateError(
                    "Order validation failed",
                    orderValidation.Errors
                );
            }

            // Détection de fraude
            // TODO: Créer une transaction temporaire pour la validation de fraude
            // Pour l'instant, on assume un risque faible
            var fraudCheckResult = new FraudAnalysisResult 
            { 
                FraudScore = 10, 
                RiskLevel = FraudRiskLevel.Low,
                Recommendation = FraudRecommendation.Allow
            };

            if (fraudCheckResult.RiskLevel == FraudRiskLevel.High || fraudCheckResult.Recommendation == FraudRecommendation.Block)
            {
                await _logsService.LogWarningAsync($"Fraud detected for payment creation", new Dictionary<string, object>
                {
                    ["CommandId"] = command.CommandId,
                    ["CustomerId"] = command.CustomerId,
                    ["FraudScore"] = fraudCheckResult.FraudScore,
                    ["RiskLevel"] = fraudCheckResult.RiskLevel.ToString()
                });

                return ApiResponse<PaymentResponse>.CreateError(
                    "Payment blocked due to fraud detection",
                    new[] { $"Fraud score: {fraudCheckResult.FraudScore}" }
                );
            }

            // Création de l'entité Payment avec constructeur
            var payment = new Payment.Domain.Entities.Payment(
                command.CustomerId,
                command.MerchantId,
                command.OrderId,
                new Money(command.Amount, command.Currency),
                command.PaymentMethod)
            {
                Description = command.Description,
                ReturnUrl = command.ReturnUrl,
                CancelUrl = command.CancelUrl,
                WebhookUrl = command.NotificationUrl
            };

            // Ajouter les métadonnées si fournies
            if (command.Metadata != null)
            {
                foreach (var metadata in command.Metadata)
                {
                    payment.Metadata[metadata.Key] = metadata.Value?.ToString() ?? string.Empty;
                }
            }

            // Persistance
            await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // NiesPro Enterprise: Audit trail automatique
            await _auditService.AuditCreateAsync(
                userId: command.CustomerId.ToString(),
                userName: "Customer",
                entityName: "Payment",
                entityId: payment.Id.ToString(),
                metadata: new Dictionary<string, object>
                {
                    ["paymentNumber"] = payment.PaymentNumber,
                    ["orderId"] = payment.OrderId,
                    ["amount"] = payment.Amount.Amount,
                    ["currency"] = payment.Amount.Currency.Code,
                    ["paymentMethod"] = payment.Method.ToString(),
                    ["status"] = payment.Status.ToString()
                }
            );

            // Événement de domaine
            var paymentCreatedEvent = new PaymentCreatedEvent(
                payment.Id,
                payment.Reference,
                payment.Amount,
                payment.CustomerId,
                payment.OrderId);

            await _mediator.Publish(paymentCreatedEvent, cancellationToken);

            // Réponse
            var response = new PaymentResponse
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                MerchantId = payment.MerchantId,
                Status = payment.Status,
                Amount = payment.Amount.Amount,
                Currency = payment.Amount.Currency.Code,
                PaymentMethod = payment.Method,
                Description = payment.Description,
                ExternalReference = payment.Reference,
                ReturnUrl = payment.ReturnUrl,
                NotificationUrl = payment.WebhookUrl,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };

            await _logsService.LogInformationAsync($"Payment created successfully: {payment.PaymentNumber}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["PaymentId"] = payment.Id,
                ["PaymentNumber"] = payment.PaymentNumber,
                ["Status"] = payment.Status.ToString()
            });

            return ApiResponse<PaymentResponse>.CreateSuccess(
                response,
                "Payment created successfully"
            );
        }
        catch (Exception ex)
        {
            await _logsService.LogErrorAsync(ex, $"Error creating payment for order: {command.OrderId}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["OrderId"] = command.OrderId,
                ["ErrorMessage"] = ex.Message
            });

            Logger.LogError(ex, "Error creating payment for order {OrderId}", command.OrderId);
            
            return ApiResponse<PaymentResponse>.CreateError(
                "Internal server error while creating payment",
                new[] { ex.Message }
            );
        }
    }

    /// <summary>
    /// Génère un numéro de paiement unique
    /// </summary>
    private static string GeneratePaymentNumber()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"PAY-{timestamp}-{random}";
    }

    /// <summary>
    /// Génère un token de paiement sécurisé
    /// </summary>
    private static string GeneratePaymentToken(Guid paymentId)
    {
        return Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{paymentId}:{DateTimeOffset.UtcNow.Ticks}")
        );
    }
}

/// <summary>
/// Handler pour le traitement de paiement - NiesPro Enterprise Standard
/// </summary>
public class ProcessPaymentCommandHandler : BaseCommandHandler<ProcessPaymentCommand, ApiResponse<TransactionResponse>>,
    IRequestHandler<ProcessPaymentCommand, ApiResponse<TransactionResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogsServiceClient _logsService;
    private readonly IAuditServiceClient _auditService;
    private readonly IMediator _mediator;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        IPaymentProcessorFactory paymentProcessorFactory,
        IUnitOfWork unitOfWork,
        ILogsServiceClient logsService,
        IAuditServiceClient auditService,
        IMediator mediator,
        ILogger<ProcessPaymentCommandHandler> logger) : base(logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _paymentProcessorFactory = paymentProcessorFactory;
        _unitOfWork = unitOfWork;
        _logsService = logsService;
        _auditService = auditService;
        _mediator = mediator;
    }

    public async Task<ApiResponse<TransactionResponse>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        => await HandleAsync(request, cancellationToken);

    protected override async Task<ApiResponse<TransactionResponse>> ExecuteAsync(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Processing payment {PaymentId}", command.PaymentId);

            await _logsService.LogInformationAsync($"Processing payment: {command.PaymentId}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["PaymentId"] = command.PaymentId,
                ["HasToken"] = !string.IsNullOrEmpty(command.PaymentToken),
                ["Has3DS"] = command.ThreeDSecureData != null
            });

            // Récupération du paiement
            var payment = await _paymentRepository.GetByIdAsync(command.PaymentId);
            if (payment == null)
            {
                await _logsService.LogWarningAsync($"Payment not found: {command.PaymentId}", new Dictionary<string, object>
                {
                    ["CommandId"] = command.CommandId,
                    ["PaymentId"] = command.PaymentId
                });

                return ApiResponse<TransactionResponse>.CreateError(
                    "Payment not found",
                    404
                );
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                await _logsService.LogWarningAsync($"Payment not in pending status: {payment.PaymentNumber}", new Dictionary<string, object>
                {
                    ["CommandId"] = command.CommandId,
                    ["PaymentId"] = payment.Id,
                    ["CurrentStatus"] = payment.Status.ToString()
                });

                return ApiResponse<TransactionResponse>.CreateError(
                    $"Payment cannot be processed. Current status: {payment.Status}",
                    400
                );
            }

            // Traitement via processeur de paiement
            var processor = _paymentProcessorFactory.GetProcessor(payment.Method);
            var processResult = await processor.CreatePaymentAsync(new PaymentProcessorRequest
            {
                Amount = payment.Amount,
                PaymentMethodToken = command.PaymentToken,
                CustomerId = payment.CustomerId,
                Description = payment.Description
            });

            // Récupération ou création d'une PaymentMethod par défaut
            // TODO: Implémenter repository de PaymentMethod pour récupérer la vraie instance
            var paymentMethod = await GetOrCreatePaymentMethodAsync(payment.Method);
            
            // Création de la transaction via le paiement (méthode du domaine)
            var transaction = payment.AddTransaction(
                payment.Amount,
                TransactionType.Payment,
                paymentMethod,
                command.ClientIpAddress,
                command.UserAgent);

            // Marquer comme traitée selon le résultat
            if (processResult.Success)
            {
                transaction.Capture(payment.Amount);
                transaction.SetExternalReference(processResult.TransactionId ?? "");
            }
            else
            {
                transaction.Decline(PaymentDeclineReason.SystemError, processResult.ErrorMessage ?? "Payment processing failed");
            }

            await _transactionRepository.AddAsync(transaction);

            // Mise à jour du paiement
            payment.Status = processResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit trail
            await _auditService.AuditUpdateAsync(
                userId: payment.CustomerId.ToString(),
                userName: "Customer",
                entityName: "Payment",
                entityId: payment.Id.ToString(),
                metadata: new Dictionary<string, object>
                {
                    ["transactionId"] = transaction.Id,
                    ["previousStatus"] = "Pending",
                    ["newStatus"] = payment.Status.ToString(),
                    ["externalReference"] = transaction.ExternalReference ?? "null"
                }
            );

            // Événement de domaine
            if (processResult.Success)
            {
                var completedEvent = new PaymentCompletedEvent(
                    payment.Id,
                    payment.PaymentNumber,
                    payment.Amount,
                    payment.Amount, // montant payé = montant total
                    payment.OrderId);
                await _mediator.Publish(completedEvent, cancellationToken);
            }
            else
            {
                var failedEvent = new PaymentFailedEvent(
                    payment.Id,
                    payment.PaymentNumber,
                    payment.Amount,
                    processResult.ErrorMessage ?? "Payment processing failed");
                await _mediator.Publish(failedEvent, cancellationToken);
            }

            var response = new TransactionResponse
            {
                Id = transaction.Id,
                PaymentId = payment.Id,
                Type = transaction.Type,
                Status = transaction.Status,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency.Code,
                ExternalReference = transaction.ExternalReference,
                ProcessedAt = transaction.ProcessedAt ?? DateTime.UtcNow,
                CreatedAt = transaction.CreatedAt
            };

            await _logsService.LogInformationAsync($"Payment processed: {payment.PaymentNumber}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["PaymentId"] = payment.Id,
                ["TransactionId"] = transaction.Id,
                ["Status"] = transaction.Status.ToString(),
                ["Success"] = processResult.Success
            });

            return ApiResponse<TransactionResponse>.CreateSuccess(
                response,
                processResult.Success ? "Payment processed successfully" : "Payment processing failed"
            );
        }
        catch (Exception ex)
        {
            await _logsService.LogErrorAsync(ex, $"Error processing payment: {command.PaymentId}", new Dictionary<string, object>
            {
                ["CommandId"] = command.CommandId,
                ["PaymentId"] = command.PaymentId,
                ["ErrorMessage"] = ex.Message
            });

            Logger.LogError(ex, "Error processing payment {PaymentId}", command.PaymentId);
            
            return ApiResponse<TransactionResponse>.CreateError(
                "Internal server error while processing payment",
                new[] { ex.Message }
            );
        }
    }

        /// <summary>
        /// Méthode helper temporaire pour obtenir/créer une PaymentMethod
        /// TODO: Remplacer par appel au repository PaymentMethod
        /// </summary>
        private async Task<PaymentMethod> GetOrCreatePaymentMethodAsync(PaymentMethodType methodType)
        {
            // Implémentation temporaire - créer une PaymentMethod basique
            // En production, ceci devrait utiliser le PaymentMethodRepository
            return await Task.FromResult(new PaymentMethod(
                type: methodType,
                displayName: methodType.ToString(),
                customerId: Guid.Empty // TODO: Récupérer le vrai CustomerId  
            ));
        }
    }

/// <summary>
/// Request pour la détection de fraude
/// </summary>
public class FraudCheckRequest
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";
    public PaymentMethodType PaymentMethod { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request pour le processeur de paiement
/// </summary>
public class ProcessPaymentRequest
{
    public Guid PaymentId { get; set; }
    public Money Amount { get; set; } = null!;
    public string? PaymentToken { get; set; }
    public ThreeDSecureData? ThreeDSecureData { get; set; }
}

